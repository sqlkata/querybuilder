using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests;

public class WhereTests : TestSupport
{
    [Fact]
    public void GroupedWhereFilters()
    {
        var q = new Query("T")
            .Where(q => q.Or().Where("A", 10).Or().Where("B", 20))
            .Where(q => q.And().Where("C", 30).Where("D", 40))
            .Where("E", 50);

        var c = Compile(q);

        Assert.Equal("SELECT * FROM [T] " +
                     "WHERE ([A] = 10 OR [B] = 20) " +
                     "AND ([C] = 30 AND [D] = 40) " +
                     "AND [E] = 50",
            c[EngineCodes.SqlServer]);
    }

    [Fact]
    public void GroupedHavingFilters()
    {
        var q = new Query("Table1")
            .Having(q => q.Or().HavingRaw("SUM([Column1]) = ?", 10).Or().HavingRaw("SUM([Column2]) = ?", 20))
            .HavingRaw("SUM([Column3]) = ?", 30);

        var c = Compile(q);

        Assert.Equal(
            @"SELECT * FROM ""Table1"" HAVING (SUM(""Column1"") = 10 OR SUM(""Column2"") = 20) AND SUM(""Column3"") = 30",
            c[EngineCodes.PostgreSql]);
    }
}
