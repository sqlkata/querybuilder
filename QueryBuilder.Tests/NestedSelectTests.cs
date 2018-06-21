using Xunit;
using SqlKata;
using SqlKata.Compilers;

public class NestedSelectTests
{

    [Fact]
    public static void Compile_RawSql_WithLimit_ReturnsCorrectQuery()
    {
        var q = new Query().From("Foo as src").Limit(1);
        var target = new SqlServerCompiler();

        var actual = target.Compile(q).ToString();
        Assert.Contains("SELECT TOP (1) * FROM [Foo]", actual);
    }

    [Fact]
    public static void SqlCompile_QueryAadNestedLimit_ReturnsQueryWithTop()
    {
        var q = new Query().From("Foo as src").Select("MyData");
        var n = new Query().From("Bar").Limit(1).Select("MyData");
        q.Select(n, "Bar");

        var target = new SqlServerCompiler();

        var actual = target.Compile(q).ToString();
        Assert.Contains("SELECT TOP (1) [MyData] FROM [Bar]", actual);
        Assert.Contains("SELECT [MyData], (SELECT TOP (1) [MyData] FROM [Bar]) AS [Bar] FROM [Foo] AS [src]", actual);
    }

    [Fact]
    public static void SqlCompile_QueryLimitAndNestedLimit_ReturnsQueryWithTop()
    {
        var q = new Query().From("Foo as src").Limit(1).Select("MyData");
        var n = new Query().From("Bar").Limit(1).Select("MyData");
        q.Select(n, "Bar");

        var target = new SqlServerCompiler();

        var actual = target.Compile(q).ToString();
        Assert.Contains("SELECT TOP (1) [MyData], (SELECT TOP (1) [MyData] FROM [Bar]) AS [Bar] FROM [Foo] AS [src]", actual);
    }

    [Fact]
    public static void SqlCompile_QueryLimitAndNestedLimit_BindingValue()
    {
        var n = new Query().From("Bar");
        var q = new Query().From("Foo").Select("MyData").Where("x", true).WhereNotExists(n);


        var target = new SqlServerCompiler();

        var actual = target.Compile(q).ToString();
        Assert.Contains("SELECT [MyData] FROM [Foo] WHERE [x] = True AND NOT EXISTS (SELECT TOP (1) 1 FROM [Bar])", actual);
    }
}
