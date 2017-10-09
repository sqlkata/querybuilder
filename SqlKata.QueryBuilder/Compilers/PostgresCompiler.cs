using System;

namespace SqlKata.QueryBuilder.Compilers
{
    public class PostgresCompiler : Compiler
    {
        public PostgresCompiler() : base()
        {
            EngineCode = "postgres";
        }

        protected override string OpeningIdentifier()
        {
            return "\"";
        }

        protected override string ClosingIdentifier()
        {
            return "\"";
        }
    }
    public static class PostgresCompilerExtensions
    {
        public static string ENGINE_CODE = "postgres";

        public static Query ForPostgres(this Query src, Func<Query, Query> fn)
        {
            return src.For(PostgresCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}