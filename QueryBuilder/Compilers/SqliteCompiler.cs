using System.Collections.Generic;
using System.Linq;
using SqlKata;
using SqlKata.Compilers;

namespace SqlKata.Compilers
{
    public class SqliteCompiler : Compiler
    {
        public override string EngineCode { get; } = EngineCodes.Sqlite;
        public override string parameterPlaceholder { get; } = "?";
        public override string parameterPrefix { get; } = "@p";
        public override string OpeningIdentifier { get; } = "\"";
        public override string ClosingIdentifier { get; } = "\"";
        public override string LastId { get; } = "select last_insert_rowid() as id";

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
                return "LIMIT -1 OFFSET ?";
            }

            return base.CompileLimit(ctx);
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
            var value = Parameter(ctx, condition.Value);

            var formatMap = new Dictionary<string, string> {
                { "date", "%Y-%m-%d" },
                { "time", "%H:%M:%S" },
                { "year", "%Y" },
                { "month", "%m" },
                { "day", "%d" },
                { "hour", "%H" },
                { "minute", "%M" },
            };

            if (!formatMap.ContainsKey(condition.Part))
            {
                return $"{column} {condition.Operator} {value}";
            }

            var sql = $"strftime('{formatMap[condition.Part]}', {column}) {condition.Operator} cast({value} as text)";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        private class AggregateAnyValueColumn: SqlKata.AggregateAnyValueColumn
        {
            public override string Compile(SqlResult ctx)
            {
                return $"COALESCE(NULL, {new Column { Name = Column }.Compile(ctx)}) {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.WrapValue(Alias ?? Type)}";
            }
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            /**
             * This replaces all "any_value" aggregates by the COALESCE function
             * but this requires a group by clause (anything will do).
             */
            if (!ctx.Query.HasComponent("group") && ctx.Query.Clauses.Any(clause => clause is SqlKata.AggregateAnyValueColumn aggregate))
            {
                ctx.Query.GroupByRaw("\"\"");
            }
            ctx.Query.Clauses = ctx.Query.Clauses.Select(clause =>
            {
                if (clause is SqlKata.AggregateAnyValueColumn column)
                {
                    return new AggregateAnyValueColumn
                    {
                        Engine = column.Engine,
                        Component = column.Component,
                        Column = column.Column,
                        Alias = column.Alias,
                        Distinct = column.Distinct,
                    };
                }
                return clause;
            }).ToList();

            return base.CompileColumns(ctx);
        }
    }
}
