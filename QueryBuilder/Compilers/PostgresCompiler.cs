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

        protected override void VisitBasicStringCondition(CompileContext ctx, BasicStringCondition x)
        {
            var column = Wrap(x.Column);

            var value = Resolve(ctx, x.Value) as string;

            if (value == null)
            {
                throw new ArgumentException("Expecting a non nullable string");
            }

            var method = x.Operator;

            if (new[] { "starts", "ends", "contains", "like", "ilike" }.Contains(x.Operator))
            {
                method = x.CaseSensitive ? "LIKE" : "ILIKE";

                switch (x.Operator)
                {
                    case "starts":
                        value = $"{value}%";
                        break;
                    case "ends":
                        value = $"%{value}";
                        break;
                    case "contains":
                        value = $"%{value}%";
                        break;
                }
            }

            if (x.IsNot)
            {
                ctx.Write("NOT (");
            }

            if (x.Value is UnsafeLiteral)
            {
                ctx.Write(column).Write(" ").Write(checkOperator(method)).Write(" ").Write(value);
            }
            else
            {
                ctx.Write(column).Write(" ").Write(checkOperator(method)).Write(" ").Write(Parameter(ctx, value));
            }

            if (!string.IsNullOrEmpty(x.EscapeCharacter))
            {
                ctx.Write(" ESCAPE '").Write(x.EscapeCharacter).Write("'");
            }

            if (x.IsNot)
            {
                ctx.Write(")");
            }
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
