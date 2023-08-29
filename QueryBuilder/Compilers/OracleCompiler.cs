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

        public bool UseLegacyPagination { get; set; } = false;

        public override SqlResult CompileSelectQuery(Query query, Writer writer)
        {
            if (!UseLegacyPagination) return base.CompileSelectQuery(query, writer);

            var result = base.CompileSelectQuery(query, writer);

            ApplyLegacyLimit(result);

            return result;
        }

        protected override string? CompileLimit(SqlResult ctx, Writer writer)
        {
            if (UseLegacyPagination)
                // in pre-12c versions of Oracle,
                // limit is handled by ROWNUM techniques
                return null;

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return null;

            if (!ctx.Query.HasComponent("order"))
            {
                writer.S.Append("ORDER BY (SELECT 0 FROM DUAL) ");
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                writer.S.Append("OFFSET ? ROWS");
                return writer;
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);

            writer.S.Append("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY");
            return writer;
        }

        internal void ApplyLegacyLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return;

            string newSql;
            if (limit == 0)
            {
                newSql =
                    $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\") WHERE \"row_num\" > ?";
                ctx.Bindings.Add(offset);
            }
            else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= ?";
                ctx.Bindings.Add(limit);
            }
            else
            {
                newSql =
                    $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\" WHERE ROWNUM <= ?) WHERE \"row_num\" > ?";
                ctx.Bindings.Add(limit + offset);
                ctx.Bindings.Add(offset);
            }

            ctx.ReplaceRaw(newSql);
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = XService.Wrap(condition.Column);
            var value = Parameter(ctx, condition.Value);

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
                        valueFormat = condition.Value.ToString()!.Split(':').Count() == 2 ? $"TO_DATE({value}, 'HH24:MI')" :
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

            if (condition.IsNot) return $"NOT ({sql})";

            return sql;
        }

        protected override SqlResult CompileRemainingInsertClauses(
            SqlResult ctx, string table, IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                var columns = insert.Columns.GetInsertColumnsList(XService);
                var values = string.Join(", ", Parametrize(ctx, insert.Values));

                var intoFormat = " INTO {0}{1} VALUES ({2})";
                var nextInsert = string.Format(intoFormat, table, columns, values);

                ctx.Raw.Append(nextInsert);
            }

            ctx.Raw.Append(" SELECT 1 FROM DUAL");
            return ctx;
        }
    }
}
