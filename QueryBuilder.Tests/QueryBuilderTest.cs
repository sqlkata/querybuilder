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
            _pg = new PostgresCompiler();
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
            Assert.Equal("SELECT TOP (10) [id], [name] FROM [users]", c[0]);
            Assert.Equal("SELECT `id`, `name` FROM `users` LIMIT 10", c[1]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" LIMIT 10", c[2]);
        }

        [Fact]
        public void Offset()
        {
            var q = new Query().From("users").Offset(10);
            var c = Compile(q);

            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [subquery] WHERE [row_num] >= 11", c[0]);
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

            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [subquery] WHERE [row_num] >= " + (offset + 1), c.ToString());
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

        public void CteAndBindings()
        {
            var query = new Query("Races")
                        .For("mysql", s =>
                            s.With("range", q => q.From("seqtbl").Select("Id").Where("Id", "<", 33))
                            .WhereIn("RaceAuthor",
                                q => q.From("Users").Select("Name").Where("Status", "Available")
                            )
                        )
                        .For("sqlsrv", s =>
                            s.With("range",
                                q => q.From("Sequence").Select("Number").Where("Number", "<", 78)
                            )
                            .Limit(25).Offset(20)
                        )
                        .For("postgres",
                            s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d")).Where("Name", "3778")
                        )
                        .Where("Id", ">", 55)
                        .WhereBetween("Value", 18, 24);

            var c = Compile(query);

            Assert.Equal("WITH [range] AS (SELECT [Number] FROM [Sequence] WHERE [Number] < 78) SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Races]WHERE [Id] > 55 AND [Value] BETWEEN 18 AND 24) WHERE [[row_num]]] BETWEEN 21 AND 45", c[0]);
            Assert.Equal("WITH `range` AS (SELECT `Id` FROM `seqtbl` WHERE `Id` < 33) SELECT * FROM `Races` WHERE `RaceAuthor` IN (SELECT `Name` FROM `Users` WHERE `Status` = Available) AND `Id` > 55 AND `Value` BETWEEN 18 AND 24", c[1]);

            Assert.Equal("WITH \"range\" AS (SELECT \"d\" FROM generate_series(1, 33) as d) SELECT * FROM \"Races\" WHERE \"Name\" = 3778 AND \"Id\" > 55 AND \"Value\" BETWEEN 18 AND 24", c[2]);
        }

        [Fact]
        public void InnerScopeEngineWithinCTE()
        {
            var series = new Query("table")
                .ForPostgres(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"));
            var query = new Query("series").With("series", series);

            var c = Compile(query);

            Assert.Equal("WITH [series] AS (SELECT * FROM [table] WHERE sqlsrv = 1) SELECT * FROM [series]", c[0]);
            Assert.Equal("WITH \"series\" AS (SELECT * FROM \"table\" WHERE postgres = true) SELECT * FROM \"series\"", c[2]);
        }


        [Fact]
        public void SqlServerTop()
        {
            var query = new Query("table").First();
            Assert.Equal("SELECT TOP (@p0) * FROM [table]", _sqlsrv.Compile(query).Sql);
        }

        [Fact]
        public void InsertFromSubQueryWithCte()
        {
            var query = new Query("expensive_cars")
            .With("old_cards", new Query("all_cars").Where("year", "<", 2000))
            .AsInsert(
                    new[] { "name", "model", "year" },
                    new Query("old_cars").Where("price", ">", 100).ForPage(2, 10)
            );

            var c = Compile(query);

            Assert.Equal("WITH [old_cards] AS (SELECT * FROM [all_cars] WHERE [year] < 2000) INSERT INTO [expensive_cars] ([name], [model], [year]) SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [old_cars] WHERE [price] > 100) AS [subquery] WHERE [row_num] BETWEEN 11 AND 20", c[0]);

            Assert.Equal("WITH `old_cards` AS (SELECT * FROM `all_cars` WHERE `year` < 2000) INSERT INTO `expensive_cars` (`name`, `model`, `year`) SELECT * FROM `old_cars` WHERE `price` > 100 LIMIT 10 OFFSET 10", c[1]);

            Assert.Equal("WITH \"old_cards\" AS (SELECT * FROM \"all_cars\" WHERE \"year\" < 2000) INSERT INTO \"expensive_cars\" (\"name\", \"model\", \"year\") SELECT * FROM \"old_cars\" WHERE \"price\" > 100 LIMIT 10 OFFSET 10", c[2]);
        }

        [Fact]
        public void InsertWithNullValues()
        {
            var query = new Query("Books").AsInsert(
                new[] { "Id", "Author", "ISBN", "Date" },
                new object[] { 1, "Author 1", "123456", null }
            );

            var c = Compile(query);

            Assert.Equal("INSERT INTO [Books] ([Id], [Author], [ISBN], [Date]) VALUES (1, 'Author 1', 123456, NULL)", c[0]);
        }

        [Fact]
        public void UpdateWithNullValues()
        {
            var query = new Query("Books").Where("Id", 1).AsUpdate(
                new[] { "Author", "Date", "Version" },
                new object[] { "Author 1", null, null }
            );

            var c = Compile(query);

            Assert.Equal("UPDATE [Books] SET [Author] = 'Author 1', [Date] = NULL, [Version] = NULL WHERE [Id] = 1", c[0]);
        }
    }
}