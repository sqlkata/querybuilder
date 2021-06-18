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
            var query = new Query(TableName)
                .Select()
                .WhereDate("STAMP", "=", "2018-04-01");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'YY-MM-DD') = TO_CHAR(TO_DATE(?, 'YY-MM-DD'), 'YY-MM-DD')", ctx.RawSql);
            Assert.Equal("2018-04-01", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartDateTest()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("date", "STAMP", "=", "2018-04-01");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'YY-MM-DD') = TO_CHAR(TO_DATE(?, 'YY-MM-DD'), 'YY-MM-DD')", ctx.RawSql);
            Assert.Equal("2018-04-01", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereTimeWithSecondsTest()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereTime("STAMP", "=", "19:01:10");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI:SS'), 'HH24:MI:SS')", ctx.RawSql);
            Assert.Equal("19:01:10", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartTimeWithSecondsTest()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("time", "STAMP", "=", "19:01:10");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI:SS'), 'HH24:MI:SS')", ctx.RawSql);
            Assert.Equal("19:01:10", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereTimeWithoutSecondsTest()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereTime("STAMP", "=", "19:01");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI'), 'HH24:MI:SS')", ctx.RawSql);
            Assert.Equal("19:01", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartTimeWithoutSecondsTest()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("time", "STAMP", "=", "19:01");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE TO_CHAR(\"STAMP\", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI'), 'HH24:MI:SS')", ctx.RawSql);
            Assert.Equal("19:01", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartYear()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("year", "STAMP", "=", "2018");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(YEAR FROM \"STAMP\") = ?", ctx.RawSql);
            Assert.Equal("2018", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartMonth()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("month", "STAMP", "=", "9");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(MONTH FROM \"STAMP\") = ?", ctx.RawSql);
            Assert.Equal("9", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartDay()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("day", "STAMP", "=", "15");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(DAY FROM \"STAMP\") = ?", ctx.RawSql);
            Assert.Equal("15", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartHour()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("hour", "STAMP", "=", "15");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(HOUR FROM \"STAMP\") = ?", ctx.RawSql);
            Assert.Equal("15", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartMinute()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("minute", "STAMP", "=", "25");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(MINUTE FROM \"STAMP\") = ?", ctx.RawSql);
            Assert.Equal("25", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void SimpleWhereDatePartSecond()
        {
            // Arrange:
            var query = new Query(TableName)
                .Select()
                .WhereDatePart("second", "STAMP", "=", "59");

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal($"SELECT * FROM \"{TableName}\" WHERE EXTRACT(SECOND FROM \"STAMP\") = ?", ctx.RawSql);
            Assert.Equal("59", ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }
    }
}
