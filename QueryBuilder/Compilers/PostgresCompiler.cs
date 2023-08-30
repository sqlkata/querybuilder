namespace SqlKata.Compilers
{
    public class PostgresCompiler : Compiler
    {
        private static readonly string[] LikeOperators = { "starts", "ends", "contains", "like", "ilike" };

        public PostgresCompiler()
        {
            LastId = "SELECT lastval() AS id";
            EngineCode = EngineCodes.PostgreSql;
            SupportsFilterClause  = true;
        }


        protected override void CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x, Writer writer)
        {
            var column = XService.Wrap(x.Column);

            if (Resolve(ctx, x.Value) is not string value)
                throw new ArgumentException("Expecting a non nullable string");

            var method = x.Operator;

            if (LikeOperators.Contains(x.Operator))
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
                writer.S.Append("NOT (");
            writer.S.Append(column);
            writer.S.Append(" ");
            writer.S.Append(Operators.CheckOperator(method));
            writer.S.Append(" ");
            writer.S.Append(x.Value is UnsafeLiteral ? value : Parameter(ctx, writer, value));
            if (x.EscapeCharacter is { } esc1)
            {
                writer.S.Append(" ESCAPE '");
                writer.S.Append(esc1);
                writer.S.Append('\'');
            }

            if (x.IsNot)
                writer.S.Append(")");
        }


        protected override void CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition, Writer writer)
        {
            var column = XService.Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
                left = $"{column}::time";
            else if (condition.Part == "date")
                left = $"{column}::date";
            else
                left = $"DATE_PART('{condition.Part.ToUpperInvariant()}', {column})";

            var sql = $"{left} {condition.Operator} {Parameter(ctx, writer, condition.Value)}";

            writer.S.Append(condition.IsNot ? $"NOT ({sql})" :sql);
        }
    }
}
