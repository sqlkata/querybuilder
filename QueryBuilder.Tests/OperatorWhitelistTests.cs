using System;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public class OperatorWhitelistTests
    {

        public OperatorWhitelistTests()
        {

        }

        [Theory]
        [InlineData("!!")]
        [InlineData("~!")]
        [InlineData("*=")]
        public void DenyInvalidOperatorsInWhere(string op)
        {
            var compiler = new SqlServerCompiler();

            Assert.Throws<Exception>(() =>
            {
                compiler.Compile(new Query("Table").Where("Id", op, 1));
                compiler.Compile(new Query("Table").OrWhere("Id", op, 1));
                compiler.Compile(new Query("Table").WhereNot("Id", op, 1));
                compiler.Compile(new Query("Table").OrWhereNot("Id", op, 1));

                compiler.Compile(new Query("Table").WhereColumns("Col1", op, "Col2"));
                compiler.Compile(new Query("Table").OrWhereColumns("Col1", op, "Col2"));
            });
        }

        [Theory]
        [InlineData("!!")]
        [InlineData("~!")]
        [InlineData("*=")]
        public void DenyInvalidOperatorsInHaving(string op)
        {
            var compiler = new SqlServerCompiler();

            Assert.Throws<Exception>(() =>
            {
                compiler.Compile(new Query("Table").Having("Id", op, 1));
                compiler.Compile(new Query("Table").OrHaving("Id", op, 1));
                compiler.Compile(new Query("Table").HavingNot("Id", op, 1));
                compiler.Compile(new Query("Table").OrHavingNot("Id", op, 1));

                compiler.Compile(new Query("Table").HavingColumns("Col1", op, "Col2"));
                compiler.Compile(new Query("Table").OrHavingColumns("Col1", op, "Col2"));
            });
        }


        [Theory]
        [InlineData("=")]
        [InlineData("!=")]
        [InlineData("ilike")]
        public void AllowValidOperatorsInWhere(string op)
        {
            new Query("Table").Where("Id", op, 1);
            new Query("Table").OrWhere("Id", op, 1);
            new Query("Table").WhereNot("Id", op, 1);
            new Query("Table").OrWhereNot("Id", op, 1);

            new Query("Table").WhereColumns("Col1", op, "Col2");
            new Query("Table").OrWhereColumns("Col1", op, "Col2");
        }

        [Theory]
        [InlineData("=")]
        [InlineData("!=")]
        [InlineData("ilike")]
        public void AllowValidOperatorsInHaving(string op)
        {
            new Query("Table").Having("Id", op, 1);
            new Query("Table").OrHaving("Id", op, 1);
            new Query("Table").HavingNot("Id", op, 1);
            new Query("Table").OrHavingNot("Id", op, 1);

            new Query("Table").HavingColumns("Col1", op, "Col2");
            new Query("Table").OrHavingColumns("Col1", op, "Col2");
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
            var compiler = new SqlServerCompiler().Whitelist(op);

            var query = new Query("Table");

            compiler.Compile(query.Clone().Where("Id", op, 1));
            compiler.Compile(query.Clone().OrWhere("Id", op, 1));
            compiler.Compile(query.Clone().WhereNot("Id", op, 1));
            compiler.Compile(query.Clone().OrWhereNot("Id", op, 1));

            compiler.Compile(query.Clone().WhereColumns("Col1", op, "Col2"));
            compiler.Compile(query.Clone().OrWhereColumns("Col1", op, "Col2"));

            compiler.Compile(query.Clone().Having("Id", op, 1));
            compiler.Compile(query.Clone().OrHaving("Id", op, 1));
            compiler.Compile(query.Clone().HavingNot("Id", op, 1));
            compiler.Compile(query.Clone().OrHavingNot("Id", op, 1));

            compiler.Compile(query.Clone().HavingColumns("Col1", op, "Col2"));
            compiler.Compile(query.Clone().OrHavingColumns("Col1", op, "Col2"));
        }

        [Fact]
        public void ShouldAllowWhiteListedOperatorsInNestedWhere()
        {
            var compiler = new SqlServerCompiler().Whitelist("!!");

            var query = new Query("Table")
                .Where(q => q.Where("A", "!!", "value"));

            compiler.Compile(query);
        }

        [Fact]
        public void ShouldNotConsiderWhereRawCondition()
        {
            var compiler = new SqlServerCompiler();

            var query = new Query("Table")
                .WhereRaw("Col !! value");

        }

    }
}