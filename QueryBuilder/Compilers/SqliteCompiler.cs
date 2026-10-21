using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Compilers
{
    public class SqliteCompiler : Compiler
    {
        public override string EngineCode { get; } = EngineCodes.Sqlite;
        protected override string OpeningIdentifier { get; set; } = "\"";
        protected override string ClosingIdentifier { get; set; } = "\"";
        protected override string LastId { get; set; } = "select last_insert_rowid() as id";
        public override bool SupportsFilterClause { get; set; } = true;

        public override void VisitUpdateQuery(CompileContext ctx, Query query)
        {
            if (query.Joins.ForEngine(EngineCode).Any())
            {
                throw new NotSupportedException("Sqlite does not support UPDATE with JOIN clauses");
            }

            base.VisitUpdateQuery(ctx, query);
        }

        public override void VisitDeleteQuery(CompileContext ctx, Query query)
        {
            if (query.Joins.ForEngine(EngineCode).Any())
            {
                throw new NotSupportedException("Sqlite does not support DELETE with JOIN clauses");
            }

            base.VisitDeleteQuery(ctx, query);
        }

        public override string CompileTrue()
        {
            return "1";
        }

        public override string CompileFalse()
        {
            return "0";
        }

        public override void VisitLimit(CompileContext ctx, Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset > 0)
            {
                ctx.EnsureSpace();
                ctx.Write($"LIMIT -1 OFFSET {Parameter(ctx, offset)}");
                return;
            }

            base.VisitLimit(ctx, query);
        }

        protected override void VisitBasicDateCondition(CompileContext ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
            var value = Parameter(ctx, condition.Value);

            var formatMap = new Dictionary<string, string> {
                { "date", "%Y-%m-%d" },
                { "time", "%H:%M:%S" },
                { "year", "%Y" },
                { "month", "%m" },
                { "day", "%d" },
                { "hour", "%H" },
                { "minute", "%M" },
            };

            if (!formatMap.ContainsKey(condition.Part))
            {
                if (condition.IsNot)
                {
                    ctx.Write("NOT (");
                }

                ctx.Write(column).Write(" ").Write(condition.Operator).Write(" ").Write(value);

                if (condition.IsNot)
                {
                    ctx.Write(")");
                }

                return;
            }

            var sql = $"strftime('{formatMap[condition.Part]}', {column}) {condition.Operator} cast({value} as text)";

            if (condition.IsNot)
            {
                ctx.Write($"NOT ({sql})");
            }
            else
            {
                ctx.Write(sql);
            }
        }
    }
}
