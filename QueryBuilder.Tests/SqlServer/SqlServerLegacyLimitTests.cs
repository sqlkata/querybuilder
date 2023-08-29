using FluentAssertions;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests.SqlServer;

public sealed class SqlServerLegacyLimitTests 
{
    private readonly SqlServerCompiler _compiler = new()
    {
        UseLegacyPagination = true
    };

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
            .Be("SELECT TOP (10) * FROM [Table]");
    }

    [Fact]
    public void OffsetOnly()
    {
        var query = new Query("Table").Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("SELECT * FROM (SELECT *, ROW_NUMBER() " +
                "OVER (ORDER BY (SELECT 0)) AS [row_num] " +
                "FROM [Table]) AS [results_wrapper] " +
                "WHERE [row_num] >= 21");
    }

    [Fact]
    public void LimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("SELECT * FROM (SELECT *, ROW_NUMBER() " +
                "OVER (ORDER BY (SELECT 0)) AS [row_num] " +
                "FROM [Table]) AS [results_wrapper] " +
                "WHERE [row_num] BETWEEN 21 AND 25");
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
