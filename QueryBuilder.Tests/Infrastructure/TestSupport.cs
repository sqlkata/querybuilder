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

    private static (T?, Exception?) Run<T>(Func<T> action)
        where T : notnull
    {
        try
        {
            return (action(), null);
        }
        catch (Exception x)
        {
            return (default, x);
        }
    }
    protected void CompareWithCompiler(Query query)
    {
        var compilerRun = Run(() => Compilers.Compile(query));
        foreach (var engine in Compilers.KnownEngineCodes)
        {
            var queryBuilder = engine switch
            {
                _ => new QueryBuilder(query),
            };
            var renderer = engine switch
            {
                EngineCodes.Generic => new Renderer(new X("[", "]", "AS ")),
                EngineCodes.SqlServer => new Renderer(new X("[", "]", "AS "))
                {
                    True = "cast(1 as bit)",
                    False = "cast(0 as bit)"
                },
                EngineCodes.Firebird => new Renderer(new("\"", "\"", "AS ", true))
                {
                    SingleRowDummyTableName = "RDB$DATABASE",
                    Dialect = Dialect.Firebird,
                    True = "1",
                    False = "0"

                },
                EngineCodes.MySql => new Renderer(new X("`", "`", "AS ")),
                EngineCodes.PostgreSql => new Renderer(new("\"", "\"", "AS ")),
                EngineCodes.Sqlite => new Renderer(new("\"", "\"", "AS "))
                {

                    True = "1",
                    False = "0"
                },
                EngineCodes.Oracle => new Renderer(new("\"", "\"", ""))
                {
                    ParameterPrefix = ":p",
                    MultiInsertStartClause = "INSERT ALL INTO",
                    Dialect = Dialect.Oracle

                },
                _ => throw new ArgumentOutOfRangeException(engine)
            };

            Match(RunQuery(BindingMode.Values), Expand(compilerRun, BindingMode.Values));
            Match(RunQuery(BindingMode.Params), Expand(compilerRun, BindingMode.Params));
            Match(RunQuery(BindingMode.Placeholders), Expand(compilerRun, BindingMode.Placeholders));

            (string?, Exception?) RunQuery(BindingMode mode)
            {
                renderer.BindingMode = mode;
                return Run(() => queryBuilder.Build().Render(renderer));
            }

            (string?, Exception?) Expand(
                (TestSqlResultContainer?, Exception?) input,
                BindingMode mode)
            {
                return input switch
                {
                    ({ } r, null) => (mode switch
                    {
                        BindingMode.Placeholders => r[engine].RawSql,
                        BindingMode.Params => r[engine].Sql,
                        BindingMode.Values => r[engine].ToString(),
                        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
                    }, null),
                    (null, { } x) => (null, x),
                    _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
                };
            }
            void Match((string? sql, Exception? exception) actual,
                (string? sql, Exception? exception) expected)
            {
                if (expected.exception is { } x)
                {
                    actual.exception.Should().NotBeNull(expected.exception.Message);
                    actual.exception!.Message.Should().Be(x.Message);
                }
                else
                {
                    expected.sql.Should().NotBeNull();
                    actual.sql.Should().NotBeNull(actual.exception?.Message);
                    actual.sql.Should().Be(expected.sql);
                }
            }
        }
    }
}
