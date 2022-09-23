using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public class WhereTests : TestSupport
    {
        [Fact]
        public void GroupedWhereFilters()
        {
            var q = new Query("Table1")
                .Where(q => q.Or().Where("Column1", 10).Or().Where("Column2", 20))
                .Where("Column3", 30);

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM ""Table1"" WHERE (""Column1"" = 10 OR ""Column2"" = 20) AND ""Column3"" = 30", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void GroupedHavingFilters()
        {
            var q = new Query("Table1")
                .Having(q => q.Or().HavingRaw("SUM([Column1]) = ?", 10).Or().HavingRaw("SUM([Column2]) = ?", 20))
                .HavingRaw("SUM([Column3]) = ?", 30);

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM ""Table1"" HAVING (SUM(""Column1"") = 10 OR SUM(""Column2"") = 20) AND SUM(""Column3"") = 30", c[EngineCodes.PostgreSql]);
        }
    }
}
