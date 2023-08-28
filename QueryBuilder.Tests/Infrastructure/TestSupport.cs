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
    
    protected static void CompareWithCompiler(Query query)
    {
        var sqlResult = new SqlServerCompiler().Compile(query);
        query.Build().Render(BindingMode.Values)
            .Should().Be(sqlResult.ToString());
        query.Build().Render(BindingMode.Placeholders)
            .Should().Be(sqlResult.RawSql);
        query.Build().Render(BindingMode.Params)
            .Should().Be(sqlResult.Sql);
    }
}
