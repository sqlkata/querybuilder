using FluentAssertions;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure;

public abstract class TestSupport
{
    protected readonly TestCompilersContainer Compilers = new();

    /// <summary>
    ///     For legacy test support
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    protected IReadOnlyDictionary<string, string> Compile(Query query)
    {
        return Compilers.Compile(query).ToDictionary(s => s.Key, v => v.Value.ToString());
    }

    protected void CompareWithCompiler(Query query)
    {
        var container = Compilers.Compile(query);
        foreach (var engine in container.Keys)
        {
            var sqlResult = container[engine];
            var queryBuilder = engine switch
            {
                _ => new QueryBuilder(query),
            };
            var build = queryBuilder.Build();
            var renderer = engine switch
            {
                EngineCodes.Generic => new Renderer(new X("[", "]", "AS ")),
                EngineCodes.SqlServer => new Renderer(new X("[", "]", "AS ")),
                EngineCodes.Firebird => new Renderer(new ("\"", "\"", "AS ", true))
                {
                    SingleRowDummyTableName = "RDB$DATABASE",
                    Dialect = Dialect.Firebird,

                },
                EngineCodes.MySql => new Renderer(new X("`", "`", "AS ")),
                EngineCodes.PostgreSql => new Renderer(new("\"", "\"", "AS ")),
                EngineCodes.Sqlite => new Renderer(new("\"", "\"", "AS ")),
                EngineCodes.Oracle => new Renderer(new("\"", "\"", ""))
                {
                    ParameterPrefix = ":p",
                    MultiInsertStartClause = "INSERT ALL INTO",
                    Dialect = Dialect.Oracle

                },
                _ => throw new ArgumentOutOfRangeException(engine)
            };

            renderer.BindingMode = BindingMode.Values;
            build.Render(renderer)
                .Should().Be(sqlResult.ToString(), engine);

            renderer.BindingMode = BindingMode.Placeholders;
            build.Render(renderer)
                .Should().Be(sqlResult.RawSql, engine);

            renderer.BindingMode = BindingMode.Params;
            build.Render(renderer)
                .Should().Be(sqlResult.Sql, engine);
        }
    }
}
