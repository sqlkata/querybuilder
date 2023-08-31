namespace SqlKata.Compilers
{
    public sealed class SqliteCompiler : Compiler
    {
        private static readonly Dictionary<string, string> FormatMap = new()
        {
            { "date", "%Y-%m-%d" },
            { "time", "%H:%M:%S" },
            { "year", "%Y" },
            { "month", "%m" },
            { "day", "%d" },
            { "hour", "%H" },
            { "minute", "%M" }
        };

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

        protected override string? CompileLimit(Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset > 0)
            {
                writer.Append("LIMIT -1 OFFSET ");
                writer.AppendParameter(offset);
                return writer;
            }

            if (base.CompileLimit(query, writer) == null) return null;
            return writer;
        }

        protected override void CompileBasicDateCondition(Query query, BasicDateCondition condition, Writer writer)
        {
            if (!FormatMap.ContainsKey(condition.Part))
            {
                writer.AppendName(condition.Column);
                writer.Append(" ");
                writer.Append(condition.Operator);
                writer.Append(" ");
                writer.AppendParameter(query, condition.Value);
                return;
            }

            if (condition.IsNot)
                writer.Append("NOT (");
            writer.Append("strftime('");
            writer.Append(FormatMap[condition.Part]);
            writer.Append("', ");
            writer.AppendName(condition.Column);
            writer.Append(") ");
            writer.Append(condition.Operator);
            writer.Append(" cast(");
            writer.AppendParameter(query, condition.Value);
            writer.Append(" as text)");
            if (condition.IsNot)
                writer.Append(")");
        }
    }
}
