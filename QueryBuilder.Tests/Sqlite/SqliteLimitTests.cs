using FluentAssertions;
using SqlKata.Compilers;

namespace SqlKata.Tests.Sqlite;

public class SqliteLimitTests 
{
    private readonly SqliteCompiler _compiler = new();

    [Fact]
    public void WithNoLimitNorOffset()
    {
        var query = new Query("Table");
        _compiler.Compile(query).ToString().Should()
            .Be("""
                SELECT * FROM "Table"
                """);
    }

    [Fact]
    public void WithNoOffset()
    {
        var query = new Query("Table").Limit(10);
        _compiler.Compile(query).ToString().Should()
            .Be("""SELECT * FROM "Table" LIMIT 10""");
    }

    [Fact]
    public void WithNoLimit()
    {
        var query = new Query("Table").Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("""SELECT * FROM "Table" LIMIT -1 OFFSET 20""");
    }

    [Fact]
    public void WithLimitAndOffset()
    {
        var query = new Query("Table").Limit(5).Offset(20);
        _compiler.Compile(query).ToString().Should()
            .Be("""SELECT * FROM "Table" LIMIT 5 OFFSET 20""");
    }
}
