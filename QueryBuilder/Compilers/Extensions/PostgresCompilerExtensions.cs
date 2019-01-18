using System;

namespace SqlKata.Compilers.Extensions
{
    public static class PostgresCompilerExtensions
    {
        public static string ENGINE_CODE = "postgres";

        public static Query ForPostgres(this Query src, Func<Query, Query> fn)
        {
            return src.For(PostgresCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}