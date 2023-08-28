using FluentAssertions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests;

public class IntermediateStageInsertTests : TestSupport
{
    [Fact]
    public void InsertObject()
    {
        var query = new Query("Table")
            .AsInsert(
                new
                {
                    Name = "The User",
                    Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

        var sqlResult = new SqlServerCompiler().Compile(query);

        query.Build().Render(BindingMode.Values)
            .Should().Be(sqlResult.ToString());
        query.Build().Render(BindingMode.Placeholders)
            .Should().Be(sqlResult.RawSql);
        query.Build().Render(BindingMode.Params)
            .Should().Be(sqlResult.Sql);
    }

}
