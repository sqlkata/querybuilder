using System;

namespace SqlKata.Compilers.Extensions
{
    public static class SqlServerCompilerExtensions
    {
        public static string ENGINE_CODE = "sqlsrv";
        public static Query ForSqlServer(this Query src, Func<Query, Query> fn)
        {
            return src.For(SqlServerCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}