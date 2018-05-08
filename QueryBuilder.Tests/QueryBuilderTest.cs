using System;
using System.Collections.Generic;
using SqlKata.Compilers;
using SqlKata.Execution;
using Xunit;

namespace SqlKata.Tests
{
    public class QueryBuilderTest
    {
        private readonly Compiler pgsql;
        private readonly MySqlCompiler mysql;

        public SqlServerCompiler mssql { get; private set; }

        private string[] Compile(Query q)
        {
            return new[]{
                 mssql.Compile(q.Clone()).ToString(),
                 mysql.Compile(q.Clone()).ToString(),
                pgsql.Compile(q.Clone()).ToString(),
            };
        }
        public QueryBuilderTest()
        {
            mssql = new SqlServerCompiler();
            mysql = new MySqlCompiler();
            pgsql = new PostgresCompiler();
        }

        [Fact]
        public void BasicSelect()
        {
            var q = new Query().From("users").Select("id", "name");
            var c = Compile(q);

            Assert.Equal("SELECT [id], [name] FROM [users]", c[0]);
            Assert.Equal("SELECT `id`, `name` FROM `users`", c[1]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\"", c[2]);
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

        [Fact]
        public void LimitOffset()
        {
            var q = new Query().From("users").Offset(10).Limit(5);

            var c = Compile(q);

            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [subquery] WHERE [row_num] BETWEEN 11 AND 15", c[0]);
            Assert.Equal("SELECT * FROM `users` LIMIT 5 OFFSET 10", c[1]);
            Assert.Equal("SELECT * FROM \"users\" LIMIT 5 OFFSET 10", c[2]);
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
            var c = mssql.Compile(q);

            Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [subquery] WHERE [row_num] >= " + (offset + 1), c.ToString());
        }

        [Theory()]
        [InlineData(-100)]
        [InlineData(0)]
        public void OffsetSqlServer_Should_Be_Ignored_If_Zero_Or_Negative(int offset)
        {
            var q = new Query().From("users").Offset(offset);
            var c = mssql.Compile(q);

            Assert.Equal("SELECT * FROM [users]", c.ToString());
        }

        [Fact]
        public void ColumnsEscaping()
        {
            var q = new Query().From("users").Select("mycol[isthis]");
            var c = Compile(q);

            Assert.Equal("SELECT [mycol[isthis]]] FROM [users]", c[0]);
        }

        [Fact]
        public void DeepJoin()
        {
            var q = new Query().From("streets").DeepJoin("cities.countries");
            var c = Compile(q);

            Assert.Equal("SELECT * FROM [streets] INNER JOIN [cities] ON [streets].[cityId] = [cities].[Id] INNER JOIN [countries] ON [cities].[countryId] = [countries].[Id]", c[0]);

            Assert.Equal("SELECT * FROM `streets` INNER JOIN `cities` ON `streets`.`cityId` = `cities`.`Id` INNER JOIN `countries` ON `cities`.`countryId` = `countries`.`Id`", c[1]);

            Assert.Equal("SELECT * FROM \"streets\" INNER JOIN \"cities\" ON \"streets\".\"cityId\" = \"cities\".\"Id\" INNER JOIN \"countries\" ON \"cities\".\"countryId\" = \"countries\".\"Id\"", c[2]);
        }

        [Fact]
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

            Assert.Equal("WITH [range] AS (SELECT [Number] FROM [Sequence] WHERE [Number] < 78) \nSELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Races] WHERE [Id] > 55 AND [Value] BETWEEN 18 AND 24) AS [subquery] WHERE [row_num] BETWEEN 21 AND 45", c[0]);
            Assert.Equal("WITH `range` AS (SELECT `Id` FROM `seqtbl` WHERE `Id` < 33) \nSELECT * FROM `Races` WHERE `RaceAuthor` IN (SELECT `Name` FROM `Users` WHERE `Status` = 'Available') AND `Id` > 55 AND `Value` BETWEEN 18 AND 24", c[1]);

            Assert.Equal("WITH \"range\" AS (SELECT \"d\" FROM generate_series(1, 33) as d) \nSELECT * FROM \"Races\" WHERE \"Name\" = 3778 AND \"Id\" > 55 AND \"Value\" BETWEEN 18 AND 24", c[2]);
        }

        [Fact]
        public void UpdateWithCte()
        {

            var now = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var query = new Query("Books")
                .With("OldBooks", q => q.From("Books").Where("Date", "<", now))
                .Where("Price", ">", 100)
                .AsUpdate(new Dictionary<string, object> {
                    {"Price", "150"}
                });

            var c = Compile(query);

            Assert.Equal($"WITH [OldBooks] AS (SELECT * FROM [Books] WHERE [Date] < '{now}') \nUPDATE [Books] SET [Price] = 150 WHERE [Price] > 100", c[0]);
        }

