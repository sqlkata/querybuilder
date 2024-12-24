using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.SqlServer
{
    public class SqlServerLegacyLimitTests : TestSupport
    {
        private readonly Compiler compiler;

        public SqlServerLegacyLimitTests()
        {
            compiler = CreateCompiler(EngineCodes.SqlServer, useLegacyPagination: true);
        }

        [Fact]
        public void NoLimitNorOffset()
        {
            var query = new Query("Table");

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM [Table]", ctx.RawSql);
        }

        [Fact]
        public void LimitOnly()
        {
            var query = new Query("Table").Limit(10);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT TOP (?) * FROM [Table]", ctx.RawSql);
        }

        [Fact]
        public void OffsetOnly()
        {
            var query = new Query("Table").Offset(20);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Table]) AS [results_wrapper] WHERE [row_num] >= ?", ctx.RawSql);
            Assert.Single(ctx.Bindings);
            Assert.Equal(21L, ctx.Bindings[0]);
        }

        [Fact]
        public void LimitAndOffset()
        {
            var query = new Query("Table").Limit(5).Offset(20);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Table]) AS [results_wrapper] WHERE [row_num] BETWEEN ? AND ?", ctx.RawSql);
            Assert.Collection(ctx.Bindings,
                e => Assert.Equal(21L, e),
                e => Assert.Equal(25L, e));
        }

        [Fact]
        public void ShouldEmulateOrderByIfNoOrderByProvided()
        {
            var query = new Query("Table").Limit(5).Offset(20);

            var ctx = compiler.Compile(query);

            Assert.Contains("ORDER BY (SELECT 0)", ctx.ToString());
        }

        [Fact]
        public void ShouldKeepTheOrdersAsIsIfNoPaginationProvided()
        {
            var query = new Query("Table").OrderBy("Id");

            var ctx = compiler.Compile(query);

            Assert.Contains("ORDER BY [Id]", ctx.ToString());
        }

        [Fact]
        public void ShouldKeepTheOrdersAsIsIfPaginationProvided()
        {
            var query = new Query("Table").Offset(10).Limit(20).OrderBy("Id");

            var sqlResult = compiler.Compile(query);

            Assert.Contains("ORDER BY [Id]", sqlResult.ToString());
            Assert.DoesNotContain("(SELECT 0)", sqlResult.ToString());
        }
    }
}
