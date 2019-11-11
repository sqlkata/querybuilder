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

            Assert.Equal("ROWS ? TO ?", compiler.CompileLimit(context));
            Assert.Equal(21, context.Bindings[0]);
            Assert.Equal(25, context.Bindings[1]);
            Assert.Equal(2, context.Bindings.Count);
        }
    }
}