using FluentAssertions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public sealed class CoverageTests : TestSupport
    {
        [Fact]
        public void CompileCte_With_RawFromClause()
        {
            var queryCTe = new Query("prodCTE")
                .WithRaw("prodCTE", "SELECT * FROM B");

            Compile(queryCTe)[EngineCodes.SqlServer].Should()
                .Be("WITH [prodCTE] AS (SELECT * FROM B)\n" +
                    "SELECT * FROM [prodCTE]");
        }
    }
}
