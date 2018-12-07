using System;
using SqlKata;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public class OracleLimitTests
    {
        private const string TableName = "Table";
        private const string SqlPlaceholder = "GENERATED_SQL";

        private OracleCompiler compiler = new OracleCompiler();

        [Fact]
        public void NoLimitNorOffset()
        {
            // Arrange:
            var query = new Query(TableName);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            Assert.Null(compiler.CompileLimit(ctx));
        }

        [Fact]
        public void LimitOnly()
        {
            // Arrange:
            var query = new Query(TableName).Limit(10);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            //  Act & Assert:
            Assert.EndsWith("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", compiler.CompileLimit(ctx));
            Assert.Equal(2, ctx.Bindings.Count);
            Assert.Equal(0, ctx.Bindings[0]);
            Assert.Equal(10, ctx.Bindings[1]);
        }

        [Fact]
        public void OffsetOnly()
        {
            // Arrange:
            var query = new Query(TableName).Offset(20);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            Assert.EndsWith("OFFSET ? ROWS", compiler.CompileLimit(ctx));

            Assert.Single(ctx.Bindings);
            Assert.Equal(20, ctx.Bindings[0]);
        }

        [Fact]
        public void LimitAndOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(5).Offset(20);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            Assert.EndsWith("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY", compiler.CompileLimit(ctx));

            Assert.Equal(2, ctx.Bindings.Count);
            Assert.Equal(20, ctx.Bindings[0]);
            Assert.Equal(5, ctx.Bindings[1]);

            compiler.CompileLimit(ctx);
        }
    }
}