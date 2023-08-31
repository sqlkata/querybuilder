using FluentAssertions;
using SqlKata.Compilers;

namespace SqlKata.Tests.SqlServer;

public sealed class SqlServerLimitTests
{
    private readonly SqlServerCompiler _compiler = new();

    [Fact]
    public void NoLimitNorOffset()
    {
        var query = new Query("Table");
        _compiler.Compile(query).ToString().Should()
            .Be("SELECT * FROM [Table]");
    }

    [Fact]
    public void LimitOnly()
    {
        var query = new Query("Table").Limit(10);
        _compiler.Compile(query).ToString().Should()
            .Be("SELECT * FROM [Table] ORDER BY (SELECT 0) " +
                "OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY");
    }

    [Fact]
    public void OffsetOnly()
    {
        var query = new Query("Table").Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("SELECT * FROM [Table] ORDER BY (SELECT 0) OFFSET 20 ROWS");
    }

    [Fact]
    public void LimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("SELECT * FROM [Table] ORDER BY (SELECT 0) " +
                "OFFSET 20 ROWS FETCH NEXT 5 ROWS ONLY");
    }

    [Fact]
    public void ShouldEmulateOrderByIfNoOrderByProvided()
    {
        var query = new Query("Table").Limit(5).Offset(20);

        Assert.Contains("ORDER BY (SELECT 0)", _compiler.Compile(query).ToString());
    }

    [Fact]
    public void ShouldKeepTheOrdersAsIsIfNoPaginationProvided()
    {
        var query = new Query("Table").OrderBy("Id");

        Assert.Contains("ORDER BY [Id]", _compiler.Compile(query).ToString());
    }

    [Fact]
    public void ShouldKeepTheOrdersAsIsIfPaginationProvided()
    {
        var query = new Query("Table").Offset(10).Limit(20).OrderBy("Id");

        Assert.Contains("ORDER BY [Id]", _compiler.Compile(query).ToString());
        Assert.DoesNotContain("(SELECT 0)", _compiler.Compile(query).ToString());
    }
}
