using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.MySql
{
    public class MySqlLimitTests : TestSupport
    {
        private readonly MySqlCompiler compiler;

        public MySqlLimitTests()
        {
            compiler = Compilers.Get<MySqlCompiler>(EngineCodes.MySql);
        }

        [Fact]
        public void WithNoLimitNorOffset()
        {
            var query = new Query("Table");
            var context = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(context));
        }

        [Fact]
        public void WithNoOffset()
        {
            var query = new Query("Table").Limit(10);
            var context = new SqlResult {Query = query};

            Assert.Equal("LIMIT ?", compiler.CompileLimit(context));
            Assert.Equal(10, context.Bindings[0]);
        }

        [Fact]
        public void WithNoLimit()
        {
            var query = new Query("Table").Offset(20);
            var context = new SqlResult {Query = query};

            Assert.Equal("LIMIT 18446744073709551615 OFFSET ?", compiler.CompileLimit(context));
            Assert.Equal(20, context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void WithLimitAndOffset()
        {
            var query = new Query("Table").Limit(5).Offset(20);
            var context = new SqlResult {Query = query};

            Assert.Equal("LIMIT ? OFFSET ?", compiler.CompileLimit(context));
            Assert.Equal(5, context.Bindings[0]);
            Assert.Equal(20, context.Bindings[1]);
            Assert.Equal(2, context.Bindings.Count);
        }
    }
}