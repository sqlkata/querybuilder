using FluentAssertions;
using SqlKata.Compilers;

namespace SqlKata.Tests.Oracle;

public sealed class OracleLimitTests
{
    private readonly OracleCompiler _compiler = new ();

    [Fact]
    public void NoLimitNorOffset()
    {
        var query = new Query("Table");
        _compiler.Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM "Table"
                """);
    }

    [Fact]
    public void LimitOnly()
    {
        var query = new Query("Table").Limit(10);
        _compiler.Compile(query).ToString().Should()
            .Be("""SELECT * FROM "Table" ORDER BY (SELECT 0 FROM DUAL) OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY""");
    }

    [Fact]
    public void OffsetOnly()
    {
        var query = new Query("Table").Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM "Table" ORDER BY (SELECT 0 FROM DUAL) OFFSET 20 ROWS
                """);
    }

    [Fact]
    public void LimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("""SELECT * FROM "Table" ORDER BY (SELECT 0 FROM DUAL) OFFSET 20 ROWS FETCH NEXT 5 ROWS ONLY""");
    }
}
