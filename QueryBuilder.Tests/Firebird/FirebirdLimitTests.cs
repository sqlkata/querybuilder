using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests.Firebird
{
    public class FirebirdLimitTests : TestSupport
    {
        private readonly Compiler compiler;

        public FirebirdLimitTests()
        {
            compiler = CreateCompiler(EngineCodes.Firebird);
        }

        [Fact]
        public void NoLimitNorOffset()
        {
            var query = new Query("Table");

            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM \"TABLE\"", result.ToString());
        }

        [Fact]
        public void LimitOnly()
        {
            var query = new Query("Table").Limit(10);

            var result = compiler.Compile(query);

            Assert.Equal("SELECT FIRST 10 * FROM \"TABLE\"", result.ToString());
        }

        [Fact]
        public void OffsetOnly()
        {
            var query = new Query("Table").Offset(20);

            var result = compiler.Compile(query);

            Assert.Equal("SELECT SKIP 20 * FROM \"TABLE\"", result.ToString());
        }

        [Fact]
        public void LimitAndOffset()
        {
            var query = new Query("Table").Limit(5).Offset(20);

            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM \"TABLE\" ROWS ? TO ?", result.RawSql);
            Assert.Equal(2, result.Bindings.Count);
            Assert.Equal(21L, result.Bindings[0]);
            Assert.Equal(25L, result.Bindings[1]);
        }
    }
}