        [Fact]
        public void InnerScopeEngineWithinCTE()
        {
            var series = new Query("table")
                .ForPostgres(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"));
            var query = new Query("series").With("series", series);

            var c = Compile(query);

            Assert.Equal("WITH [series] AS (SELECT * FROM [table] WHERE sqlsrv = 1) \nSELECT * FROM [series]", c[0]);
            Assert.Equal("WITH \"series\" AS (SELECT * FROM \"table\" WHERE postgres = true) \nSELECT * FROM \"series\"", c[2]);
        }


        [Fact]
        public void SqlServerTop()
        {
            var query = new Query("table").Limit(1);
            Assert.Equal("SELECT TOP (@p0) * FROM [table]", mssql.Compile(query).Sql);
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

            Assert.Equal("WITH [old_cards] AS (SELECT * FROM [all_cars] WHERE [year] < 2000) \nINSERT INTO [expensive_cars] ([name], [model], [year]) SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [old_cars] WHERE [price] > 100) AS [subquery] WHERE [row_num] BETWEEN 11 AND 20", c[0]);

            Assert.Equal("WITH `old_cards` AS (SELECT * FROM `all_cars` WHERE `year` < 2000) \nINSERT INTO `expensive_cars` (`name`, `model`, `year`) SELECT * FROM `old_cars` WHERE `price` > 100 LIMIT 10 OFFSET 10", c[1]);

            Assert.Equal("WITH \"old_cards\" AS (SELECT * FROM \"all_cars\" WHERE \"year\" < 2000) \nINSERT INTO \"expensive_cars\" (\"name\", \"model\", \"year\") SELECT * FROM \"old_cars\" WHERE \"price\" > 100 LIMIT 10 OFFSET 10", c[2]);
        }

        [Fact]
        public void InsertMultiRecords()
        {
            var query = new Query("expensive_cars")
            .AsInsert(
                    new[] { "name", "brand", "year" },
                    new[]
                    {
                        new object[] { "Chiron", "Bugatti", null},
                        new object[] { "Huayra", "Pagani", 2012},
                        new object[] { "Reventon roadster", "Lamborghini", 2009}
                    }
            );

            var c = Compile(query);

            Assert.Equal("INSERT INTO [expensive_cars] ([name], [brand], [year]) VALUES ('Chiron', 'Bugatti', NULL), ('Huayra', 'Pagani', 2012), ('Reventon roadster', 'Lamborghini', 2009)", c[0]);
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

        [Fact]
        public void ShouldThrowException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new Query("Books").Get();
            });
        }

        [Fact]
        public void Union()
        {
            var laptops = new Query("Laptops");
            var mobiles = new Query("Phones").Union(laptops);

            var c = Compile(mobiles);

            Assert.Equal("(SELECT * FROM [Phones]) UNION (SELECT * FROM [Laptops])", c[0]);

        }

        [Fact]
        public void MultipleUnion()
        {
            var laptops = new Query("Laptops");
            var tablets = new Query("Tablets");

            var mobiles = new Query("Phones").Union(laptops).Union(tablets);


            var c = Compile(mobiles);

            Assert.Equal("(SELECT * FROM [Phones]) UNION (SELECT * FROM [Laptops]) UNION (SELECT * FROM [Tablets])", c[0]);

        }

        [Fact]
        public void MultipleUnionWithBindings()
        {
            var laptops = new Query("Laptops").Where("Price", ">", 1000);
            var tablets = new Query("Tablets").Where("Price", ">", 2000);

            var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).Union(tablets);


            var c = Compile(mobiles);

