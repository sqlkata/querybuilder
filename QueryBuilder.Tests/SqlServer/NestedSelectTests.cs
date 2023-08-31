using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests.SqlServer;

public class NestedSelectTests : TestSupport
{
    private readonly SqlServerCompiler _compiler;

    public NestedSelectTests()
    {
        _compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
    }

    [Fact]
    public void Compile_RawSql_WithLimit_ReturnsCorrectQuery()
    {
        var q = new Query().From("Foo as src").Limit(1);

        var actual = _compiler.Compile(q).ToString();
        Assert.Contains("SELECT TOP (1) * FROM [Foo]", actual);
    }

    [Fact]
    public void SqlCompile_QueryAadNestedLimit_ReturnsQueryWithTop()
    {
        var q = new Query().From("Foo as src").Select("MyData");
        var n = new Query().From("Bar").Limit(1).Select("MyData");
        q.Select(n, "Bar");

        var actual = _compiler.Compile(q).ToString();
        Assert.Contains("SELECT TOP (1) [MyData] FROM [Bar]", actual);
        Assert.Contains("SELECT [MyData], (SELECT TOP (1) [MyData] FROM [Bar]) AS [Bar] FROM [Foo] AS [src]",
            actual);
    }

    [Fact]
    public void SqlCompile_QueryLimitAndNestedLimit_ReturnsQueryWithTop()
    {
        var q = new Query().From("Foo as src").Limit(1).Select("MyData");
        var n = new Query().From("Bar").Limit(1).Select("MyData");
        q.Select(n, "Bar");


        var actual = _compiler.Compile(q).ToString();
        Assert.Contains(
            "SELECT TOP (1) [MyData], (SELECT TOP (1) [MyData] FROM [Bar]) AS [Bar] FROM [Foo] AS [src]", actual);
    }

    [Fact]
    public void SqlCompile_QueryLimitAndNestedLimit_BindingValue()
    {
        var n = new Query().From("Bar");
        var q = new Query().From("Foo").Where("x", true).WhereNotExists(n);
        // var q = new Query().From("Foo").Where("C", "c").WhereExists(n).Where("A", "a");

        var actual = _compiler.Compile(q).ToString();
        Assert.Contains("SELECT * FROM [Foo] WHERE [x] = cast(1 as bit) AND NOT EXISTS (SELECT 1 FROM [Bar])",
            actual);
        // Assert.Contains("SELECT * FROM [Foo] WHERE [C] = 'c' AND EXISTS (SELECT TOP (1) 1 FROM [Bar]) AND [A] = 'a'", actual);
    }
}
