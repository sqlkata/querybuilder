using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Firebird
{
    public class FirebirdLimitTests : TestSupport
    {
        private readonly FirebirdCompiler compiler;

        public FirebirdLimitTests()
        {
            compiler = Compilers.Get<FirebirdCompiler>(EngineCodes.Firebird);
        }

        [Fact]
        public void NoLimitNorOffset()
        {
            var query = new Query("Table");
            var ctx = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(ctx));
        }

        [Fact]
        public void LimitOnly()
        {
            var query = new Query("Table").Limit(10);
            var ctx = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(ctx));
        }

        [Fact]
        public void OffsetOnly()
        {
            var query = new Query("Table").Offset(20);
            var ctx = new SqlResult {Query = query};

            Assert.Null(compiler.CompileLimit(ctx));
        }

        [Fact]
        public void LimitAndOffset()
        {
            var query = new Query("Table").Limit(5).Offset(20);
            var ctx = new SqlResult {Query = query};

            Assert.Equal("ROWS ? TO ?", compiler.CompileLimit(ctx));
            Assert.Equal(21, ctx.Bindings[0]);
            Assert.Equal(25, ctx.Bindings[1]);
            Assert.Equal(2, ctx.Bindings.Count);
        }
    }
}
