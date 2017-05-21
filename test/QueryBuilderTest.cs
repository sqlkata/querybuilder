using System;
using System.Collections.Generic;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public class QueryBuilderTest
    {
        private readonly Compiler _pg;
        private readonly MySqlCompiler _mysql;

        public SqlServerCompiler _sqlsrv { get; private set; }

        private string[] Compile(Query q)
        {
            return new[]{
                 _sqlsrv.Compile(q.Clone()).ToString(),
                 _mysql.Compile(q.Clone()).ToString(),
                _pg.Compile(q.Clone()).ToString(),
            };
        }
        public QueryBuilderTest()
        {
            _sqlsrv = new SqlServerCompiler();
            _mysql = new MySqlCompiler();
            _pg = new Compiler();
        }

        [Fact]
        public void BasicSelect()
        {
            var q = new Query().From("users").Select("id", "name");
            var c = Compile(q);

            Assert.Equal(c[0], "SELECT [id], [name] FROM [users]");
            Assert.Equal(c[1], "SELECT `id`, `name` FROM `users`");
            Assert.Equal(c[2], "SELECT \"id\", \"name\" FROM \"users\"");
        }

        [Fact]
        public void BasicSelectWithAlias()
        {
            var q = new Query().From("users as u").Select("id", "name");
            var c = Compile(q);

            Assert.Equal("SELECT [id], [name] FROM [users] AS [u]", c[0]);
            Assert.Equal("SELECT `id`, `name` FROM `users` AS `u`", c[1]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" AS \"u\"", c[2]);
        }

        [Fact]
        public void Limit()
        {
            var q = new Query().From("users").Select("id", "name").Limit(10);
            var c = Compile(q);

            // Assert.Equal(c[0], "SELECT * FROM (SELECT [id], [name],ROW_NUMBER() OVER (SELECT 0) AS [row_num] FROM [users]) AS [temp_table] WHERE [row_num] >= 10");
            Assert.Equal("SELECT TOP 10 [id], [name] FROM [users]", c[0]);
            Assert.Equal("SELECT `id`, `name` FROM `users` LIMIT 10", c[1]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" LIMIT 10", c[2]);
        }

        [Fact]
        public void Offset()
        {
            var q = new Query().From("users").Offset(10);
            var c = Compile(q);

            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) WHERE [row_num] >= 11", c[0]);
            Assert.Equal("SELECT * FROM `users` LIMIT 18446744073709551615 OFFSET 10", c[1]);
            Assert.Equal("SELECT * FROM \"users\" OFFSET 10", c[2]);
        }

        [Theory()]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(1000000)]
        public void OffsetSqlServer_Should_Be_Incremented_By_One(int offset)
        {
            var q = new Query().From("users").Offset(offset);
            var c = _sqlsrv.Compile(q);

            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) WHERE [row_num] >= " + (offset + 1), c.ToString());
        }

        [Theory()]
        [InlineData(-100)]
        [InlineData(0)]
        public void OffsetSqlServer_Should_Be_Ignored_If_Zero_Or_Negative(int offset)
        {
            var q = new Query().From("users").Offset(offset);
            var c = _sqlsrv.Compile(q);

            Assert.Equal("SELECT * FROM [users]", c.ToString()); 
        }

        [Fact]
        public void ColumnsEscaping()
        {
            var q = new Query().From("users").Select("mycol[isthis]");
            var c = Compile(q);

            Assert.Equal("SELECT [mycol[isthis]]] FROM [users]", c[0]);
        }

        public void DeepJoin()
        {
            var q = new Query().From("streets").DeepJoin("cities.countries");
            var c = Compile(q);

            Assert.Equal("SELECT * FROM [streets] INNER JOIN [cities] ON [streets].[cityId] = [cities].[Id] INNER JOIN [countries] ON [streets].[countryId] = [countries].[Id]", c[0]);

            Assert.Equal("SELECT * FROM `streets` INNER JOIN `cities` ON `streets`.`cityId` = `cities`.`Id` INNER JOIN `countries` ON `streets`.`countryId` = `countries`.`Id`", c[1]);

            Assert.Equal("SELECT * FROM \"streets\" INNER JOIN \"cities\" ON \"streets\".\"cityId\" = \"cities\".\"Id\" INNER JOIN \"countries\" ON \"streets\".\"countryId\" = \"countries\".\"Id\"", c[1]);
        }
    }
}