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

        protected override void CompileBasicDateCondition(SqlResult ctx, Query query, BasicDateCondition condition,
            Writer writer)
        {
            var column = XService.Wrap(condition.Column);
            var value = Parameter(ctx, query, writer, condition.Value);

            string sql;
            string valueFormat;

            var isDateTime = condition.Value is DateTime;

            switch (condition.Part)
            {
                case "date": // assume YY-MM-DD format
                    valueFormat = isDateTime ? $"{value}" : $"TO_DATE({value}, 'YY-MM-DD')";
                    sql = $"TO_CHAR({column}, 'YY-MM-DD') {condition.Operator} TO_CHAR({valueFormat}, 'YY-MM-DD')";
                    break;
                case "time":
                    if (isDateTime)
                    {
                        valueFormat = $"{value}";
                    }
                    else
                    {
                        // assume HH:MM format
                        valueFormat = condition.Value.ToString()!.Split(':').Length == 2 ? $"TO_DATE({value}, 'HH24:MI')" :
                            // assume HH:MM:SS format
                            $"TO_DATE({value}, 'HH24:MI:SS')";
                    }

                    sql = $"TO_CHAR({column}, 'HH24:MI:SS') {condition.Operator} TO_CHAR({valueFormat}, 'HH24:MI:SS')";
                    break;
                case "year":
                case "month":
                case "day":
                case "hour":
                case "minute":
                case "second":
                    sql = $"EXTRACT({condition.Part.ToUpperInvariant()} FROM {column}) {condition.Operator} {value}";
                    break;
                default:
                    sql = $"{column} {condition.Operator} {value}";
                    break;
            }

            writer.Append(condition.IsNot ? $"NOT ({sql})" : sql);
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
                writer.List(", ", insert.Values, value =>
                {
                    writer.Append(Parameter(ctx, query, writer, value));
                });
                writer.Append(")");
            }
            writer.Append(" SELECT 1 FROM DUAL");
        }
    }
}
