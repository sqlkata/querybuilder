using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public partial class QueryBuilderTests
    {
       
        [Fact]
        public void BasicJoin()
        {
            var q = new Query().From("users").Join("countries", "countries.id", "users.country_id");

            var c = Compile(q);

            Assert.Equal("SELECT * FROM [users] \nINNER JOIN [countries] ON [countries].[id] = [users].[country_id]",
                c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM `users` \nINNER JOIN `countries` ON `countries`.`id` = `users`.`country_id`",
                c[EngineCodes.MySql]);
        }

        [Theory]
        [InlineData("inner join", "INNER JOIN")]
        [InlineData("left join", "LEFT JOIN")]
        [InlineData("right join", "RIGHT JOIN")]
        [InlineData("cross join", "CROSS JOIN")]
        public void JoinTypes(string given, string output)
        {
            var q = new Query().From("users")
                .Join("countries", "countries.id", "users.country_id", "=", given);

            var c = Compile(q);

            Assert.Equal($"SELECT * FROM [users] \n{output} [countries] ON [countries].[id] = [users].[country_id]",
                c[EngineCodes.SqlServer]);

            Assert.Equal($"SELECT * FROM `users` \n{output} `countries` ON `countries`.`id` = `users`.`country_id`",
                c[EngineCodes.MySql]);

            Assert.Equal(
                $"SELECT * FROM \"users\" \n{output} \"countries\" ON \"countries\".\"id\" = \"users\".\"country_id\"",
                c[EngineCodes.PostgreSql]);

            Assert.Equal(
                $"SELECT * FROM \"USERS\" \n{output} \"COUNTRIES\" ON \"COUNTRIES\".\"ID\" = \"USERS\".\"COUNTRY_ID\"",
                c[EngineCodes.Firebird]);
        }
    }
}
