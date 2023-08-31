using System.Diagnostics;

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
                writer.Append(">= ");
                writer.AppendParameter(offset + 1);
                ctx.BindingsAdd(offset + 1);
            }
            else
            {
                writer.Append("BETWEEN ");
                writer.AppendParameter(offset + 1);
                writer.Append(" AND ");
                writer.AppendParameter(limit + offset);
                ctx.BindingsAdd(offset + 1);
                ctx.BindingsAdd(limit + offset);
            }
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
                ctx.BindingsAdd(limit);

                query.RemoveComponent("limit");

                // handle distinct
                if (!query.HasComponent("aggregate", EngineCode) && query.IsDistinct)
                {
                    writer.Append("SELECT DISTINCT TOP (");
                    writer.AppendParameter(limit);
                    writer.Append(") ");
                    CompileFlatColumns(query, writer, ctx);
                    return;
                }

                writer.Append("SELECT TOP (");
                writer.AppendParameter(limit);
                writer.Append(") ");
                CompileColumnsAfterSelect(ctx, query, writer);
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
                writer.Append("OFFSET ");
                writer.AppendParameter(offset);
                writer.Append(" ROWS");
                writer.AssertMatches(ctx);
                return writer;
            }

            ctx.BindingsAdd(offset);
            ctx.BindingsAdd(limit);
            writer.Append("OFFSET ");
            writer.AppendParameter(offset);
            writer.Append(" ROWS FETCH NEXT ");
            writer.AppendParameter(limit);
            writer.Append(" ROWS ONLY");
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

        protected override void CompileBasicDateCondition(
            SqlResult ctx, Query query,
            BasicDateCondition condition, Writer writer)
        {
            var part = condition.Part.ToUpperInvariant();

            if (condition.IsNot)
                writer.Append("NOT (");

            if (part is "TIME" or "DATE")
            {
                writer.Append("CAST(");
                writer.AppendName(condition.Column);
                writer.Append(" AS ");
                writer.AppendKeyword(part);
                writer.Append(")");
            }
            else
            {
                writer.Append("DATEPART(");
                writer.AppendKeyword(part);
                writer.Append(", ");
                writer.AppendName(condition.Column);
                writer.Append(")");
            }

            writer.Append(" ");
            writer.Append(condition.Operator);
            writer.Append(" ");
            writer.AppendParameter(ctx, query, condition.Value);
            if (condition.IsNot)
                writer.Append(")");
        }

        protected override void CompileAdHocQuery(AdHocTableFromClause adHoc, Writer writer)
        {
            Debug.Assert(adHoc.Alias != null, "adHoc.Alias != null");
            writer.AppendValue(adHoc.Alias);
            writer.Append(" AS (SELECT ");
            writer.WriteInsertColumnsList(adHoc.Columns, false);
            writer.Append(" FROM (VALUES ");
            writer.List(", ", adHoc.Values.Length / adHoc.Columns.Length, _ =>
            {
                writer.Append("(");
                writer.List(", ", adHoc.Columns.Length, _ =>
                {
                    writer.Append("?");
                });
                writer.Append(")");
            });
            writer.BindMany(adHoc.Values);
            writer.Append(") AS tbl");
            writer.WriteInsertColumnsList(adHoc.Columns);
            writer.Append(")");
        }
    }
}
