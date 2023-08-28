using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests;

public class IntermediateStageInsertTests : TestSupport
{
    [Fact]
    public void InsertObject()
    {
        CompareWithCompiler(new Query("Table")
            .AsInsert(
                new
                {
                    Name = "The User",
                    Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }));
    }
}
