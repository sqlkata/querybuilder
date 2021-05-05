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

            var result = base.CompileSelectQuery(query);

            ApplyLegacyLimit(result);

            return result;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            if (UseLegacyPagination)
            {
                // in pre-12c versions of Oracle, limit is handled by ROWNUM techniques
                return null;
            }

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            var safeOrder = "";

            if (!ctx.Query.HasComponent("order"))
            {
                safeOrder = "ORDER BY (SELECT 0 FROM DUAL) ";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return $"{safeOrder}OFFSET ? ROWS";
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);

            return $"{safeOrder}OFFSET ? ROWS FETCH NEXT ? ROWS ONLY";
        }

        internal void ApplyLegacyLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return;
            }

            string newSql;
            if (limit == 0)
            {
                newSql = $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\") WHERE \"row_num\" > ?";
                ctx.Bindings.Add(offset);
            }
            else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= ?";
                ctx.Bindings.Add(limit);
            }
            else
            {
                newSql = $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({ctx.RawSql}) \"results_wrapper\" WHERE ROWNUM <= ?) WHERE \"row_num\" > ?";
                ctx.Bindings.Add(limit + offset);
                ctx.Bindings.Add(offset);
            }

            ctx.RawSql = newSql;
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {

            var column = Wrap(condition.Column);
            var value = Parameter(ctx, condition.Value);

            var sql = "";
            var valueFormat = "";

            var isDateTime = (condition.Value is DateTime dt);

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
