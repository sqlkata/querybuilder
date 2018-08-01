using SqlKata;
using SqlKata.Compilers;
using Xunit;

public class MySqlLimitTest
{
    private MySqlCompiler compiler = new MySqlCompiler();

    [Fact]
    public void WithNoLimitNorOffset()
    {
        var query = new Query("Table");
        var ctx = new SqlResult { Query = query };

        Assert.Null(compiler.CompileLimit(ctx));

    }

    [Fact]
    public void WithNoOffset()
    {
        var query = new Query("Table").Limit(10);
        var ctx = new SqlResult { Query = query };

        Assert.Equal("LIMIT ?", compiler.CompileLimit(ctx));
        Assert.Equal(10, ctx.Bindings[0]);

    }

    [Fact]
    public void WithNoLimit()
    {
        var query = new Query("Table").Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.Equal("LIMIT 18446744073709551615 OFFSET ?", compiler.CompileLimit(ctx));
        Assert.Equal(20, ctx.Bindings[0]);
        Assert.Single(ctx.Bindings);

    }

    [Fact]
    public void WithLimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        var ctx = new SqlResult { Query = query };

        Assert.Equal("LIMIT ? OFFSET ?", compiler.CompileLimit(ctx));
        Assert.Equal(5, ctx.Bindings[0]);
        Assert.Equal(20, ctx.Bindings[1]);
        Assert.Equal(2, ctx.Bindings.Count);
    }

}