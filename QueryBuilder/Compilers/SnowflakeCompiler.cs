using System;
using System.Linq;
using System.Globalization;


namespace SqlKata.Compilers
{
    // Snowflake-specific compiler. The SQL syntax for Snowflake is actually
    // fairly similar with Postgres.
    public sealed class SnowflakeCompiler : PostgresCompiler
    {
        public override string EngineCode { get; } = EngineCodes.Snowflake;

        public override string LastId =>
            throw new NotSupportedException($"LastId not supported in {EngineCode} compiler");

        private class SnowflakeAggregatePercentileApproxColumn : SqlKata.AggregatePercentileApproxColumn
        {
            public SnowflakeAggregatePercentileApproxColumn() : base() { }

            public SnowflakeAggregatePercentileApproxColumn(SnowflakeAggregatePercentileApproxColumn other)
                : base(other)
            {
            }

            public override AbstractClause Clone()
            {
                return new SnowflakeAggregatePercentileApproxColumn(this);
            }

            public override string Compile(SqlResult ctx)
            {
                return $"APPROX_PERCENTILE({new Column { Name = Column }.Compile(ctx)}, {Percentile}) {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.WrapValue(Alias ?? Type)}";
            }
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            ctx.Query.Clauses = ctx.Query.Clauses.Select(clause =>
            {
                {
                    /**
                     * Snowflake supports the ANY_VALUE function natively
                     */
                    if (clause is AggregateAnyValueColumn column)
                    {
                        return new AggregateGenericColumn
                        {
                            Alias = column.Alias,
                            Column = column.Column,
                            Component = column.Component,
                            Distinct = column.Distinct,
                            Engine = column.Engine,
                            Type = column.Type,
                        };
                    }
                }

                {
                    /**
                     * Snowflake supports the APPROX_PERCENTILE function natively
                     */
                    if (clause is SqlKata.AggregatePercentileApproxColumn column)
                    {
                        return new SnowflakeAggregatePercentileApproxColumn
                        {
                            Alias = column.Alias,
                            Column = column.Column,
                            Component = column.Component,
                            Distinct = column.Distinct,
                            Engine = column.Engine,
                            Percentile = column.Percentile,
                        };
                    }
                }
                return clause;
            }).ToList();

            return base.CompileColumns(ctx);
        }

        private static SqlResult PrepareResultForSnowflake(SqlResult ctx)
        {
            ctx.NamedBindings = ctx.Bindings
                .Select((v, i) => (k: $"{i + 1}", v))
                .ToDictionary(kv => kv.k, kv => kv.v);
            ctx.Sql = ctx.RawSql;
            return ctx;
        }

        public override SqlResult Compile(Query query) => PrepareResultForSnowflake(CompileRaw(query));
    }
}
