using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Oracle
{
    public class OracleLegacyLimitTests : TestSupport
    {
        private const string TableName = "Table";
        private const string SqlPlaceholder = "GENERATED_SQL";
        private readonly OracleCompiler compiler;

        public OracleLegacyLimitTests()
        {
            compiler = Compilers.Get<OracleCompiler>(EngineCodes.Oracle);
            compiler.UseLegacyPagination = true;
        }

        [Fact]
        public void WithNoLimitNorOffset()
        {
            // Arrange:
            var query = new Query(TableName);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(ctx);

            // Assert:
            Assert.Equal(SqlPlaceholder, ctx.RawSql);
        }

        [Fact]
        public void WithNoOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(10);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(ctx);

            // Assert:
            Assert.Matches($"SELECT \\* FROM \\({SqlPlaceholder}\\) WHERE ROWNUM <= ?", ctx.RawSql);
            Assert.Equal(10, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void WithNoLimit()
        {
            // Arrange:
            var query = new Query(TableName).Offset(20);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(ctx);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM (GENERATED_SQL) \"results_wrapper\") WHERE \"row_num\" > ?", ctx.RawSql);
            Assert.Equal(20, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void WithLimitAndOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(5).Offset(20);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(ctx);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM (GENERATED_SQL) \"results_wrapper\" WHERE ROWNUM <= ?) WHERE \"row_num\" > ?", ctx.RawSql);
            Assert.Equal(25, ctx.Bindings[0]);
            Assert.Equal(20, ctx.Bindings[1]);
            Assert.Equal(2, ctx.Bindings.Count);
        }
    }
}
