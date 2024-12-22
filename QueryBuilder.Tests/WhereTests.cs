using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public class WhereTests : TestSupport
    {
        [Theory]
        [InlineData(EngineCodes.PostgreSql,
            """SELECT * FROM "Table1" WHERE ("Column1" = 10 OR "Column2" = 20) AND "Column3" = 30""")]
        public void GroupedWhereFilters(string engine, string sqlText)
        {
            var q = new Query("Table1")
                .Where(q => q.Or().Where("Column1", 10).Or().Where("Column2", 20))
                .Where("Column3", 30);

            var c = CompileFor(engine, q);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.PostgreSql,
            """SELECT * FROM "Table1" HAVING (SUM("Column1") = 10 OR SUM("Column2") = 20) AND SUM("Column3") = 30""")]
        public void GroupedHavingFilters(string engine, string sqlText)
        {
            var q = new Query("Table1")
                .Having(q => q.Or().HavingRaw("SUM([Column1]) = ?", 10).Or().HavingRaw("SUM([Column2]) = ?", 20))
                .HavingRaw("SUM([Column3]) = ?", 30);

            var c = CompileFor(engine, q);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird, """SELECT * FROM "TABLE1" WHERE "FIELD1" = Field2""")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `Table1` WHERE `Field1` = Field2")]
        [InlineData(EngineCodes.Oracle, """SELECT * FROM "Table1" WHERE "Field1" = Field2""")]
        [InlineData(EngineCodes.PostgreSql, """SELECT * FROM "Table1" WHERE "Field1" = Field2""")]
        [InlineData(EngineCodes.Sqlite, """SELECT * FROM "Table1" WHERE "Field1" = Field2""")]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] WHERE [Field1] = Field2")]
        public void UnsafeLiteralConditions(string engine, string sqlText)
        {
            var q = new Query("Table1")
                .Where("Field1", new UnsafeLiteral("Field2"));

            var c = CompileFor(engine, q);

            Assert.Equal(sqlText, c.ToString());
        }


    }
}
