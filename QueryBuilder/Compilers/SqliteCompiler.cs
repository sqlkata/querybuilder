namespace SqlKata.Compilers
{
    public sealed class SqliteCompiler : Compiler
    {
        public SqliteCompiler()
        {
            LastId = "select last_insert_rowid() as id";
            EngineCode = EngineCodes.Sqlite;
            SupportsFilterClause = true;
        }

        protected override string CompileTrue()
        {
            return "1";
        }

        protected override string CompileFalse()
        {
            return "0";
        }

        protected override string? CompileLimit(SqlResult ctx, Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset > 0)
            {
                ctx.BindingsAdd(offset);
                writer.Append("LIMIT -1 OFFSET ");
                writer.AppendParameter(offset);
                writer.AssertMatches(ctx);
                return writer;
            }

            writer.AssertMatches(ctx);
            if (base.CompileLimit(ctx, query, writer) == null) return null;
            return writer;
        }

        protected override void CompileBasicDateCondition(SqlResult ctx,
            Query query, BasicDateCondition condition, Writer writer)
        {
            var column = XService.Wrap(condition.Column);
            var value = Parameter(ctx, query, writer, condition.Value);

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

            if (!formatMap.ContainsKey(condition.Part))
            {
                writer.Append(column);
                writer.Append(" ");
                writer.Append(condition.Operator);
                writer.Append(" ");
                writer.Append(value);
                return;
            }

            var sql = $"strftime('{formatMap[condition.Part]}', {column}) {condition.Operator} cast({value} as text)";

            writer.Append(condition.IsNot ? $"NOT ({sql})" :sql);
        }
    }
}
