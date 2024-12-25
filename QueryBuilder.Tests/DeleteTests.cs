using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests
{
    public class DeleteTests : TestSupport
    {
        [Theory]
        [InlineData(EngineCodes.SqlServer, "DELETE FROM [Posts]")]
        [InlineData(EngineCodes.Oracle, "DELETE FROM \"Posts\"")]
        [InlineData(EngineCodes.PostgreSql, "DELETE FROM \"Posts\"")]
        [InlineData(EngineCodes.MySql, "DELETE FROM `Posts`")]
        [InlineData(EngineCodes.Firebird, "DELETE FROM \"POSTS\"")]
        [InlineData(EngineCodes.Sqlite, "DELETE FROM \"Posts\"")]
        public void BasicDelete(string engine, string query)
        {
            var q = new Query("Posts").AsDelete();

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "DELETE [Posts] FROM [Posts] \n" +
            "INNER JOIN [Authors] ON [Authors].[Id] = [Posts].[AuthorId] WHERE [Authors].[Id] = 5")]
        [InlineData(EngineCodes.Oracle,
            "DELETE \"Posts\" FROM \"Posts\" \n" +
            "INNER JOIN \"Authors\" ON \"Authors\".\"Id\" = \"Posts\".\"AuthorId\" WHERE \"Authors\".\"Id\" = 5")]
        [InlineData(EngineCodes.PostgreSql,
            "DELETE \"Posts\" FROM \"Posts\" \n" +
            "INNER JOIN \"Authors\" ON \"Authors\".\"Id\" = \"Posts\".\"AuthorId\" WHERE \"Authors\".\"Id\" = 5")]
        [InlineData(EngineCodes.MySql,
            "DELETE `Posts` FROM `Posts` \n" +
            "INNER JOIN `Authors` ON `Authors`.`Id` = `Posts`.`AuthorId` WHERE `Authors`.`Id` = 5")]
        [InlineData(EngineCodes.Firebird,
            "DELETE \"POSTS\" FROM \"POSTS\" \n" +
            "INNER JOIN \"AUTHORS\" ON \"AUTHORS\".\"ID\" = \"POSTS\".\"AUTHORID\" WHERE \"AUTHORS\".\"ID\" = 5")]
        [InlineData(EngineCodes.Sqlite,
            "DELETE \"Posts\" FROM \"Posts\" \n" +
            "INNER JOIN \"Authors\" ON \"Authors\".\"Id\" = \"Posts\".\"AuthorId\" WHERE \"Authors\".\"Id\" = 5")]
        public void DeleteWithJoin(string engine, string query)
        {
            var q = new Query("Posts")
                .Join("Authors", "Authors.Id", "Posts.AuthorId")
                .Where("Authors.Id", 5)
                .AsDelete();

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "DELETE [P] FROM [Posts] AS [P] \n" +
            "INNER JOIN [Authors] AS [A] ON [A].[Id] = [P].[AuthorId] WHERE [A].[Id] = 5")]
        [InlineData(EngineCodes.Oracle,
            "DELETE \"P\" FROM \"Posts\" \"P\" \n" +
            "INNER JOIN \"Authors\" \"A\" ON \"A\".\"Id\" = \"P\".\"AuthorId\" WHERE \"A\".\"Id\" = 5")]
        [InlineData(EngineCodes.PostgreSql,
            "DELETE \"P\" FROM \"Posts\" AS \"P\" \n" +
            "INNER JOIN \"Authors\" AS \"A\" ON \"A\".\"Id\" = \"P\".\"AuthorId\" WHERE \"A\".\"Id\" = 5")]
        [InlineData(EngineCodes.MySql,
            "DELETE `P` FROM `Posts` AS `P` \n" +
            "INNER JOIN `Authors` AS `A` ON `A`.`Id` = `P`.`AuthorId` WHERE `A`.`Id` = 5")]
        [InlineData(EngineCodes.Firebird,
            "DELETE \"P\" FROM \"POSTS\" AS \"P\" \n" +
            "INNER JOIN \"AUTHORS\" AS \"A\" ON \"A\".\"ID\" = \"P\".\"AUTHORID\" WHERE \"A\".\"ID\" = 5")]
        [InlineData(EngineCodes.Sqlite,
            "DELETE \"P\" FROM \"Posts\" AS \"P\" \n" +
            "INNER JOIN \"Authors\" AS \"A\" ON \"A\".\"Id\" = \"P\".\"AuthorId\" WHERE \"A\".\"Id\" = 5")]
        public void DeleteWithJoinAndAlias(string engine, string query)
        {
            var q = new Query("Posts as P")
                .Join("Authors as A", "A.Id", "P.AuthorId")
                .Where("A.Id", 5)
                .AsDelete();

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }
    }
}
