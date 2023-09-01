using SqlKata.Compilers;
using SqlKata.Tests.ApprovalTests.Utils;

namespace SqlKata.Tests.ApprovalTests
{
    [UsesVerify]
    public sealed class CompileFrom
    {
        [Theory, ClassData(typeof(AllCompilers))]
        public Task NoFrom(Compiler compiler)
        {
            return new Query().Select("a").Verify(compiler);
        }
    }
    [UsesVerify]
    public sealed class CompileTableExpression
    {
        [Theory, ClassData(typeof(AllCompilers))]
        public Task RawFromClause(Compiler compiler)
        {
            return new Query()
                .FromRaw("(INNER {a} ?)", 5)
                .Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task SubQuery_No_Alias(Compiler compiler)
        {
            return new Query("X").From(new Query("Y")).Verify(compiler);
        }

    }

    [UsesVerify]
    public sealed class CompileColumns
    {
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Limit(Compiler compiler)
        {
            return new Query("X").Limit(3).Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Offset(Compiler compiler)
        {
            return new Query("X").Offset(4).Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Limit_Distinct(Compiler compiler)
        {
            return new Query("X").Distinct().Limit(3).Verify(compiler);
        }

    }
    [UsesVerify]
    public sealed class CompileFlatColumns
    {
        [Theory, ClassData(typeof(AllCompilers))]
        public Task All(Compiler compiler)
        {
            return new Query("X").Verify(compiler);
        }

        [Theory, ClassData(typeof(AllCompilers))]
        public Task Specific(Compiler compiler)
        {
            return new Query("X").Select("a", "b", "c").Verify(compiler);
        }
    }
    [UsesVerify]
    public sealed class CompileColumnList
    {
        [Theory, ClassData(typeof(AllCompilers))]
        public Task RawColumn(Compiler compiler)
        {
            return new Query("X").SelectRaw("{1}, ?", "p").Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task SubQuery(Compiler compiler)
        {
            return new Query("X").Select(new Query("Y"), "q").Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Aggregate(Compiler compiler)
        {
            return new Query("X").SelectCount("*").Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Aggregate_Alias(Compiler compiler)
        {
            return new Query("X").SelectCount("s.a as q").Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
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
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Distinct(Compiler compiler)
        {
            return new Query("X").Distinct().Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Aggregate(Compiler compiler)
        {
            return new Query("X").AsMin("a").Verify(compiler);
        }
        [Theory, ClassData(typeof(AllCompilers))]
        public Task Aggregate_Multiple_Columns(Compiler compiler)
        {
            return new Query("X")
                .AsCount(new[] { "a", "b" })
                .Verify(compiler);
        }
    }
}
