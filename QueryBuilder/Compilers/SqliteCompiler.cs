using System.Collections.Generic;
using SqlKata;
using SqlKata.Compilers;

namespace SqlKata.Compilers
{
    public class SqliteCompiler : Compiler
    {
        public override string EngineCode { get; } = EngineCodes.Sqlite;
        protected override string parameterPlaceholder { get; set; } = "?";
        protected override string parameterPrefix { get; set; } = "@p";
        protected override string OpeningIdentifier { get; set; } = "\"";
        protected override string ClosingIdentifier { get; set; } = "\"";
        protected override string LastId { get; set; } = "select last_insert_rowid() as id";

        public override string CompileTrue()
        {
            return "1";
        }

        public override string CompileFalse()
        {
            return "0";
        }

        public override string CompileLimit(SqlResult ctx)
        {
            int limit = ctx.Query.GetLimit(EngineCode);
            int offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Add(offset);
                return "LIMIT -1 OFFSET ?";
            }

            return base.CompileLimit(ctx);
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            string column = Wrap(condition.Column);
            string value = Parameter(ctx, condition.Value);

            Dictionary<string,string> formatMap = new Dictionary<string, string> {
                {"date", "%Y-%m-%d"},
                {"time", "%H:%M:%S"},
                {"year", "%Y"},
                {"month", "%m"},
                {"day", "%d"},
                {"hour", "%H"},
                {"minute", "%M"},
            };

            if (!formatMap.ContainsKey(condition.Part))
            {
                return $"{column} {condition.Operator} {value}";
            }

            string sql = $"strftime('{formatMap[condition.Part]}', {column}) {condition.Operator} cast({value} as text)";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

    }
}
