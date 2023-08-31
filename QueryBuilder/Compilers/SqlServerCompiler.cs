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

        public bool UseLegacyPagination { get; init; }

        protected override void CompileSelectQuery(Query original, Writer writer)
        {
            if (!UseLegacyPagination || !original.HasOffset(EngineCode))
            {
                base.CompileSelectQuery(original, writer);
                return;
            }

            var limit = original.GetLimit(EngineCode);
            var offset = original.GetOffset(EngineCode);

            var modified = ModifyQuery(original.Clone());
            writer.Append("SELECT * FROM (");
            base.CompileSelectQuery(modified, writer);
            writer.Append(") AS [results_wrapper] WHERE [row_num] ");
            if (limit == 0)
            {
                writer.Append(">= ");
                writer.AppendParameter(offset + 1);
            }
            else
            {
                writer.Append("BETWEEN ");
                writer.AppendParameter(offset + 1);
                writer.Append(" AND ");
                writer.AppendParameter(limit + offset);
            }
        }

        private Query ModifyQuery(Query query)
        {
            if (!query.HasComponent("select")) query.Select("*");

            var writer = new Writer(XService);
            var order = CompileOrders(query, writer) ?? "ORDER BY (SELECT 0)";
            query.SelectRaw($"ROW_NUMBER() OVER ({order}) AS [row_num]");

            query.RemoveComponent("order");
            return query;
        }

        protected override void CompileColumns(Query query, Writer writer)
        {
            if (!UseLegacyPagination)
            {
                base.CompileColumns(query, writer);
                return;
            }

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                query.RemoveComponent("limit");

                // handle distinct
                if (!query.HasComponent("aggregate", EngineCode) && query.IsDistinct)
                {
                    writer.Append("SELECT DISTINCT TOP (");
                    writer.AppendParameter(limit);
                    writer.Append(") ");
                    CompileFlatColumns(query, writer);
                    return;
                }

                writer.Append("SELECT TOP (");
                writer.AppendParameter(limit);
                writer.Append(") ");
                CompileColumnsAfterSelect(query, writer);
                return;
            }

            base.CompileColumns(query, writer);
        }

        protected override string? CompileLimit(Query query, Writer writer)
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
                writer.Append("OFFSET ");
                writer.AppendParameter(offset);
                writer.Append(" ROWS");
                return writer;
            }

            writer.Append("OFFSET ");
            writer.AppendParameter(offset);
            writer.Append(" ROWS FETCH NEXT ");
            writer.AppendParameter(limit);
            writer.Append(" ROWS ONLY");
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

        protected override void CompileBasicDateCondition(Query query,
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
                writer.Append(part);
                writer.Append(")");
            }
            else
            {
                writer.Append("DATEPART(");
                writer.Append(part);
                writer.Append(", ");
                writer.AppendName(condition.Column);
                writer.Append(")");
            }

            writer.Append(" ");
            writer.Append(condition.Operator);
            writer.Append(" ");
            writer.AppendParameter(query, condition.Value);
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
