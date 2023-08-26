using System.Collections.Generic;

namespace SqlKata.Compilers
{
    public sealed class SqliteCompiler : Compiler
    {
        public SqliteCompiler()
        {
            OpeningIdentifier = ClosingIdentifier = "\"";
            LastId = "select last_insert_rowid() as id";
            EngineCode = EngineCodes.Sqlite;
            SupportsFilterClause = true;
        }

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
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Add(offset);
                return $"LIMIT -1 OFFSET {ParameterPlaceholder}";
            }

            return base.CompileLimit(ctx);
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
            var value = Parameter(ctx, condition.Value);

            var formatMap = new Dictionary<string, string>
            {
                { "date", "%Y-%m-%d" },
                { "time", "%H:%M:%S" },
                { "year", "%Y" },
                { "month", "%m" },
                { "day", "%d" },
                { "hour", "%H" },
                { "minute", "%M" }
            };

            if (!formatMap.ContainsKey(condition.Part)) return $"{column} {condition.Operator} {value}";

            var sql = $"strftime('{formatMap[condition.Part]}', {column}) {condition.Operator} cast({value} as text)";

            if (condition.IsNot) return $"NOT ({sql})";

            return sql;
        }
    }
}
