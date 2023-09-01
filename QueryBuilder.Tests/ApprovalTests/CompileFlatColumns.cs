using FluentAssertions;
using SqlKata.Compilers;
using SqlKata.Tests.ApprovalTests.Utils;

namespace SqlKata.Tests.ApprovalTests
{
    [UsesVerify]
    public sealed class CompileFrom
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task NoFrom(Compiler compiler)
        {
            return new Query().Select("a").Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileTableExpression
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task RawFromClause(Compiler compiler)
        {
            return new Query()
                .FromRaw("(INNER {a} ?)", 5)
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task SubQuery_No_Alias(Compiler compiler)
        {
            return new Query("X").From(new Query("Y")).Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileColumns
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Limit(Compiler compiler)
        {
            return new Query("X").Limit(3).Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Offset(Compiler compiler)
        {
            return new Query("X").Offset(4).Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Limit_Distinct(Compiler compiler)
        {
            return new Query("X").Distinct().Limit(3).Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileFlatColumns
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task All(Compiler compiler)
        {
            return new Query("X").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Specific(Compiler compiler)
        {
            return new Query("X").Select("a", "b", "c").Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileColumnList
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task RawColumn(Compiler compiler)
        {
            return new Query("X").SelectRaw("{1}, ?", "p").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task SubQuery(Compiler compiler)
        {
            return new Query("X").Select(new Query("Y"), "q").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Aggregate(Compiler compiler)
        {
            return new Query("X").SelectCount("*").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Aggregate_Alias(Compiler compiler)
        {
            return new Query("X").SelectCount("s.a as q").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Aggregate_Filter(Compiler compiler)
        {
            return new Query("X")
                .SelectAggregate("t", "a", q => q.Where("b", 3))
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileColumnsAfterSelect
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Distinct(Compiler compiler)
        {
            return new Query("X").Distinct().Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Aggregate(Compiler compiler)
        {
            return new Query("X").AsMin("a").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Aggregate_Multiple_Columns(Compiler compiler)
        {
            return new Query("X")
                .AsCount(new[] { "a", "b" })
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileJoin
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Without_Conditions(Compiler compiler)
        {
            return new Query("X").CrossJoin("Y").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task With_Conditions(Compiler compiler)
        {
            return new Query("X")
                .Join("Y", on => on.On("a", "b")).Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileConditions
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task And_Condition(Compiler compiler)
        {
            return new Query("X")
                .Where("a", 88).And().Where("b", 77)
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Or_Condition(Compiler compiler)
        {
            return new Query("X")
                .Where("a", 88).Or().Where("b", 77)
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileRawCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task AppendRaw(Compiler compiler)
        {
            return new Query("X")
                .WhereRaw("blah ? ? ?", 1, 2, 3)
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileQueryCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task CompileSelectQuery(Compiler compiler)
        {
            return new Query("X")
                .Where("a", "=", new Query("Y"))
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileSubQueryCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task CompileSelectQuery(Compiler compiler)
        {
            return new Query("X")
                .WhereSub(new Query("Y"), 52)
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileBasicCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X").WhereNot("a", "k").Verify(compiler);
        }
        // TODO: [Theory, ClassData(typeof(AllCompilers))]
        // public Task Trivial(Compiler compiler)
        // {
        //     return new Query("X")
        //         .Where("a", new[] { "m", "n", "o"})
        //         .Verify(compiler);
        // }
    }

    [UsesVerify]
    public sealed class CompileBasicStringCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Starts(Compiler compiler)
        {
            return new Query("X").WhereStarts("a", "k").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Ends(Compiler compiler)
        {
            return new Query("X").WhereEnds("a", "k").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Contains(Compiler compiler)
        {
            return new Query("X").WhereContains("a", "k").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task WhereLike(Compiler compiler)
        {
            return new Query("X").WhereLike("a", "k").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public void WhereNull(Compiler compiler)
        {
            Assert.Throws<ArgumentException>(
                    () => compiler.Compile(
                        new Query("X").WhereLike("a", null!)))
                .Message.Should().Be("Expecting a non nullable string");
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public void WhereInt(Compiler compiler)
        {
            Assert.Throws<ArgumentException>(
                    () => compiler.Compile(
                        new Query("X").WhereLike("a", 123)))
                .Message.Should().Be("Expecting a non nullable string");
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task WhereNotLike(Compiler compiler)
        {
            return new Query("X").WhereNotLike("a", "K").Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task CaseSensitive(Compiler compiler)
        {
            return new Query("X")
                .WhereStarts("a", "K", true)
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task EscapeCharacter(Compiler compiler)
        {
            return new Query("X")
                .WhereStarts("a", "K*", escapeCharacter: '*')
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileBasicDateCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X").Not().WhereDatePart("year", "a",
                new DateTime(2000, 1, 2, 3, 4, 5)).Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileNestedCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X")
                .WhereNot(q => q.Where("a", 632))
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileTwoColumnsCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X")
                .Not().WhereColumns("a", "<>", "b")
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileBetweenCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Straight(Compiler compiler)
        {
            return new Query("X")
                .WhereBetween("a", "aaa", "zzz")
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X")
                .WhereNotBetween("a", "0", "99")
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileInCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Straight(Compiler compiler)
        {
            return new Query("X")
                .WhereIn("a", "aaa", "zzz")
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X")
                .OrWhereNotIn("a", "0", "99")
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Empty_Straight(Compiler compiler)
        {
            return new Query("X")
                .WhereIn("a", Enumerable.Empty<long>())
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Empty_Not(Compiler compiler)
        {
            return new Query("X")
                .WhereNotIn("a", Enumerable.Empty<int>())
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileInQueryCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Straight(Compiler compiler)
        {
            return new Query("X")
                .WhereIn("a", new Query("Y"))
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X")
                .WhereNotIn("a", new Query("Y"))
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileNullCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Straight(Compiler compiler)
        {
            return new Query("X")
                .WhereNull("a")
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Not(Compiler compiler)
        {
            return new Query("X")
                .WhereNotNull("a")
                .Verify(compiler);
        }
    }
    [UsesVerify]
    public sealed class CompileBooleanCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Equal(Compiler compiler)
        {
            return new Query("X")
                .WhereTrue("a")
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task NotEqual(Compiler compiler)
        {
            return new Query("X")
                .Not().WhereTrue("a")
                .Verify(compiler);
        }
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task False(Compiler compiler)
        {
            return new Query("X")
                .WhereTrue("a")
                .OrWhereFalse("b")
                .Verify(compiler);
        }
    }
    [UsesVerify]
    public sealed class CompileExistsCondition
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Query(Compiler compiler)
        {
            return new Query("X")
                .WhereExists(q => q.From("Y").Where("a", "=", 4))
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task SubQuery(Compiler compiler)
        {
            return new Query("X")
                .WhereNotExists(new Query("Y"))
                .Verify(compiler);
        }
    }
    [UsesVerify]
    public sealed class CompileUnion
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Combine(Compiler compiler)
        {
            return new Query("X")
                .Union(new Query("Y"))
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Combine_All(Compiler compiler)
        {
            return new Query("X")
                .ExceptAll(new Query("Y"))
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Combine_Raw(Compiler compiler)
        {
            return new Query("X")
                .CombineRaw("(Y ?)", 3)
                .Verify(compiler);
        }
    }
    [UsesVerify]
    public sealed class CompileCteQuery
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task RawFromClause(Compiler compiler)
        {
            return new Query("X")
                .WithRaw("q", "{Y} ?", 70)
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task QueryFromClause(Compiler compiler)
        {
            return new Query("X")
                .With(q => q.Select("a").As("q"))
                .Verify(compiler);
        }

        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task AdHocTableFromClause(Compiler compiler)
        {
            return new Query("X")
                .With("q", new[] { "a", "b", "c" },
                    new[]
                    {
                        new object?[]{1, "k", null},
                        new object?[]{2, null, "j"}
                    })
                .Verify(compiler);
        }

    }

    [UsesVerify]
    public sealed class CompileInsertQuery
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task SingleValue(Compiler compiler)
        {
            return new Query("X")
                .AsInsert(new[] { "a" },
                    new object?[]
                    {
                        new[] { 1 }
                    })
                .Verify(compiler);
        }
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task MultiValue(Compiler compiler)
        {
            return new Query("X")
                .AsInsert(new[] { "a" }, new[]
                {
                    new object?[] { 1 },
                    new object?[] { 2 }
                })
                .Verify(compiler);
        }
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task ReturnId(Compiler compiler)
        {
            return new Query("X")
                .AsInsert(new { a = 3 }, true)
                .Verify(compiler);
        }
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task InsertQueryClause(Compiler compiler)
        {
            return new Query("X")
                .AsInsert(new[]{"a"}, new Query("Y"))
                .Verify(compiler);
        }
    }

    [UsesVerify]
    public sealed class CompileDeleteQuery
    {
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task No_Join(Compiler compiler)
        {
            return new Query("X").AsDelete().Verify(compiler);
        }
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Join(Compiler compiler)
        {
            return new Query("X").CrossJoin("Y").AsDelete().Verify(compiler);
        }
        [Theory]
        [ClassData(typeof(AllCompilers))]
        public Task Join_NoFrom(Compiler compiler)
        {
            return new Query().CrossJoin("Y").AsDelete().Verify(compiler);
        }
    }

}
