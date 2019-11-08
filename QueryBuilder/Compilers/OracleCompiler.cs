using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SqlKata.Compilers
{
    public class OracleCompiler : Compiler
    {
        public OracleCompiler()
        {
            ColumnAsKeyword = "";
            TableAsKeyword = "";
            parameterPrefix = ":p";
        }

        public override string EngineCode { get; } = EngineCodes.Oracle;
        public bool UseLegacyPagination { get; set; } = false;

        protected override SqlResult CompileSelectQuery(Query query)
        {
            if (!UseLegacyPagination)
            {
                return base.CompileSelectQuery(query);
            }

            SqlResult result = base.CompileSelectQuery(query);

            ApplyLegacyLimit(result);

            return result;
        }

        public override string CompileLimit(SqlResult context)
        {
            if (UseLegacyPagination)
            {
                // in pre-12c versions of Oracle, limit is handled by ROWNUM techniques
                return null;
            }

            int limit = context.Query.GetLimit(EngineCode);
            int offset = context.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            string safeOrder = "";

            if (!context.Query.HasComponent("order"))
            {
                safeOrder = "ORDER BY (SELECT 0 FROM DUAL) ";
            }

            if (limit == 0)
            {
                context.Bindings.Add(offset);
                return $"{safeOrder}OFFSET ? ROWS";
            }

            context.Bindings.Add(offset);
            context.Bindings.Add(limit);

            return $"{safeOrder}OFFSET ? ROWS FETCH NEXT ? ROWS ONLY";
        }

        internal void ApplyLegacyLimit(SqlResult context)
        {
            int limit = context.Query.GetLimit(EngineCode);
            int offset = context.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return;
            }

            string newSql;
            if (limit == 0)
            {
                newSql = $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({context.RawSql}) \"results_wrapper\") WHERE \"row_num\" > ?";
                context.Bindings.Add(offset);
            }
            else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({context.RawSql}) WHERE ROWNUM <= ?";
                context.Bindings.Add(limit);
            }
            else
            {
                newSql = $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({context.RawSql}) \"results_wrapper\" WHERE ROWNUM <= ?) WHERE \"row_num\" > ?";
                context.Bindings.Add(limit + offset);
                context.Bindings.Add(offset);
            }

            context.RawSql = newSql;
        }

        protected override string CompileBasicDateCondition(SqlResult context, BasicDateCondition condition)
        {

            string column = Wrap(condition.Column);
            string value = Parameter(context, condition.Value);

            string sql = "";
            string valueFormat = "";

            bool isDateTime = (condition.Value is DateTime dt);

            switch (condition.Part)
            {
                case "date": // assume YY-MM-DD format
                    if (isDateTime)
                        valueFormat = $"{value}";
                    else
                        valueFormat = $"TO_DATE({value}, 'YY-MM-DD')";
                    sql = $"TO_CHAR({column}, 'YY-MM-DD') {condition.Operator} TO_CHAR({valueFormat}, 'YY-MM-DD')";
                    break;
                case "time":
                    if (isDateTime)
                        valueFormat = $"{value}";
                    else
                    {
                        // assume HH:MM format
                        if (condition.Value.ToString().Split(':').Count() == 2)
                            valueFormat = $"TO_DATE({value}, 'HH24:MI')";
                        else // assume HH:MM:SS format
                            valueFormat = $"TO_DATE({value}, 'HH24:MI:SS')";
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

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;

        }
    }
}
