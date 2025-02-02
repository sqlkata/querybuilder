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


        protected override string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
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

            string sql;

            if (x.Value is UnsafeLiteral)
            {
                sql = $"{column} {checkOperator(method)} {value}";
            }
            else
            {
                sql = $"{column} {checkOperator(method)} {Parameter(ctx, value)}";
            }

            if (!string.IsNullOrEmpty(x.EscapeCharacter))
            {
                sql = $"{sql} ESCAPE '{x.EscapeCharacter}'";
            }

            return x.IsNot ? $"NOT ({sql})" : sql;

        }


        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
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

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected override string GetCtePrefix(bool recursive = false)
        {
            return recursive ? "WITH RECURSIVE" : "WITH";
        }
    }
}
