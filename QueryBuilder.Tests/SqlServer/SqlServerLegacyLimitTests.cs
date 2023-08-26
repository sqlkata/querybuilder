using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.SqlServer;

public class SqlServerLegacyLimitTests : TestSupport
{
    private readonly SqlServerCompiler _compiler;

    public SqlServerLegacyLimitTests()
    {
        _compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
        _compiler.UseLegacyPagination = true;
    }

    [Fact]
    public void NoLimitNorOffset()
    {
        var query = new Query("Table");
        var ctx = new SqlResult { Query = query };

        Assert.Null(_compiler.CompileLimit(ctx));
    }

    [Fact]
    public void LimitOnly()
    {
        var query = new Query("Table").Limit(10);
        var ctx = new SqlResult { Query = query };

        Assert.Null(_compiler.CompileLimit(ctx));
    }

    [Fact]
    public void OffsetOnly()
    {
        var query = new Query("Table").Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.Null(_compiler.CompileLimit(ctx));
    }

    [Fact]
    public void LimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.Null(_compiler.CompileLimit(ctx));
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
