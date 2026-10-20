using System;
using System.Linq;

namespace SqlKata.Compilers
{
    public class PostgresCompiler : Compiler
    {
        public PostgresCompiler()
        {
            LastId = "SELECT lastval() AS id";
        }

        public override string EngineCode { get; } = EngineCodes.PostgreSql;
        public override bool SupportsFilterClause { get; set; } = true;

        public override void VisitUpdateQuery(CompileContext ctx, Query query)
        {
            if (query.Joins.ForEngine(EngineCode).Any())
            {
                throw new NotSupportedException("PostgreSql does not support UPDATE with JOIN clauses");
            }

            base.VisitUpdateQuery(ctx, query);
        }

        public override void VisitDeleteQuery(CompileContext ctx, Query query)
        {
            if (query.Joins.ForEngine(EngineCode).Any())
            {
                throw new NotSupportedException("PostgreSql does not support DELETE with JOIN clauses");
            }

            base.VisitDeleteQuery(ctx, query);
        }

        protected override void VisitBasicDateCondition(CompileContext ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"{column}::time";
            }
            else if (condition.Part == "date")
            {
                left = $"{column}::date";
            }
            else
            {
                left = $"DATE_PART('{condition.Part.ToUpperInvariant()}', {column})";
            }

            if (condition.IsNot)
            {
                ctx.Write("NOT (");
            }

            ctx.Write(left).Write(" ").Write(condition.Operator).Write(" ").Write(Parameter(ctx, condition.Value));

            if (condition.IsNot)
            {
                ctx.Write(")");
            }
        }
    }
}
