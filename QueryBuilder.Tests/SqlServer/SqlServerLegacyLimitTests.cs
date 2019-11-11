using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.SqlServer
{
    public class SqlServerLegacyLimitTests : TestSupport
    {
        private readonly SqlServerCompiler compiler;

        public SqlServerLegacyLimitTests()
        {
            compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
            compiler.UseLegacyPagination = true;
        }

        [Fact]
        public void NoLimitNorOffset()
        {
            Query query = new Query("Table");
            SqlResult context = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(context));
        }

        [Fact]
        public void LimitOnly()
        {
            Query query = new Query("Table").Limit(10);
            SqlResult context = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(context));
        }

        [Fact]
        public void OffsetOnly()
        {
            Query query = new Query("Table").Offset(20);
            SqlResult context = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(context));
        }

        [Fact]
        public void LimitAndOffset()
        {
            Query query = new Query("Table").Limit(5).Offset(20);
            SqlResult context = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(context));
        }

        [Fact]
        public void ShouldEmulateOrderByIfNoOrderByProvided()
        {
            Query query = new Query("Table").Limit(5).Offset(20);

            Assert.Contains("ORDER BY (SELECT 0)", compiler.Compile(query).ToString());
        }

        [Fact]
        public void ShouldKeepTheOrdersAsIsIfNoPaginationProvided()
        {
            Query query = new Query("Table").OrderBy("Id");

            Assert.Contains("ORDER BY [Id]", compiler.Compile(query).ToString());
        }

        [Fact]
        public void ShouldKeepTheOrdersAsIsIfPaginationProvided()
        {
            Query query = new Query("Table").Offset(10).Limit(20).OrderBy("Id");

            Assert.Contains("ORDER BY [Id]", compiler.Compile(query).ToString());
            Assert.DoesNotContain("(SELECT 0)", compiler.Compile(query).ToString());
        }
    }
}