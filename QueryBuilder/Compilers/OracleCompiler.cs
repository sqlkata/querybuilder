namespace SqlKata.Compilers
{
    public class OracleCompiler : Compiler
    {
        public OracleCompiler()
        {
            ColumnAsKeyword = "";
            TableAsKeyword = "";
            ParameterPrefix = ":p";
            MultiInsertStartClause = "INSERT ALL INTO";
            EngineCode = EngineCodes.Oracle;
            SingleRowDummyTableName = "DUAL";
        }

        public bool UseLegacyPagination { get; set; } = false;

        protected override SqlResult CompileSelectQuery(Query query)
        {
            if (!UseLegacyPagination) return base.CompileSelectQuery(query);

            var result = base.CompileSelectQuery(query);

            ApplyLegacyLimit(result);

            return result;
        }

        public override string? CompileLimit(SqlResult ctx)
        {
            if (UseLegacyPagination)
                // in pre-12c versions of Oracle, limit is handled by ROWNUM techniques
                return null;

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return null;

            var safeOrder = "";

            if (!ctx.Query.HasComponent("order")) safeOrder = "ORDER BY (SELECT 0 FROM DUAL) ";

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return $"{safeOrder}OFFSET {ParameterPlaceholder} ROWS";
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);

            return $"{safeOrder}OFFSET {ParameterPlaceholder} ROWS FETCH NEXT {ParameterPlaceholder} ROWS ONLY";
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
                    $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\") WHERE \"row_num\" > {ParameterPlaceholder}";
                ctx.Bindings.Add(offset);
            }
            else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= {ParameterPlaceholder}";
                ctx.Bindings.Add(limit);
            }
            else
            {
                newSql =
                    $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\" WHERE ROWNUM <= {ParameterPlaceholder}) WHERE \"row_num\" > {ParameterPlaceholder}";
                ctx.Bindings.Add(limit + offset);
                ctx.Bindings.Add(offset);
            }

            ctx.RawSql = newSql;
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
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
                var columns = GetInsertColumnsList(insert.Columns);
                var values = string.Join(", ", Parametrize(ctx, insert.Values));

                var intoFormat = " INTO {0}{1} VALUES ({2})";
                var nextInsert = string.Format(intoFormat, table, columns, values);

                ctx.RawSql += nextInsert;
            }

            ctx.RawSql += " SELECT 1 FROM DUAL";
            return ctx;
        }
    }
}
