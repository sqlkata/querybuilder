using SqlKata.Compilers;

namespace SqlKata.Tests.ApprovalTests
{
    public sealed class Class1
    {
        [Fact]
        public void CompileCte_With_RawFromClause()
        {
            var queryCTe = new Query("prodCTE")
                .WithRaw("prodCTE", "SELECT * FROM B");

            Verify(new SqlServerCompiler().Compile(queryCTe));
        }
    }
}
