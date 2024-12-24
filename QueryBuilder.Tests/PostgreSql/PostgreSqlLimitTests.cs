using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.PostgreSql
{
    public class PostgreSqlLimitTests : TestSupport
    {
        private readonly Compiler compiler;

        public PostgreSqlLimitTests()
        {
            compiler = CreateCompiler(EngineCodes.PostgreSql);
        }

        [Fact]
        public void WithNoLimitNorOffset()
        {
            var query = new Query("Table");

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\"", ctx.RawSql);
        }

        [Fact]
        public void WithNoOffset()
        {
            var query = new Query("Table").Limit(10);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\" LIMIT ?", ctx.RawSql);
            Assert.Equal(10, ctx.Bindings[0]);
        }

        [Fact]
        public void WithNoLimit()
        {
            var query = new Query("Table").Offset(20);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\" OFFSET ?", ctx.RawSql);
            Assert.Equal(20L, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void WithLimitAndOffset()
        {
            var query = new Query("Table").Limit(5).Offset(20);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\" LIMIT ? OFFSET ?", ctx.RawSql);
            Assert.Equal(5, ctx.Bindings[0]);
            Assert.Equal(20L, ctx.Bindings[1]);
            Assert.Equal(2, ctx.Bindings.Count);
        }
    }
}
