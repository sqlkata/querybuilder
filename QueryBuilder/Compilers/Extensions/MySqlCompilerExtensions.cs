using System;

namespace SqlKata.Compilers.Extensions
{
    public static class MySqlCompilerExtensions
    {
        public static string ENGINE_CODE = "mysql";
        public static Query ForMySql(this Query src, Func<Query, Query> fn)
        {
            return src.For(MySqlCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}