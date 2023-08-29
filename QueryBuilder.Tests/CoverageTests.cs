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
        [Fact]
        public void From_QueryFromClause_With_Bindings()
        {
            var query = new Query("T")
                .From(new Query("S").Where("c", 1));

            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM " +
                    "(SELECT * FROM [S] WHERE [c] = 1)");
        }
        [Fact]
        public void Join_QueryFromClause_With_Bindings()
        {
            var query = new Query("L")
                .Join(new Query("R").Where("c", 1),
                    join => join.On("a", "b"));

            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM [L] \n" +
                    "INNER JOIN " +
                    "(SELECT * FROM [R] WHERE [c] = 1) " +
                    "ON ([a] = [b])");
        }
    }
}
