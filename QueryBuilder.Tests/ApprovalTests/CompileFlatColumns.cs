using SqlKata.Compilers;
using SqlKata.Tests.ApprovalTests.Utils;

namespace SqlKata.Tests.ApprovalTests
{
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
}
