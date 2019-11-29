using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Oracle
{
    public class OracleDateConditionTests : TestSupport
    {
        private const string TableName = "Table";
        private const string SqlPlaceholder = "GENERATED_SQL";

        private OracleCompiler compiler;

        public OracleDateConditionTests()
        {
            compiler = Compilers.Get<OracleCompiler>(EngineCodes.Oracle);
        }

        [Fact]
        public void SimpleWhereDateTest()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDate("STAMP", "=", "2018-04-01");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'YY-MM-DD') = TO_CHAR(TO_DATE(?, 'YY-MM-DD'), 'YY-MM-DD')", context.RawSql);
            Assert.Equal("2018-04-01", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartDateTest()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("date", "STAMP", "=", "2018-04-01");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'YY-MM-DD') = TO_CHAR(TO_DATE(?, 'YY-MM-DD'), 'YY-MM-DD')", context.RawSql);
            Assert.Equal("2018-04-01", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereTimeWithSecondsTest()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereTime("STAMP", "=", "19:01:10");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI:SS'), 'HH24:MI:SS')", context.RawSql);
            Assert.Equal("19:01:10", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartTimeWithSecondsTest()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("time", "STAMP", "=", "19:01:10");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI:SS'), 'HH24:MI:SS')", context.RawSql);
            Assert.Equal("19:01:10", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereTimeWithoutSecondsTest()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereTime("STAMP", "=", "19:01");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI'), 'HH24:MI:SS')", context.RawSql);
            Assert.Equal("19:01", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartTimeWithoutSecondsTest()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("time", "STAMP", "=", "19:01");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI'), 'HH24:MI:SS')", context.RawSql);
            Assert.Equal("19:01", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartYear()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("year", "STAMP", "=", "2018");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(YEAR FROM \"STAMP\") = ?", context.RawSql);
            Assert.Equal("2018", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartMonth()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("month", "STAMP", "=", "9");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(MONTH FROM \"STAMP\") = ?", context.RawSql);
            Assert.Equal("9", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartDay()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("day", "STAMP", "=", "15");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(DAY FROM \"STAMP\") = ?", context.RawSql);
            Assert.Equal("15", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartHour()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("hour", "STAMP", "=", "15");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(HOUR FROM \"STAMP\") = ?", context.RawSql);
            Assert.Equal("15", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartMinute()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("minute", "STAMP", "=", "25");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(MINUTE FROM \"STAMP\") = ?", context.RawSql);
            Assert.Equal("25", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartSecond()
        {
            // Arrange:
            Query query = new Query(TableName)
                .Select()
                .WhereDatePart("second", "STAMP", "=", "59");

            // Act:
            SqlResult context = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(SECOND FROM \"STAMP\") = ?", context.RawSql);
            Assert.Equal("59", context.Bindings[0]);
            Assert.Single(context.Bindings);
        }
    }
}
