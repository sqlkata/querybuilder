using System;
using SqlKata;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public class OracleLegacyLimitTests
    {
        private const string TableName = "Table";
        private const string SqlPlaceholder = "GENERATED_SQL";

        private OracleCompiler compiler = new OracleCompiler()
        {
            UseLegacyPagination = true
        };

        [Fact]
        public void NoLimitNorOffset()
        {
            // Arrange:
            var query = new Query(TableName);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            var result = compiler.CompileLimit(ctx);
            compiler.ApplyLegacyLimit(ctx, 0, 0);

            Assert.Null(result);
            Assert.Equal(SqlPlaceholder, ctx.RawSql);
        }

        [Fact]
        public void LimitOnly()
        {
            // Arrange:
            var query = new Query(TableName).Limit(10);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act:
            var result = compiler.CompileLimit(ctx);
            compiler.ApplyLegacyLimit(ctx, 10, 0);

            // Assert:
            Assert.Null(result);
            Assert.Matches($"SELECT \\* FROM \\({SqlPlaceholder}\\) WHERE ROWNUM <= ?", ctx.RawSql);
            Assert.Equal(10, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void OffsetOnly()
        {
            // Arrange:
            var query = new Query(TableName).Offset(20);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            var result = compiler.CompileLimit(ctx);
            compiler.ApplyLegacyLimit(ctx, 0, 20);

            // Assert:
            //Assert.Null(result);
            Assert.Matches($"SELECT \\* FROM \\(SELECT \"(SqlKata_.*__)\"\\.\\*, ROWNUM \"(SqlKata_.*__)\" FROM \\({SqlPlaceholder}\\) \"(SqlKata_.*__)\"\\) WHERE \"(SqlKata_.*__)\" > \\?", ctx.RawSql);
            Assert.Equal(20, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);

        }

        [Fact]
        public void LimitAndOffset()
        {
            // Arrange:
            var query = new Query(TableName).Limit(5).Offset(20);
            var ctx = new SqlResult { Query = query, RawSql = SqlPlaceholder };

            // Act & Assert:
            var result = compiler.CompileLimit(ctx);
            compiler.ApplyLegacyLimit(ctx, 5, 20);

            // Assert:
            Assert.Null(result);
            Assert.Matches($"SELECT \\* FROM \\(SELECT \"(SqlKata_.*__)\"\\.\\*, ROWNUM \"(SqlKata_.*__)\" FROM \\({SqlPlaceholder}\\) \"(SqlKata_.*__)\" WHERE ROWNUM <= \\?\\) WHERE \"(SqlKata_.*__)\" > \\?", ctx.RawSql);
            Assert.Equal(25, ctx.Bindings[0]);
            Assert.Equal(20, ctx.Bindings[1]);
            Assert.Equal(2, ctx.Bindings.Count);
        }
    }
}