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

        public override string Parameter(SqlResult ctx, object parameter)
        {
            switch (parameter)
            {
                case DateTimeOffset dto:
                    {
                        // DateTimeOffset is always w.r.t. UTC
                        ctx.Bindings.Add(dto.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture));
                        return "?";
                    }

                case DateTime dt:
                    {
                        if (dt.Kind != DateTimeKind.Utc)
                        {
                            throw new ArgumentException("Unsupported DateTime kind for Snowflake compiler (must be UTC)");
                        }
                        ctx.Bindings.Add(dt.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture));
                        return "?";
                    }

                default:
                    {
                        return base.Parameter(ctx, parameter);
                    }
            }
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            /**
             * Snowflake supports the ANY_VALUE function natively
             */
            ctx.Query.Clauses = ctx.Query.Clauses.Select(clause =>
            {
                if (clause is AggregateAnyValueColumn column)
                {
                    return new AggregateGenericColumn
                    {
                        Engine = column.Engine,
                        Component = column.Component,
                        Type = column.Type,
                        Column = column.Column,
                        Alias = column.Alias,
                        Distinct = column.Distinct,
                    };
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
