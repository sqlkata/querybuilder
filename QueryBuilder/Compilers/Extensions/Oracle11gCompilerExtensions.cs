using System;

namespace SqlKata.Compilers.Extensions
{
    public static class Oracle11gCompilerExtensions
    {
        public static string ENGINE_CODE = "oracle11g";

        public static Query ForOracle11g(this Query src, Func<Query, Query> fn)
        {
            return src.For(ENGINE_CODE, fn);
        }
    }
}