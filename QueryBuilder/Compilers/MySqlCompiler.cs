using System;
using SqlKata.Interfaces;

namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler() : base()
        {
            EngineCode = "mysql";
            OpeningIdentifier = ClosingIdentifier = "`";
        }

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);


            if (offset == 0 && limit == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return "LIMIT ?";
            }

            if (limit == 0)
            {

                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                ctx.Bindings.Add(offset);
                return "LIMIT 18446744073709551615 OFFSET ?";
            }

            // We have both values

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return "LIMIT ? OFFSET ?";

        }
    }

    public static class MySqlCompilerExtensions
    {
        public static string ENGINE_CODE = "mysql";
        public static IQuery ForMySql(this IQuery src, Func<IQuery, IQuery> fn)
        {
            return src.For(MySqlCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}