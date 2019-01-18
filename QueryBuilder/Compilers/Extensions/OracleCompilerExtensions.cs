using System;

namespace SqlKata.Compilers.Extensions
{
    public static class OracleCompilerExtensions
    {
        public static string ENGINE_CODE = "oracle";

        public static Query ForOracle(this Query src, Func<Query, Query> fn)
        {
            return src.For(ENGINE_CODE, fn);
        }
    }
}