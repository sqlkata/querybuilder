namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public SqlServerCompiler()
        {
            XService = new X("[", "]", "AS ");
            LastId = "SELECT scope_identity() as Id";
            EngineCode = EngineCodes.SqlServer;
        }

        public bool UseLegacyPagination { get; set; }

        public override void CompileSelectQueryInner(SqlResult ctx, Query original, Writer writer)
        {
            if (!UseLegacyPagination || !original.HasOffset(EngineCode))
            {
                base.CompileSelectQueryInner(ctx, original, writer);
                return;
            }

            var limit = original.GetLimit(EngineCode);
            var offset = original.GetOffset(EngineCode);

            var modified = ModifyQuery(original.Clone());
            writer.Append("SELECT * FROM (");
            base.CompileSelectQueryInner(ctx, modified, writer);
            writer.Append(") AS [results_wrapper] WHERE [row_num] ");
            if (limit == 0)
            {
                writer.Append(">= ?");
                ctx.BindingsAdd(offset + 1);
                writer.BindOne(offset + 1);
            }
            else
            {
                writer.Append("BETWEEN ? AND ?");
                ctx.BindingsAdd(offset + 1);
                writer.BindOne(offset + 1);
                ctx.BindingsAdd(limit + offset);
                writer.BindOne(limit + offset);
            }
            ctx.ReplaceRaw(writer);
        }

        private Query ModifyQuery(Query query)
        {
            var ctx = new SqlResult();

            if (!query.HasComponent("select")) query.Select("*");

            var writer = new Writer(XService);
            writer.Push(ctx);
            var order = CompileOrders(ctx, query, writer) ?? "ORDER BY (SELECT 0)";
            writer.AssertMatches(ctx);
            query.SelectRaw($"ROW_NUMBER() OVER ({order}) AS [row_num]", ctx.Bindings.ToArray());

            query.RemoveComponent("order");
            return query;
        }

        protected override void CompileColumns(SqlResult ctx, Query query, Writer writer)
        {
            if (!UseLegacyPagination)
            {
                base.CompileColumns(ctx, query, writer);
                return;
            }

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                // top bindings should be inserted first
                ctx.PrependOne(limit);
                writer.BindOne(limit);

                query.RemoveComponent("limit");

                // handle distinct
                if (!query.HasComponent("aggregate", EngineCode) && query.IsDistinct)
                {
                    writer.Append("SELECT DISTINCT TOP (?) ");
                    CompileFlatColumns(query, writer, ctx);
                    return;
                }

                writer.Append("SELECT TOP (?) ");
                CompileColumnsAfterSelect(ctx, query, writer);
                // writer.Append(compiled.Substring(6));
                writer.AssertMatches(ctx);
                return;

            }

            base.CompileColumns(ctx, query, writer);
        }

        protected override string? CompileLimit(SqlResult ctx, Query query, Writer writer)
        {
            if (UseLegacyPagination)
                // in legacy versions of Sql Server, limit is handled by TOP
                // and ROW_NUMBER techniques
                return null;

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return null;

            if (!query.HasComponent("order")) writer.Append("ORDER BY (SELECT 0) ");

            if (limit == 0)
            {
                ctx.BindingsAdd(offset);
                writer.BindOne(offset);
                writer.Append("OFFSET ? ROWS");
                writer.AssertMatches(ctx);
                return writer;
            }

            ctx.BindingsAdd(offset);
            writer.BindOne(offset);
            ctx.BindingsAdd(limit);
            writer.BindOne(limit);
            writer.Append("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY");
            writer.AssertMatches(ctx);
            return writer;
        }

        protected override string CompileTrue()
        {
            return "cast(1 as bit)";
        }

        protected override string CompileFalse()
        {
            return "cast(0 as bit)";
        }

        protected override void CompileBasicDateCondition(SqlResult ctx, Query query, BasicDateCondition condition,
            Writer writer)
        {
            var column = XService.Wrap(condition.Column);
            var part = condition.Part.ToUpperInvariant();

            string left;

            if (part == "TIME" || part == "DATE")
                left = $"CAST({column} AS {part.ToUpperInvariant()})";
            else
                left = $"DATEPART({part.ToUpperInvariant()}, {column})";

            var sql = $"{left} {condition.Operator} {Parameter(ctx, query, writer, condition.Value)}";

            writer.Append(condition.IsNot ? $"NOT ({sql})" : sql);
        }

        protected override SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc, Writer writer)
        {
            var colNames = string.Join(", ", adHoc.Columns.Select(value => XService.Wrap(value)));

            var valueRow = string.Join(", ", Enumerable.Repeat("?", adHoc.Columns.Length));
            var valueRows = string.Join(", ",
                Enumerable.Repeat($"({valueRow})", adHoc.Values.Length / adHoc.Columns.Length));
            var sql = $"SELECT {colNames} FROM (VALUES {valueRows}) AS tbl ({colNames})";

            var ctx = new SqlResult(adHoc.Values, sql);
            writer.BindMany(adHoc.Values);
            writer.Push(ctx);
            return ctx;
        }
    }
}
