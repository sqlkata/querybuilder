using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests.Oracle
{
    public class OracleLimitTests : TestSupport
    {
        private const string TableName = "Table";
        private const string SqlPlaceholder = "GENERATED_SQL";

        private readonly Compiler compiler;

        public OracleLimitTests()
        {
            compiler = CreateCompiler(EngineCodes.Oracle);
        }

        [Fact]
        public void NoLimitNorOffset()
        {
            // Arrange:
            var query = new Query(TableName);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\"", ctx.RawSql);
        }

        [Fact]
        public void LimitOnly()
        {
            // Arrange:
            var query = new Query(TableName).Limit(10);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\" ORDER BY (SELECT 0 FROM DUAL) OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", ctx.RawSql);
            Assert.Equal(2, ctx.Bindings.Count);
            Assert.Equal(0L, ctx.Bindings[0]);
            Assert.Equal(10, ctx.Bindings[1]);
        }

        [Fact]
        public void OffsetOnly()
        {
            // Arrange:
            var query = new Query(TableName).Offset(20);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\" ORDER BY (SELECT 0 FROM DUAL) OFFSET ? ROWS", ctx.RawSql);
            Assert.Single(ctx.Bindings);
            Assert.Equal(20L, ctx.Bindings[0]);
        }

        [Fact]
        public void LimitAndOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(5).Offset(20);

            // Act
            var ctx = compiler.Compile(query);

            // Assert:
            Assert.Equal("SELECT * FROM \"Table\" ORDER BY (SELECT 0 FROM DUAL) OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", ctx.RawSql);
            Assert.Equal(2, ctx.Bindings.Count);
            Assert.Equal(20L, ctx.Bindings[0]);
            Assert.Equal(5, ctx.Bindings[1]);

            compiler.CompileLimit(ctx);
        }
    }
}
