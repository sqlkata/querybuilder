using System.Collections.ObjectModel;
using System.Dynamic;
using FluentAssertions;
using JetBrains.Annotations;
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

        var build = query.Build().Render()
            .Should().Be("INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')");

        //Assert.Equal(
        //    "INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')",
        //    c[EngineCodes.SqlServer]);
    }

}
