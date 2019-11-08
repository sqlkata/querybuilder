using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Oracle
{
    public class OracleLimitTests : TestSupport
    {
        private const string TableName = "Table";
        private const string SqlPlaceholder = "GENERATED_SQL";

        private OracleCompiler compiler;

        public OracleLimitTests()
        {
            compiler = Compilers.Get<OracleCompiler>(EngineCodes.Oracle);
        }

        [Fact]
        public void NoLimitNorOffset()
        {
            // Arrange:
            var query = new Query(TableName);
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            Assert.Null(compiler.CompileLimit(context));
        }

        [Fact]
        public void LimitOnly()
        {
            // Arrange:
            var query = new Query(TableName).Limit(10);
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            //  Act & Assert:
            Assert.EndsWith("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", compiler.CompileLimit(context));
            Assert.Equal(2, context.Bindings.Count);
            Assert.Equal(0, context.Bindings[0]);
            Assert.Equal(10, context.Bindings[1]);
        }

        [Fact]
        public void OffsetOnly()
        {
            // Arrange:
            var query = new Query(TableName).Offset(20);
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            Assert.EndsWith("OFFSET ? ROWS", compiler.CompileLimit(context));

            Assert.Single(context.Bindings);
            Assert.Equal(20, context.Bindings[0]);
        }

        [Fact]
        public void LimitAndOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(5).Offset(20);
            var context = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            Assert.EndsWith("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", compiler.CompileLimit(context));

            Assert.Equal(2, context.Bindings.Count);
            Assert.Equal(20, context.Bindings[0]);
            Assert.Equal(5, context.Bindings[1]);

            compiler.CompileLimit(context);
        }
    }
}