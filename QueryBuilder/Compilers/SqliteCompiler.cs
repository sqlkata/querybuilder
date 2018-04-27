using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlKata.Compilers
{
    public class SqliteCompiler : Compiler
    {
        public SqliteCompiler() : base()
        {
            EngineCode = "sqlite";
        }

        protected override string OpeningIdentifier()
        {
            return "[";
        }

        protected override string ClosingIdentifier()
        {
            return "]";
        }
    }
    public static class SqliteCompilerExtensions
    {
        public static string ENGINE_CODE = "sqlite";

        public static Query ForSqlite(this Query src, Func<Query, Query> fn)
        {
            return src.For(SqliteCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}
