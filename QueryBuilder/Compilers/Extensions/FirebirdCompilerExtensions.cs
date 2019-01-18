using System;

namespace SqlKata.Compilers.Extensions
{
    public static class FirebirdCompilerExtensions
    {
        public static string ENGINE_CODE = "firebird";

        public static Query ForFirebird(this Query src, Func<Query, Query> fn)
        {
            return src.For(FirebirdCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}