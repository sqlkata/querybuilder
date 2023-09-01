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
            return new Query("A").Verify(compiler);
        }
    }
}
