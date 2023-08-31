namespace SqlKata.Compilers
{
    public class OracleCompiler : Compiler
    {
        public OracleCompiler()
        {
            XService = new("\"", "\"", "");
            TableAsKeyword = "";
            ParameterPrefix = ":p";
            MultiInsertStartClause = "INSERT ALL INTO";
            EngineCode = EngineCodes.Oracle;
            SingleRowDummyTableName = "DUAL";
        }

        public bool UseLegacyPagination { get; init; }

        protected override void CompileSelectQuery(Query query, Writer writer)
        {
            if (!UseLegacyPagination)
            {
                base.CompileSelectQuery(query, writer);
                return;
            }

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                base.CompileSelectQuery(query, writer);
                return;
            }

            if (limit == 0)
            {
                writer.Append("""SELECT * FROM (SELECT "results_wrapper".*, ROWNUM "row_num" FROM (""");
                base.CompileSelectQuery(query, writer);
                writer.Append(""") "results_wrapper") WHERE "row_num" > """);
                writer.AppendParameter(offset);
            }
            else if (offset == 0)
            {
                writer.Append("""SELECT * FROM (""");
                base.CompileSelectQuery(query, writer);
                writer.Append(""") WHERE ROWNUM <= """);
                writer.AppendParameter(limit);
            }
            else
            {
                writer.Append("""SELECT * FROM (SELECT "results_wrapper".*, ROWNUM "row_num" FROM (""");
                base.CompileSelectQuery(query, writer);
                writer.Append(""") "results_wrapper" WHERE ROWNUM <= """);
                writer.AppendParameter(limit + offset);
                writer.Append(""") WHERE "row_num" > """);
                writer.AppendParameter(offset);
            }
        }

        protected override string? CompileLimit(Query query, Writer writer)
        {
            if (UseLegacyPagination)
                // in pre-12c versions of Oracle,
                // limit is handled by ROWNUM techniques
                return null;

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return null;

            if (!query.HasComponent("order"))
            {
                writer.Append("ORDER BY (SELECT 0 FROM DUAL) ");
            }

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

        protected override void CompileBasicDateCondition(Query query, BasicDateCondition condition, Writer writer)
        {
            var isDateTime = condition.Value is DateTime;

            if (condition.IsNot)
                writer.Append("NOT (");
            switch (condition.Part)
            {
                case "date": // assume YY-MM-DD format
                    writer.Append("TO_CHAR(");
                    writer.AppendName(condition.Column);
                    writer.Append(", 'YY-MM-DD') ");
                    writer.Append(condition.Operator);
                    writer.Append(" TO_CHAR(");
                    if (isDateTime)
                    {
                        writer.AppendParameter(query, condition.Value);
                    }
                    else
                    {
                        writer.Append("TO_DATE(");
                        writer.AppendParameter(query, condition.Value);
                        writer.Append(", 'YY-MM-DD')");
                    }
                    writer.Append(", 'YY-MM-DD')");
                    break;
                case "time":
                    if (isDateTime)
                    {
                        writer.Append("TO_CHAR(");
                        writer.AppendName(condition.Column);
                        writer.Append(", 'HH24:MI:SS') ");
                        writer.Append(condition.Operator);
                        writer.Append(" TO_CHAR(");
                        writer.AppendParameter(query, condition.Value);
                        writer.Append(", 'HH24:MI:SS')");
                    }
                    else
                    {
                        writer.Append("TO_CHAR(");
                        writer.AppendName(condition.Column);
                        writer.Append(", 'HH24:MI:SS') ");
                        writer.Append(condition.Operator);
                        writer.Append(" TO_CHAR(");
                        writer.Append("TO_DATE(");
                        writer.AppendParameter(query, condition.Value);
                        var isHhSs = condition.Value.ToString()!
                            .Split(':').Length == 2;
                        writer.Append(isHhSs ? ", 'HH24:MI')" : ", 'HH24:MI:SS')");
                        writer.Append(", 'HH24:MI:SS')");
                    }
                    break;
                case "year":
                case "month":
                case "day":
                case "hour":
                case "minute":
                case "second":
                    writer.Append("EXTRACT(");
                    writer.AppendKeyword(condition.Part);
                    writer.Append(" FROM ");
                    writer.AppendName(condition.Column);
                    writer.Append(") ");
                    writer.Append(condition.Operator);
                    writer.Append(" ");
                    writer.AppendParameter(query, condition.Value);
                    break;
                default:
                    writer.AppendName(condition.Column);
                    writer.Append(" ");
                    writer.Append(condition.Operator);
                    writer.Append(" ");
                    writer.AppendParameter(query, condition.Value);
                    break;
            }
            if (condition.IsNot)
                writer.Append(")");
        }

        protected override void CompileRemainingInsertClauses(Query query, string table,
            Writer writer,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                writer.Append(" INTO ");
                writer.Append(table);
                writer.WriteInsertColumnsList(insert.Columns);
                writer.Append(" VALUES (");
                writer.CommaSeparatedParameters(query, insert.Values);
                writer.Append(")");
            }
            writer.Append(" SELECT 1 FROM DUAL");
        }
    }
}
