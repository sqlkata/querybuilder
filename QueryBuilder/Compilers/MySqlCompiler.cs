using System;
using System.Collections.Generic;

namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler() : base()
        {
            EngineCode = "mysql";
            SetConfigurationInsert();
        }

        protected override string OpeningIdentifier()
        {
            return "`";
        }

        protected override string ClosingIdentifier()
        {
            return "`";
        }

        public override string CompileOffset(Query query)
        {
            var limitOffset = query.GetOneComponent("limit", EngineCode) as LimitOffset;

            if (limitOffset == null || !limitOffset.HasOffset())
            {
                return "";
            }

            // MySql will not accept offset without limit
            // So we will put a large number to avoid this error
            if (!limitOffset.HasLimit())
            {
                return "LIMIT 18446744073709551615 OFFSET ?";
            }

            return "OFFSET ?";
        }
        internal override void SetConfigurationInsert()
        {
            ConfigurationInsert = new Dictionary<Type, ConfigurationInsert>
            {
                [typeof(int)] = new ConfigurationInsert
                {
                    LastInsertCommand = ";SELECT LAST_INSERT_ID();"
                },
                [typeof(long)] = new ConfigurationInsert
                {
                    LastInsertCommand = ";SELECT LAST_INSERT_ID();"
                }
            };
        }
    }

    public static class MySqlCompilerExtensions
    {
        public static string ENGINE_CODE = "mysql";
        public static Query ForMySql(this Query src, Func<Query, Query> fn)
        {
            return src.For(MySqlCompilerExtensions.ENGINE_CODE, fn);
        }
    }
}