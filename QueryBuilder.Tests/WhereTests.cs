using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public class WhereTests : TestSupport
    {
        [Fact]
        public void GroupedWhereFilters()
        {
            var q = new Query("Table1")
                .Where(q => q.Or().Where("Column1", 10).Or().Where("Column2", 20))
                .Where("Column3", 30);

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM ""Table1"" WHERE (""Column1"" = 10 OR ""Column2"" = 20) AND ""Column3"" = 30", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void GroupedHavingFilters()
        {
            var q = new Query("Table1")
                .Having(q => q.Or().HavingRaw("SUM([Column1]) = ?", 10).Or().HavingRaw("SUM([Column2]) = ?", 20))
                .HavingRaw("SUM([Column3]) = ?", 30);

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM ""Table1"" HAVING (SUM(""Column1"") = 10 OR SUM(""Column2"") = 20) AND SUM(""Column3"") = 30", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void GroupedHavingWithConditionGroup()
        {
            var group = new ConditionGroup();
            group.Where("Total", ">", 100).OrWhere("Count", "<", 5);

            var q = new Query("Orders")
                .Having(group)
                .HavingBetween("Average", 10, 50)
                .HavingIn("Category", new[] { 1, 2, 3 });

            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Orders] HAVING ([Total] > 100 OR [Count] < 5) AND [Average] BETWEEN 10 AND 50 AND [Category] IN (1, 2, 3)", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void ConditionGroupStandalone()
        {
            var group = new ConditionGroup();
            group.Where("A", 1).OrWhere("B", 2);

            Assert.Equal(2, group.Clauses.Count);
            Assert.False(((AbstractCondition)group.Clauses[0]).IsOr);
            Assert.True(((AbstractCondition)group.Clauses[1]).IsOr);
        }

        [Fact]
        public void Or_FollowedByWhere_DoesNotAffectSubsequentHaving()
        {
            var q = new Query("Table1")
                .Where("A", 10)
                .Or()
                .Where("B", 20)
                .HavingRaw("SUM([C]) = ?", 1)
                .HavingRaw("SUM([D]) = ?", 2);

            var c = Compile(q);

            Assert.True(((AbstractCondition)q.Wheres.Clauses[1]).IsOr);
            Assert.False(((AbstractCondition)q.Havings.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 10 OR [B] = 20 HAVING SUM([C]) = 1 AND SUM([D]) = 2",
                c[EngineCodes.SqlServer]
            );
        }

        [Fact]
        public void Or_FollowedByHaving_DoesNotAffectSubsequentWhere()
        {
            var q = new Query("Table1")
                .HavingRaw("SUM([C]) = ?", 1)
                .Or()
                .HavingRaw("SUM([D]) = ?", 2)
                .Where("A", 10)
                .Where("B", 20);

            var c = Compile(q);

            Assert.True(((AbstractCondition)q.Havings.Clauses[1]).IsOr);
            Assert.False(((AbstractCondition)q.Wheres.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 10 AND [B] = 20 HAVING SUM([C]) = 1 OR SUM([D]) = 2",
                c[EngineCodes.SqlServer]
            );
        }

        [Fact]
        public void Or_Interleaved_WhereThenHavingThenWhere()
        {
            var q = new Query("Table1")
                .Where("A", 1)
                .Or()
                .HavingRaw("SUM([C]) = ?", 2)
                .Where("B", 3);

            var c = Compile(q);

            // The Or() was consumed by Having, so B remains AND
            Assert.False(((AbstractCondition)q.Wheres.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 1 AND [B] = 3 HAVING SUM([C]) = 2",
                c[EngineCodes.SqlServer]
            );
        }

        [Fact]
        public void Or_Interleaved_HavingThenWhereThenHaving()
        {
            var q = new Query("Table1")
                .HavingRaw("SUM([C]) = ?", 1)
                .Or()
                .Where("A", 2)
                .HavingRaw("SUM([D]) = ?", 3);

            var c = Compile(q);

            // The Or() was consumed by Where, so SUM(D) remains AND
            Assert.False(((AbstractCondition)q.Havings.Clauses[1]).IsOr);
            Assert.Equal(
                @"SELECT * FROM [Table1] WHERE [A] = 2 HAVING SUM([C]) = 1 AND SUM([D]) = 3",
                c[EngineCodes.SqlServer]
            );
        }
    }
}
