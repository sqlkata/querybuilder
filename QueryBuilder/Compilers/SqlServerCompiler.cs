using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public SqlServerCompiler()
        {
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
            LastId = "SELECT scope_identity() as Id";
        }

        public override string EngineCode { get; } = EngineCodes.SqlServer;
        public bool UseLegacyPagination { get; set; } = false;

        public override void VisitSelectQuery(CompileContext ctx, Query query)
        {
            if (!UseLegacyPagination || !query.HasOffset(EngineCode))
            {
                base.VisitSelectQuery(ctx, query);
                return;
            }

            query = query.Clone();

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (!query.SelectClauses.Any())
            {
                query.Select("*");
            }

            ctx.PushQuery(query);
            var beforeOrder = ctx.Buffer.Length;
            var beforeOrderBindings = ctx.Bindings.Count;
            VisitOrders(ctx, query);
            var order = ctx.Buffer.Length > beforeOrder ? ctx.Buffer.ToString(beforeOrder, ctx.Buffer.Length - beforeOrder).TrimStart() : "ORDER BY (SELECT 0)";
            var orderBindings = ctx.Bindings.Skip(beforeOrderBindings).ToArray();
            ctx.Buffer.Length = beforeOrder;
            ctx.Bindings.RemoveRange(beforeOrderBindings, orderBindings.Length);
            ctx.PopQuery();

            query.SelectRaw($"ROW_NUMBER() OVER ({order}) AS [row_num]", orderBindings);

            query.OrderByClauses.Clear();

            ctx.PushQuery(query);
            var beforeInner = ctx.Buffer.Length;
            try
            {
                base.VisitSelectQuery(ctx, query);
            }
            finally
            {
                ctx.PopQuery();
            }

            var innerSql = ctx.Buffer.ToString(beforeInner, ctx.Buffer.Length - beforeInner);
            ctx.Buffer.Length = beforeInner;

            if (limit == 0)
            {
                ctx.Write($"SELECT * FROM ({innerSql}) AS [results_wrapper] WHERE [row_num] >= {Parameter(ctx, offset + 1)}");
                return;
            }

            var p1 = Parameter(ctx, offset + 1);
            var p2 = Parameter(ctx, limit + offset);
            ctx.Write($"SELECT * FROM ({innerSql}) AS [results_wrapper] WHERE [row_num] BETWEEN {p1} AND {p2}");
        }

        public override void VisitSelectPrefix(CompileContext ctx, Query query)
        {
            if (!UseLegacyPagination)
            {
                base.VisitSelectPrefix(ctx, query);
                return;
            }

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                var p = Parameter(ctx, limit);
                var distinct = query.IsDistinct ? "DISTINCT " : "";
                ctx.Write($"SELECT {distinct}TOP ({p}) ");
                return;
            }

            base.VisitSelectPrefix(ctx, query);
        }

        public override void VisitLimit(CompileContext ctx, Query query)
        {
            if (UseLegacyPagination)
            {
                return;
            }

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return;
            }

            ctx.EnsureSpace();

            if (!query.OrderByClauses.Any())
            {
                ctx.Write("ORDER BY (SELECT 0) ");
            }

            if (limit == 0)
            {
                ctx.Write($"OFFSET {Parameter(ctx, offset)} ROWS");
                return;
            }

            ctx.Write($"OFFSET {Parameter(ctx, offset)} ROWS FETCH NEXT {Parameter(ctx, limit)} ROWS ONLY");
        }

        public override string CompileRandom(int? seed)
        {
            return "NEWID()";
        }

        public override string CompileTrue()
        {
            return "cast(1 as bit)";
        }

        public override string CompileFalse()
        {
            return "cast(0 as bit)";
        }

        protected override void VisitBasicDateCondition(CompileContext ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
            var part = condition.Part.ToUpperInvariant();

            string left;

            if (part == "TIME" || part == "DATE")
            {
                left = $"CAST({column} AS {part.ToUpperInvariant()})";
            }
            else
            {
                left = $"DATEPART({part.ToUpperInvariant()}, {column})";
            }

            if (condition.IsNot)
            {
                ctx.Write("NOT (");
            }

            ctx.Write(left).Write(" ").Write(condition.Operator).Write(" ").Write(Parameter(ctx, condition.Value));

            if (condition.IsNot)
            {
                ctx.Write(")");
            }
        }

        public override void VisitAdHocTableFromClause(CompileContext ctx, AdHocTableFromClause adHoc)
        {
            var colNames = string.Join(", ", adHoc.Columns.Select(Wrap));
            var colCount = adHoc.Columns.Count;
            var rowCount = adHoc.Values.Count / colCount;
            var rows = new List<string>(rowCount);

            for (var i = 0; i < rowCount; i++)
            {
                var rowValues = adHoc.Values.Skip(i * colCount).Take(colCount);
                rows.Add($"({string.Join(", ", rowValues.Select(v => Parameter(ctx, v)))})");
            }

            var valueRows = string.Join(", ", rows);
            ctx.Write($"SELECT {colNames} FROM (VALUES {valueRows}) AS tbl ({colNames})");
        }
    }
}
