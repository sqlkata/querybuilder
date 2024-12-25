using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests.Oracle
{
    public class OracleLegacyLimitTests : TestSupport
    {
        private const string TableName = "Table";
        private const string SqlPlaceholder = "GENERATED_SQL";
        private readonly Compiler compiler;

        public OracleLegacyLimitTests()
        {
            compiler = CreateCompiler(EngineCodes.Oracle, useLegacyPagination: true);
        }

        [Fact]
        public void WithNoLimitNorOffset()
        {
            // Arrange:
            var query = new Query(TableName);

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\"", ctx.RawSql);
        }

        [Fact]
        public void WithNoOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(10);

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT * FROM \"Table\") WHERE ROWNUM <= ?", ctx.RawSql);
            Assert.Equal(10, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void WithNoLimit()
        {
            // Arrange:
            var query = new Query(TableName).Offset(20);

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM (SELECT * FROM \"Table\") \"results_wrapper\") WHERE \"row_num\" > ?", ctx.RawSql);
            Assert.Equal(20L, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void WithLimitAndOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(5).Offset(20);

            // Act:
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM (SELECT * FROM \"Table\") \"results_wrapper\" WHERE ROWNUM <= ?) WHERE \"row_num\" > ?", ctx.RawSql);
            Assert.Equal(25L, ctx.Bindings[0]);
            Assert.Equal(20L, ctx.Bindings[1]);
            Assert.Equal(2, ctx.Bindings.Count);
        }
    }
}
