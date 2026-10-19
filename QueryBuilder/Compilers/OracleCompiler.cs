using System;
using System.Collections.Generic;
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
            MultiInsertStartClause = "INSERT ALL INTO";
        }

        public override string EngineCode { get; } = EngineCodes.Oracle;
        public bool UseLegacyPagination { get; set; } = false;
        protected override string SingleRowDummyTableName => "DUAL";

        public override void VisitUpdateQuery(CompileContext ctx, Query query)
        {
            if (query.Joins.ForEngine(EngineCode).Any())
            {
                throw new NotSupportedException("Oracle does not support UPDATE with JOIN clauses");
            }

            base.VisitUpdateQuery(ctx, query);
        }

        public override void VisitDeleteQuery(CompileContext ctx, Query query)
        {
            if (query.Joins.ForEngine(EngineCode).Any())
            {
                throw new NotSupportedException("Oracle does not support DELETE with JOIN clauses");
            }

            base.VisitDeleteQuery(ctx, query);
        }

        public override void VisitSelectQuery(CompileContext ctx, Query query)
        {
            if (!UseLegacyPagination)
            {
                base.VisitSelectQuery(ctx, query);
                return;
            }

            var beforeInner = ctx.Buffer.Length;
            base.VisitSelectQuery(ctx, query);
            var innerSql = ctx.Buffer.ToString(beforeInner, ctx.Buffer.Length - beforeInner);
            ctx.Buffer.Length = beforeInner;

            ctx.Write(ApplyLegacyLimit(ctx, innerSql));
        }

        public override void VisitLimit(CompileContext ctx, Query query)
        {
            if (UseLegacyPagination)
            {
                return;
            }

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return;
            }

            ctx.EnsureSpace();

            if (!query.OrderByClauses.Any())
            {
                ctx.Write("ORDER BY (SELECT 0 FROM DUAL) ");
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                ctx.Write($"OFFSET {parameterPlaceholder} ROWS");
                return;
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);

            ctx.Write($"OFFSET {parameterPlaceholder} ROWS FETCH NEXT {parameterPlaceholder} ROWS ONLY");
        }

        internal void ApplyLegacyLimit(CompileContext ctx)
        {
            ctx.RawSql = ApplyLegacyLimit(ctx, ctx.RawSql);
        }

        internal string ApplyLegacyLimit(CompileContext ctx, string sql)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return sql;
            }

            string newSql;
            if (limit == 0)
            {
                newSql = $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({sql}) \"results_wrapper\") WHERE \"row_num\" > {parameterPlaceholder}";
                ctx.Bindings.Add(offset);
            }
            else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({sql}) WHERE ROWNUM <= {parameterPlaceholder}";
                ctx.Bindings.Add(limit);
            }
            else
            {
                newSql = $"SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM ({sql}) \"results_wrapper\" WHERE ROWNUM <= {parameterPlaceholder}) WHERE \"row_num\" > {parameterPlaceholder}";
                ctx.Bindings.Add(limit + offset);
                ctx.Bindings.Add(offset);
            }

            return newSql;
        }

        protected override void VisitBasicDateCondition(CompileContext ctx, BasicDateCondition condition)
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
                        if (condition.Value.ToString().Split(':').Count() == 2)
                            valueFormat = $"TO_DATE({value}, 'HH24:MI')";
                        else
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
                ctx.Write("NOT (");
            }

            ctx.Write(sql);

            if (condition.IsNot)
            {
                ctx.Write(")");
            }
        }

        public override void VisitRemainingInsertClauses(
            CompileContext ctx, string table, IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                string columns = GetInsertColumnsList(insert.Columns);
                string values = string.Join(", ", Parameterize(ctx, insert.Values));

                string intoFormat = " INTO {0}{1} VALUES ({2})";
                ctx.Write(string.Format(intoFormat, table, columns, values));
            }

            ctx.Write(" SELECT 1 FROM DUAL");
        }

        public override string CompileRandom(int? seed)
        {
            return "DBMS_RANDOM.VALUE";
        }
    }
}
