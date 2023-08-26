using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests;

public class DeleteTests : TestSupport
{
    [Fact]
    public void BasicDelete()
    {
        var q = new Query("Posts").AsDelete();

        var c = Compile(q);

        Assert.Equal("DELETE FROM [Posts]", c[EngineCodes.SqlServer]);
    }

    [Fact]
    public void DeleteWithJoin()
    {
        var q = new Query("Posts")
            .Join("Authors", "Authors.Id", "Posts.AuthorId")
            .Where("Authors.Id", 5)
            .AsDelete();

        var c = Compile(q);

        Assert.Equal(
            "DELETE [Posts] FROM [Posts] \nINNER JOIN [Authors] ON [Authors].[Id] = [Posts].[AuthorId] WHERE [Authors].[Id] = 5",
            c[EngineCodes.SqlServer]);
        Assert.Equal(
            "DELETE `Posts` FROM `Posts` \nINNER JOIN `Authors` ON `Authors`.`Id` = `Posts`.`AuthorId` WHERE `Authors`.`Id` = 5",
            c[EngineCodes.MySql]);
    }

    [Fact]
    public void DeleteWithJoinAndAlias()
    {
        var q = new Query("Posts as P")
            .Join("Authors", "Authors.Id", "P.AuthorId")
            .Where("Authors.Id", 5)
            .AsDelete();

        var c = Compile(q);

        Assert.Equal(
            "DELETE [P] FROM [Posts] AS [P] \nINNER JOIN [Authors] ON [Authors].[Id] = [P].[AuthorId] WHERE [Authors].[Id] = 5",
            c[EngineCodes.SqlServer]);
    }
}
