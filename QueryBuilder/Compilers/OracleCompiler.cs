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

        public bool UseLegacyPagination { get; set; }

        public override void CompileSelectQueryInner(SqlResult ctx, Query query, Writer writer)
        {
            if (!UseLegacyPagination)
            {
                base.CompileSelectQueryInner(ctx, query, writer);
                return;
            }

            base.CompileSelectQueryInner(ctx, query, writer);

            ApplyLegacyLimit(ctx, query);

        }

        protected override string? CompileLimit(SqlResult ctx, Query query, Writer writer)
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
                ctx.BindingsAdd(offset);
                writer.Append("OFFSET ");
                writer.AppendParameter(offset);
                writer.Append(" ROWS");
                return writer;
            }

            ctx.BindingsAdd(offset);
            ctx.BindingsAdd(limit);

            writer.Append("OFFSET ");
            writer.AppendParameter(offset);
            writer.Append(" ROWS FETCH NEXT ");
            writer.AppendParameter(limit);
            writer.Append(" ROWS ONLY");
            return writer;
        }

        internal void ApplyLegacyLimit(SqlResult ctx, Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return;

            string newSql;
            if (limit == 0)
            {
                newSql =
                    $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\") WHERE \"row_num\" > ?";
                ctx.BindingsAdd(offset);
            }
            else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= ?";
                ctx.BindingsAdd(limit);
            }
            else
            {
                newSql =
                    $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\" WHERE ROWNUM <= ?) WHERE \"row_num\" > ?";
                ctx.BindingsAdd(limit + offset);
                ctx.BindingsAdd(offset);
            }

            ctx.ReplaceRaw(newSql);
        }

        protected override void CompileBasicDateCondition(SqlResult ctx,
            Query query, BasicDateCondition condition, Writer writer)
        {
            var column = XService.Wrap(condition.Column);
            var isDateTime = condition.Value is DateTime;

            if (condition.IsNot)
                writer.Append("NOT (");
            switch (condition.Part)
            {
                case "date": // assume YY-MM-DD format
                    writer.Append("TO_CHAR(");
                    writer.Append(column);
                    writer.Append(", 'YY-MM-DD') ");
                    writer.Append(condition.Operator);
                    writer.Append(" TO_CHAR(");
                    if (isDateTime)
                    {
                        writer.AppendParameter(ctx, query, condition.Value);
                    }
                    else
                    {
                        writer.Append("TO_DATE(");
                        writer.AppendParameter(ctx, query, condition.Value);
                        writer.Append(", 'YY-MM-DD')");
                    }
                    writer.Append(", 'YY-MM-DD')");
                    break;
                case "time":
                    if (isDateTime)
                    {
                        writer.Append("TO_CHAR(");
                        writer.Append(column);
                        writer.Append(", 'HH24:MI:SS') ");
                        writer.Append(condition.Operator);
                        writer.Append(" TO_CHAR(");
                        writer.AppendParameter(ctx, query, condition.Value);
                        writer.Append(", 'HH24:MI:SS')");
                    }
                    else
                    {
                        writer.Append("TO_CHAR(");
                        writer.Append(column);
                        writer.Append(", 'HH24:MI:SS') ");
                        writer.Append(condition.Operator);
                        writer.Append(" TO_CHAR(");
                        writer.Append("TO_DATE(");
                        writer.AppendParameter(ctx, query, condition.Value);
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
                    writer.Append(column);
                    writer.Append(") ");
                    writer.Append(condition.Operator);
                    writer.Append(" ");
                    writer.AppendParameter(ctx, query, condition.Value);
                    break;
                default:
                    writer.Append(column);
                    writer.Append(" ");
                    writer.Append(condition.Operator);
                    writer.Append(" ");
                    writer.AppendParameter(ctx, query, condition.Value);
                    break;
            }
            if (condition.IsNot)
                writer.Append(")");
        }

        protected override void CompileRemainingInsertClauses(SqlResult ctx, Query query, string table,
            Writer writer,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                writer.Append(" INTO ");
                writer.Append(table);
                writer.WriteInsertColumnsList(insert.Columns);
                writer.Append(" VALUES (");
                writer.CommaSeparatedParameters(ctx, query, insert.Values);
                writer.Append(")");
            }
            writer.Append(" SELECT 1 FROM DUAL");
        }
    }
}
