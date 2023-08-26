using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.SqlServer;

public class SqlServerLimitTests : TestSupport
{
    private readonly SqlServerCompiler _compiler;

    public SqlServerLimitTests()
    {
        _compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
        _compiler.UseLegacyPagination = false;
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

        Assert.EndsWith("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", _compiler.CompileLimit(ctx));
        Assert.Equal(2, ctx.Bindings.Count);
        Assert.Equal(0L, ctx.Bindings[0]);
        Assert.Equal(10, ctx.Bindings[1]);
    }

    [Fact]
    public void OffsetOnly()
    {
        var query = new Query("Table").Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.EndsWith("OFFSET ? ROWS", _compiler.CompileLimit(ctx));

        Assert.Single(ctx.Bindings);
        Assert.Equal(20L, ctx.Bindings[0]);
    }

    [Fact]
    public void LimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.EndsWith("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", _compiler.CompileLimit(ctx));

        Assert.Equal(2, ctx.Bindings.Count);
        Assert.Equal(20L, ctx.Bindings[0]);
        Assert.Equal(5, ctx.Bindings[1]);
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
