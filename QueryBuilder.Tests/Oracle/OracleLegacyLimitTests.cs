using FluentAssertions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests.Oracle;

public class OracleLegacyLimitTests : TestSupport
{
    private static readonly OracleCompiler OracleCompiler = new() { UseLegacyPagination = true };

    [Fact]
    public void WithNoLimitNorOffset()
    {
        var query = new Query("Table");
        OracleCompiler
            .Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM "Table"
                """);
    }

    [Fact]
    public void WithNoOffset()
    {
        var query = new Query("Table").Limit(10);
        OracleCompiler
            .Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM (SELECT * FROM "Table") WHERE ROWNUM <= 10
                """);
    }

    [Fact]
    public void WithNoLimit()
    {
        var query = new Query("Table").Offset(20);
        OracleCompiler
            .Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM (SELECT "results_wrapper".*, ROWNUM "row_num" FROM (SELECT * FROM "Table") "results_wrapper") WHERE "row_num" > 20
                """);
    }

    [Fact]
    public void WithLimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        OracleCompiler
            .Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM (SELECT "results_wrapper".*, ROWNUM "row_num" FROM (SELECT * FROM "Table") "results_wrapper" WHERE ROWNUM <= 25) WHERE "row_num" > 20
                """);
    }
}