            Assert.Equal("(SELECT * FROM [Phones] WHERE [Price] < 3000) UNION (SELECT * FROM [Laptops] WHERE [Price] > 1000) UNION (SELECT * FROM [Tablets] WHERE [Price] > 2000)", c[0]);

        }

        [Fact]
        public void MultipleUnionWithBindingsAndPagination()
        {
            var laptops = new Query("Laptops").Where("Price", ">", 1000);
            var tablets = new Query("Tablets").Where("Price", ">", 2000).ForPage(2);

            var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).UnionAll(tablets);


            var c = Compile(mobiles);

            Assert.Equal("(SELECT * FROM [Phones] WHERE [Price] < 3000) UNION (SELECT * FROM [Laptops] WHERE [Price] > 1000) UNION ALL (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Tablets] WHERE [Price] > 2000) AS [subquery] WHERE [row_num] BETWEEN 16 AND 30)", c[0]);

        }

        [Fact]
        public void UnionWithCallbacks()
        {
            var mobiles = new Query("Phones")
                .Where("Price", "<", 3000)
                .Union(q => q.From("Laptops"))
                .UnionAll(q => q.From("Tablets"));

            var c = Compile(mobiles);

            Assert.Equal("(SELECT * FROM [Phones] WHERE [Price] < 3000) UNION (SELECT * FROM [Laptops]) UNION ALL (SELECT * FROM [Tablets])", c[0]);

        }

        [Fact]
        public void UnionWithDifferentEngine()
        {
            var mobiles = new Query("Phones")
                .Where("Price", "<", 300)
                .ForSqlServer(scope => scope.Except(q => q.From("Phones").WhereNot("Os", "iOS")))
                .ForPostgres(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
                .ForMySql(scope => scope.IntersectAll(q => q.From("Watches").Where("Os", "Android")))
                .UnionAll(q => q.From("Tablets").Where("Price", "<", 100));

            var c = Compile(mobiles);

            Assert.Equal("(SELECT * FROM [Phones] WHERE [Price] < 300) EXCEPT (SELECT * FROM [Phones] WHERE NOT ([Os] = 'iOS')) UNION ALL (SELECT * FROM [Tablets] WHERE [Price] < 100)", c[0]);

            Assert.Equal("(SELECT * FROM `Phones` WHERE `Price` < 300) INTERSECT ALL (SELECT * FROM `Watches` WHERE `Os` = 'Android') UNION ALL (SELECT * FROM `Tablets` WHERE `Price` < 100)", c[1]);

            Assert.Equal("(SELECT * FROM \"Phones\" WHERE \"Price\" < 300) UNION (SELECT * FROM \"Laptops\" WHERE \"Price\" < 800) UNION ALL (SELECT * FROM \"Tablets\" WHERE \"Price\" < 100)", c[2]);

        }

        [Fact]
        public void CombineRaw()
        {
            var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM Devices");

            var c = Compile(query);

            Assert.Equal("(SELECT * FROM [Mobiles]) UNION ALL SELECT * FROM Devices", c[0]);
        }

        [Fact]
        public void CombineRawWithPlaceholders()
        {
            var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM {Devices}");

            var c = Compile(query);

            Assert.Equal("(SELECT * FROM [Mobiles]) UNION ALL SELECT * FROM [Devices]", c[0]);
            Assert.Equal("(SELECT * FROM `Mobiles`) UNION ALL SELECT * FROM `Devices`", c[1]);
        }

        [Fact]
        public void NestedEmptyWhere()
        {
            // Empty nested where should be ignored
            var query = new Query("A").Where(q => new Query().Where(q2 => new Query().Where(q3 => new Query())));

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [A]", c[0]);
        }

        [Fact]
        public void NestedQuery()
        {
            var query = new Query("A").Where(q => new Query("B"));

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [A]", c[0]);
        }

        [Fact]
        public void Count()
        {
            var query = new Query("A").AsCount().Limit(1);

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [A]", c[0]);
            Assert.Equal("SELECT COUNT(*) AS `count` FROM `A`", c[1]);
            Assert.Equal("SELECT COUNT(*) AS \"count\" FROM \"A\"", c[2]);
        }

        [Fact]
        public void Should_Equal_AfterMultipleCompile()
        {
            var query = new Query()
                .Select("Id", "Name")
                .From("Table")
                .OrderBy("Name")
                .Limit(20)
                .Offset(1);

            var first = Compile(query);
            Assert.Equal("SELECT * FROM (SELECT [Id], [Name], ROW_NUMBER() OVER (ORDER BY [Name]) AS [row_num] FROM [Table]) AS [subquery] WHERE [row_num] BETWEEN 2 AND 21", first[0]);
            Assert.Equal("SELECT `Id`, `Name` FROM `Table` ORDER BY `Name` LIMIT 20 OFFSET 1", first[1]);
            Assert.Equal("SELECT \"Id\", \"Name\" FROM \"Table\" ORDER BY \"Name\" LIMIT 20 OFFSET 1", first[2]);

            var second = Compile(query);

            Assert.Equal(first[0], second[0]);
            Assert.Equal(first[1], second[1]);
            Assert.Equal(first[2], second[2]);
        }

        [Fact]
        public void Raw_WrapIdentifiers()
        {
            var query = new Query("Users").SelectRaw("[Id], [Name], {Age}");

            var c = Compile(query);

            Assert.Equal("SELECT [Id], [Name], [Age] FROM [Users]", c[0]);
            Assert.Equal("SELECT `Id`, `Name`, `Age` FROM `Users`", c[1]);
            Assert.Equal("SELECT \"Id\", \"Name\", \"Age\" FROM \"Users\"", c[2]);
        }
    }
}
