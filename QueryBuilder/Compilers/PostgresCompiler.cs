namespace SqlKata.Compilers
{
    public class PostgresCompiler : Compiler
    {
        private static readonly string[] LikeOperators = { "starts", "ends", "contains", "like", "ilike" };

        public PostgresCompiler()
        {
            LastId = "SELECT lastval() AS id";
            EngineCode = EngineCodes.PostgreSql;
            SupportsFilterClause = true;
        }


        protected override void CompileBasicStringCondition(Query query, BasicStringCondition x,
            Writer writer)
        {
            if (Resolve(query, x.Value) is not string value)
                throw new ArgumentException("Expecting a non nullable string");

            var isLikeOperator = LikeOperators.Contains(x.Operator);

            if (x.IsNot)
                writer.Append("NOT (");
            writer.AppendName(x.Column);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(isLikeOperator
                ? x.CaseSensitive ? "LIKE" : "ILIKE" : x.Operator));
            writer.Append(" ");
            if (isLikeOperator)
            {
                switch (x.Operator)
                {
                    case "starts":
                        writer.Append("%");
                        writer.AppendParameter(query, value);
                        break;
                    case "ends":
                        writer.Append("%");
                        writer.AppendParameter(query, value);
                        break;
                    case "contains":
                        writer.Append("%");
                        writer.AppendParameter(query, value);
                        writer.Append("%");
                        break;
                    default:
                        writer.AppendParameter(query, value);
                        break;
                }
            }
            else
            {
                // This code is written as if other than "like"
                // operators are possible, but the public API
                // does not instantiate BasicStringCondition
                writer.AppendParameter(query, value);
            }
            if (x.EscapeCharacter is { } esc1)
            {
                writer.Append(" ESCAPE '");
                writer.Append(esc1);
                writer.Append('\'');
            }

            if (x.IsNot)
                writer.Append(")");
        }


        protected override void CompileBasicDateCondition(Query query, BasicDateCondition condition,
            Writer writer)
        {
            var column = XService.Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
                left = $"{column}::time";
            else if (condition.Part == "date")
                left = $"{column}::date";
            else
                left = $"DATE_PART('{condition.Part.ToUpperInvariant()}', {column})";

            var sql = $"{left} {condition.Operator} {Parameter(query, writer, condition.Value)}";

            writer.Append(condition.IsNot ? $"NOT ({sql})" : sql);
        }
    }
}
