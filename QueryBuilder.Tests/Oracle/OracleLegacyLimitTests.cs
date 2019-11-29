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
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(context);

            // Assert:
            Assert.Equal(SqlPlaceholder, context.RawSql);
        }

        [Fact]
        public void WithNoOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(10);
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(context);

            // Assert:
            Assert.Matches($"SELECT \\* FROM \\({SqlPlaceholder}\\) WHERE ROWNUM <= ?", context.RawSql);
            Assert.Equal(10, context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void WithNoLimit()
        {
            // Arrange:
            var query = new Query(TableName).Offset(20);
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(context);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM (GENERATED_SQL) \"results_wrapper\") WHERE \"row_num\" > ?", context.RawSql);
            Assert.Equal(20, context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void WithLimitAndOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(5).Offset(20);
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            compiler.ApplyLegacyLimit(context);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT \"results_wrapper\".*, ROWNUM \"row_num\" FROM (GENERATED_SQL) \"results_wrapper\" WHERE ROWNUM <= ?) WHERE \"row_num\" > ?", context.RawSql);
            Assert.Equal(25, context.Bindings[0]);
            Assert.Equal(20, context.Bindings[1]);
            Assert.Equal(2, context.Bindings.Count);
        }
    }
}