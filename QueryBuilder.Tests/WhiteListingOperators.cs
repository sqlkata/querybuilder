using System;
using SqlKata;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public class WhiteListingOperators
    {

        public WhiteListingOperators()
        {

        }

        [Theory]
        [InlineData("!!")]
        [InlineData("~!")]
        [InlineData("*=")]
        public void DenyInvalidOperatorsInWhere(string op)
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new Query("Table").Where("Id", op, 1);
                new Query("Table").OrWhere("Id", op, 1);
                new Query("Table").WhereNot("Id", op, 1);
                new Query("Table").OrWhereNot("Id", op, 1);

                new Query("Table").WhereColumns("Col1", op, "Col2");
                new Query("Table").OrWhereColumns("Col1", op, "Col2");
            });
        }

        [Theory]
        [InlineData("!!")]
        [InlineData("~!")]
        [InlineData("*=")]
        public void DenyInvalidOperatorsInHaving(string op)
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new Query("Table").Having("Id", op, 1);
                new Query("Table").OrHaving("Id", op, 1);
                new Query("Table").HavingNot("Id", op, 1);
                new Query("Table").OrHavingNot("Id", op, 1);

                new Query("Table").HavingColumns("Col1", op, "Col2");
                new Query("Table").OrHavingColumns("Col1", op, "Col2");
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

        [Fact]
        public void ShouldCopyTheOperatorsOnClone()
        {
            var query = new Query("Table").WhitelistOperator("!!");

            // should not throw exception
            query.Clone().Where("Col", "!!", "any value");
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
            var query = new Query("Table");

            query.WhitelistOperator(op);

            query.Clone().Where("Id", op, 1);
            query.Clone().OrWhere("Id", op, 1);
            query.Clone().WhereNot("Id", op, 1);
            query.Clone().OrWhereNot("Id", op, 1);

            query.Clone().WhereColumns("Col1", op, "Col2");
            query.Clone().OrWhereColumns("Col1", op, "Col2");

            query.Clone().Having("Id", op, 1);
            query.Clone().OrHaving("Id", op, 1);
            query.Clone().HavingNot("Id", op, 1);
            query.Clone().OrHavingNot("Id", op, 1);

            query.Clone().HavingColumns("Col1", op, "Col2");
            query.Clone().OrHavingColumns("Col1", op, "Col2");
        }

        [Fact]
        public void ShouldCopyTheOperatorsToNestedWhere()
        {
            var query = new Query("Table")
                .WhitelistOperator("!!")
                .Where(q => q.Where("A", "!!", "value"));
        }

        [Fact]
        public void ShouldCopyTheOperatorsToSubQueries()
        {
            var query = new Query()
                .WhitelistOperator("!!")
                .From(new Query("SubTable").Where("Col", "!!", "value"));
        }
    }
}