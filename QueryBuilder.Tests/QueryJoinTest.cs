using System;
using System.Collections.Generic;
using SqlKata.Execution;
using SqlKata;
using SqlKata.Compilers;
using Xunit;
using System.Collections;

namespace SqlKata.Tests
{
    public class QueryJoinTest
    {
        private readonly Compiler pgsql = new PostgresCompiler();
        private readonly MySqlCompiler mysql = new MySqlCompiler();
        private readonly FirebirdCompiler fbsql = new FirebirdCompiler();
        public SqlServerCompiler mssql = new SqlServerCompiler();

        private string[] Compile(Query q)
        {
            return new[]
            {
                mssql.Compile(q.Clone()).ToString(),
                mysql.Compile(q.Clone()).ToString(),
                pgsql.Compile(q.Clone()).ToString(),
                fbsql.Compile(q.Clone()).ToString(),
            };
        }

        [Fact]
        public void BasicJoin()
        {
            var q = new Query().From("users").Join("countries", "countries.id", "users.country_id");

            var c = Compile(q);

            Assert.Equal("SELECT * FROM [users] \nINNER JOIN [countries] ON [countries].[id] = [users].[country_id]",
                c[0]);
            Assert.Equal("SELECT * FROM `users` \nINNER JOIN `countries` ON `countries`.`id` = `users`.`country_id`",
                c[1]);
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
                c[0]);

            Assert.Equal($"SELECT * FROM `users` \n{output} `countries` ON `countries`.`id` = `users`.`country_id`",
                c[1]);

            Assert.Equal(
                $"SELECT * FROM \"users\" \n{output} \"countries\" ON \"countries\".\"id\" = \"users\".\"country_id\"",
                c[2]);

            Assert.Equal(
                $"SELECT * FROM \"USERS\" \n{output} \"COUNTRIES\" ON \"COUNTRIES\".\"ID\" = \"USERS\".\"COUNTRY_ID\"",
                c[3]);
        }
    }
}
