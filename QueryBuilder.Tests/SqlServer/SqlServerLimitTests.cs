using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests.SqlServer
{
    public class SqlServerLimitTests : TestSupport
    {
        private readonly Compiler compiler;

        public SqlServerLimitTests()
        {
            compiler = CreateCompiler(EngineCodes.SqlServer, useLegacyPagination: false);
        }

        [Fact]
        public void NoLimitNorOffset()
        {
            var query = new Query("Table");

            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM [Table]", result.ToString());
        }

        [Fact]
        public void LimitOnly()
        {
            var query = new Query("Table").Limit(10);

            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM [Table] ORDER BY (SELECT 0) OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", result.RawSql);
            Assert.Equal(2, result.Bindings.Count);
            Assert.Equal(0L, result.Bindings[0]);
            Assert.Equal(10, result.Bindings[1]);
        }

        [Fact]
        public void OffsetOnly()
        {
            var query = new Query("Table").Offset(20);

            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM [Table] ORDER BY (SELECT 0) OFFSET ? ROWS", result.RawSql);
            Assert.Single(result.Bindings);
            Assert.Equal(20L, result.Bindings[0]);
        }

        [Fact]
        public void LimitAndOffset()
        {
            var query = new Query("Table").Limit(5).Offset(20);

            var ctx = compiler.Compile(query);

            Assert.Equal("SELECT * FROM [Table] ORDER BY (SELECT 0) OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", ctx.RawSql);
            Assert.Equal(2, ctx.Bindings.Count);
            Assert.Equal(20L, ctx.Bindings[0]);
            Assert.Equal(5, ctx.Bindings[1]);
        }

        [Fact]
        public void ShouldEmulateOrderByIfNoOrderByProvided()
        {
            var query = new Query("Table").Limit(5).Offset(20);

            var sqlResult = compiler.Compile(query);

            Assert.Contains("ORDER BY (SELECT 0)", sqlResult.ToString());
        }

        [Fact]
        public void ShouldKeepTheOrdersAsIsIfNoPaginationProvided()
        {
            var query = new Query("Table").OrderBy("Id");

            var sqlResult = compiler.Compile(query);

            Assert.Contains("ORDER BY [Id]", sqlResult.ToString());
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
