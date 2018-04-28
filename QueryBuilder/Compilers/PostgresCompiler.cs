using System;
using System.Collections.Generic;

namespace SqlKata.Compilers
{
    public class PostgresCompiler : Compiler
    {
        public PostgresCompiler() : base()
        {
            EngineCode = "postgres";
            SetConfigurationInsert();
        }

        protected override string OpeningIdentifier()
        {
            return "\"";
        }

        protected override string ClosingIdentifier()
        {
            return "\"";
        }

        protected override string CompileBasicDateCondition(BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"{column}::time";
            }
            else if (condition.Part == "date")
            {
                left = $"{column}::date";
            }
            else
            {
                left = $"DATE_PART('{condition.Part.ToUpper()}', {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        internal override void SetConfigurationInsert()
        {
            ConfigurationInsert = new Dictionary<Type, ConfigurationInsert>
            {
                [typeof(int)] = new ConfigurationInsert
                {
                    LastInsertCommand = " RETURNING \"[_id_]\""
                },
                [typeof(long)] = new ConfigurationInsert
                {
                    LastInsertCommand = " RETURNING \"[_id_]\""
                }
            };
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