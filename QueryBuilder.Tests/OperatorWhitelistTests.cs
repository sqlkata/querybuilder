using System;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using SqlKata.Tests.Infrastructure.TestCompilers;
using Xunit;

namespace SqlKata.Tests
{
    public class OperatorWhitelistTests : TestSupport
    {
        public static TheoryData<string> AllowedOperators = new()
        {
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "not like",
            "ilike", "not ilike",
            "like binary", "not like binary",
            "rlike", "not rlike",
            "regexp", "not regexp",
            "similar to", "not similar to"
        };

        [Theory]
        [InlineData("!!")]
        [InlineData("~!")]
        [InlineData("*=")]
        [InlineData("")]
        public void DenyInvalidOperatorsInWhere(string op)
        {
            var compiler = new TestCompiler();

            Assert.Throws<Exception>(() =>
            {
                var query = new Query("Table").Where("Id", op, 1);
                compiler.Compile(query);
            });
        }

        [Theory]
        [InlineData("!!")]
        [InlineData("~!")]
        [InlineData("*=")]
        [InlineData("")]
        public void DenyInvalidOperatorsInHaving(string op)
        {
            var compiler = new TestCompiler();

            Assert.Throws<Exception>(() =>
            {
                var query = new Query("Table").Having("Id", op, 1);
                compiler.Compile(query);
            });
        }

        [Theory]
        [MemberData(nameof(AllowedOperators))]
        public void AllowValidOperatorsInWhere(string op)
        {
            var query = new Query("Table").Where("Id", op, 1);

            var result = CompileFor(EngineCodes.Generic, query);

            Assert.Equal($"""SELECT * FROM "Table" WHERE "Id" {op} 1""", result.ToString());
        }

        [Theory]
        [MemberData(nameof(AllowedOperators))]
        public void AllowValidOperatorsInHaving(string op)
        {
            var query = new Query("Table").Having("Id", op, 1);
            var result = CompileFor(EngineCodes.Generic, query);

            Assert.Equal($"""SELECT * FROM "Table" HAVING "Id" {op} 1""", result.ToString());
        }

        [Theory]
        [InlineData("^")]
        [InlineData("<<")]
        [InlineData(">>")]
        [InlineData("~")]
        [InlineData("~*")]
        [InlineData("!~")]
        [InlineData("!~*")]
        public void ShouldNotThrowAfterWhiteListing(string op)
        {
            var query = new Query("Table").Where("Id", op, 1);

            var result = CompileFor(
                EngineCodes.Generic,
                query,
                compiler => compiler.Whitelist(op));

            Assert.Equal($"""SELECT * FROM "Table" WHERE "Id" {op} 1""", result.ToString());

        }

        [Fact]
        public void ShouldAllowWhiteListedOperatorsInNestedWhere()
        {
            var query = new Query("Table")
                .Where(q => q.Where("A", "!!", "value"));

            var result = CompileFor(EngineCodes.Generic, query, compiler => compiler.Whitelist("!!"));

            Assert.Equal("""SELECT * FROM "Table" WHERE ("A" !! 'value')""", result.ToString());
        }

        [Fact]
        public void ShouldNotConsiderWhereRawCondition()
        {
            var query = new Query("Table")
                .WhereRaw("Col !! value");

            var result = CompileFor(EngineCodes.Generic, query);

            Assert.Equal("""SELECT * FROM "Table" WHERE Col !! value""", result.ToString());

        }

    }
}
