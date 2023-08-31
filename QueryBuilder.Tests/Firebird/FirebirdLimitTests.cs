using FluentAssertions;
using SqlKata.Compilers;

namespace SqlKata.Tests.Firebird;

public sealed class FirebirdLimitTests
{
    private readonly FirebirdCompiler _compiler = new();

    [Fact]
    public void NoLimitNorOffset()
    {
        var query = new Query("Table");
        _compiler.Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM "TABLE"
                """);
    }

    [Fact]
    public void LimitOnly()
    {
        var query = new Query("Table").Limit(10);
        _compiler.Compile(query).ToString().Should()
            .Be("""
                SELECT FIRST 10 * FROM "TABLE"
                """);
    }

    [Fact]
    public void OffsetOnly()
    {
        var query = new Query("Table").Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("""
                SELECT SKIP 20 * FROM "TABLE"
                """);
    }

    [Fact]
    public void LimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM "TABLE" ROWS 21 TO 25
                """);
    }
}
