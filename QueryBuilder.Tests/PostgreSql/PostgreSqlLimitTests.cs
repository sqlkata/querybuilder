using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.PostgreSql;

public class PostgreSqlLimitTests : TestSupport
{
    private readonly PostgresCompiler _compiler;

    public PostgreSqlLimitTests()
    {
        _compiler = Compilers.Get<PostgresCompiler>(EngineCodes.PostgreSql);
    }

    [Fact]
    public void WithNoLimitNorOffset()
    {
        var query = new Query("Table");
        var ctx = new SqlResult { Query = query };

        Assert.Null(_compiler.CompileLimit(ctx));
    }

    [Fact]
    public void WithNoOffset()
    {
        var query = new Query("Table").Limit(10);
        var ctx = new SqlResult { Query = query };

        Assert.Equal("LIMIT ?", _compiler.CompileLimit(ctx));
        Assert.Equal(10, ctx.Bindings[0]);
    }

    [Fact]
    public void WithNoLimit()
    {
        var query = new Query("Table").Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.Equal("OFFSET ?", _compiler.CompileLimit(ctx));
        Assert.Equal(20L, ctx.Bindings[0]);
        Assert.Single(ctx.Bindings);
    }

    [Fact]
    public void WithLimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.Equal("LIMIT ? OFFSET ?", _compiler.CompileLimit(ctx));
        Assert.Equal(5, ctx.Bindings[0]);
        Assert.Equal(20L, ctx.Bindings[1]);
        Assert.Equal(2, ctx.Bindings.Count);
    }
}
