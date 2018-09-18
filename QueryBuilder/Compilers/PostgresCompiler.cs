using System;
using SqlKata.Interfaces;

namespace SqlKata.Compilers
{
    public class PostgresCompiler : Compiler
    {
        public PostgresCompiler() : base()
        {
            EngineCode = "postgres";
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
                left = $"DATE_PART('{condition.Part.ToUpper()}', {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }
    }
    public static class PostgresCompilerExtensions
    {
        public static string ENGINE_CODE = "postgres";

        public static IQuery ForPostgres(this IQuery src, Func<IQuery, IQuery> fn)
        {
            return src.For(PostgresCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}