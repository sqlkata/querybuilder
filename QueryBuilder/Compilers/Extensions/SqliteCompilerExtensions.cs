using System;

namespace SqlKata.Compilers.Extensions
{
    public static class SqliteCompilerExtensions
    {
        public static string ENGINE_CODE = "sqlite";
        public static Query ForSqlite(this Query src, Func<Query, Query> fn)
        {
            return src.For(SqliteCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}