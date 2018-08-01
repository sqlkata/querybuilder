using System;
using System.Collections.Generic;
using SqlKata.Execution;
using SqlKata;
using SqlKata.Compilers;
using Xunit;

public class QueryBuilderTest
{
    private readonly Compiler pgsql;
    private readonly MySqlCompiler mysql;
    private readonly FirebirdCompiler fbsql;
    public SqlServerCompiler mssql { get; private set; }

    private string[] Compile(Query q)
    {
        return new[]{
            mssql.Compile(q.Clone()).ToString(),
            mysql.Compile(q.Clone()).ToString(),
            pgsql.Compile(q.Clone()).ToString(),
            fbsql.Compile(q.Clone()).ToString(),
        };
    }
    public QueryBuilderTest()
    {
        mssql = new SqlServerCompiler();
        mysql = new MySqlCompiler();
        pgsql = new PostgresCompiler();
        fbsql = new FirebirdCompiler();
    }

    [Fact]
    public void BasicSelect()
    {
        var q = new Query().From("users").Select("id", "name");
        var c = Compile(q);

        Assert.Equal("SELECT [id], [name] FROM [users]", c[0]);
        Assert.Equal("SELECT `id`, `name` FROM `users`", c[1]);
        Assert.Equal("SELECT \"id\", \"name\" FROM \"users\"", c[2]);
        Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"USERS\"", c[3]);
    }

    [Fact]
    public void BasicSelectWhereBindingIsEmptyOrNull()
    {
        var q = new Query()
            .From("users")
            .Select("id", "name")
            .Where("author", "")
            .OrWhere("author", null);

        var c = Compile(q);

        Assert.Equal("SELECT [id], [name] FROM [users] WHERE [author] = '' OR [author] IS NULL", c[0]);
        Assert.Equal("SELECT `id`, `name` FROM `users` WHERE `author` = '' OR `author` IS NULL", c[1]);
        Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" WHERE \"author\" = '' OR \"author\" IS NULL", c[2]);
        Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"USERS\" WHERE \"AUTHOR\" = '' OR \"AUTHOR\" IS NULL", c[3]);
    }

    [Fact]
    public void BasicSelectWithAlias()
    {
        var q = new Query().From("users as u").Select("id", "name");
        var c = Compile(q);

        Assert.Equal("SELECT [id], [name] FROM [users] AS [u]", c[0]);
        Assert.Equal("SELECT `id`, `name` FROM `users` AS `u`", c[1]);
        Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" AS \"u\"", c[2]);
        Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"USERS\" AS \"U\"", c[3]);
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
        Assert.Equal("SELECT FIRST 10 \"ID\", \"NAME\" FROM \"USERS\"", c[3]);
    }

    [Fact]
    public void Offset()
    {
        var q = new Query().From("users").Offset(10);
        var c = Compile(q);

        Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= 11", c[0]);
        Assert.Equal("SELECT * FROM `users` LIMIT 18446744073709551615 OFFSET 10", c[1]);
        Assert.Equal("SELECT * FROM \"users\" OFFSET 10", c[2]);
        Assert.Equal("SELECT SKIP 10 * FROM \"USERS\"", c[3]);
    }

    [Fact]
    public void LimitOffset()
    {
        var q = new Query().From("users").Offset(10).Limit(5);

        var c = Compile(q);

        Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] BETWEEN 11 AND 15", c[0]);
        Assert.Equal("SELECT * FROM `users` LIMIT 5 OFFSET 10", c[1]);
        Assert.Equal("SELECT * FROM \"users\" LIMIT 5 OFFSET 10", c[2]);
        Assert.Equal("SELECT * FROM \"USERS\" ROWS 11 TO 15", c[3]);
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

        Assert.Equal("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= " + (offset + 1), c.ToString());
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
    public void CteAndBindings()
    {
        var query = new Query("Races")
                    .For("mysql", s =>
                        s.With("range", q =>
                            q.From("seqtbl")
                                .Select("Id").Where("Id", "<", 33))
                                .WhereIn("RaceAuthor", q => q.From("Users")
                                    .Select("Name").Where("Status", "Available")
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
                    .For("firebird",
                        s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d")).Where("Name", "3778")
                    )
                    .Where("Id", ">", 55)
                    .WhereBetween("Value", 18, 24);

        var c = Compile(query);

        Assert.Equal("WITH [range] AS (SELECT [Number] FROM [Sequence] WHERE [Number] < 78)\nSELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Races] WHERE [Id] > 55 AND [Value] BETWEEN 18 AND 24) AS [results_wrapper] WHERE [row_num] BETWEEN 21 AND 45", c[0]);

        Assert.Equal("WITH `range` AS (SELECT `Id` FROM `seqtbl` WHERE `Id` < 33)\nSELECT * FROM `Races` WHERE `RaceAuthor` IN (SELECT `Name` FROM `Users` WHERE `Status` = 'Available') AND `Id` > 55 AND `Value` BETWEEN 18 AND 24", c[1]);

        Assert.Equal("WITH \"range\" AS (SELECT \"d\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"Races\" WHERE \"Name\" = '3778' AND \"Id\" > 55 AND \"Value\" BETWEEN 18 AND 24", c[2]);

        Assert.Equal("WITH \"RANGE\" AS (SELECT \"D\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"RACES\" WHERE \"NAME\" = '3778' AND \"ID\" > 55 AND \"VALUE\" BETWEEN 18 AND 24", c[3]);
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

        Assert.Equal($"WITH [OldBooks] AS (SELECT * FROM [Books] WHERE [Date] < '{now}')\nUPDATE [Books] SET [Price] = '150' WHERE [Price] > 100", c[0]);
    }

    [Fact]
    public void MultipleCte()
    {
        var q1 = new Query("A");
        var q2 = new Query("B");
        var q3 = new Query("C");

        var query = new Query("A")
            .With("A", q1)
            .With("B", q2)
            .With("C", q3);

        var c = Compile(query);

        Assert.Equal("WITH [A] AS (SELECT * FROM [A]),\n[B] AS (SELECT * FROM [B]),\n[C] AS (SELECT * FROM [C])\nSELECT * FROM [A]", c[0]);
    }

    [Fact]
    public void InnerScopeEngineWithinCTE()
    {
        var series = new Query("table")
            .ForPostgres(q => q.WhereRaw("postgres = true"))
            .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
            .ForFirebird(q => q.WhereRaw("firebird = 1"));
        var query = new Query("series").With("series", series);

        var c = Compile(query);

        Assert.Equal("WITH [series] AS (SELECT * FROM [table] WHERE sqlsrv = 1)\nSELECT * FROM [series]", c[0]);

        Assert.Equal("WITH \"series\" AS (SELECT * FROM \"table\" WHERE postgres = true)\nSELECT * FROM \"series\"", c[2]);
        Assert.Equal("WITH \"SERIES\" AS (SELECT * FROM \"TABLE\" WHERE firebird = 1)\nSELECT * FROM \"SERIES\"", c[3]);
    }

    [Fact]
    public void InnerScopeEngineWithinSubQuery()
    {
        var series = new Query("table")
            .ForPostgres(q => q.WhereRaw("postgres = true"))
            .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
            .ForFirebird(q => q.WhereRaw("firebird = 1"));
        var query = new Query("series").From(series.As("series"));

        var c = Compile(query);

        Assert.Equal("SELECT * FROM (SELECT * FROM [table] WHERE sqlsrv = 1) AS [series]", c[0]);

        Assert.Equal("SELECT * FROM (SELECT * FROM \"table\" WHERE postgres = true) AS \"series\"", c[2]);
        Assert.Equal("SELECT * FROM (SELECT * FROM \"TABLE\" WHERE firebird = 1) AS \"SERIES\"", c[3]);
    }


    [Fact]
    public void SqlServerTop()
    {
        var query = new Query("table").Limit(1);
        Assert.Equal("SELECT TOP (@p0) * FROM [table]", mssql.Compile(query).Sql);
    }

    [Fact]
    public void SqlServerTopWithDistinct()
    {
        var query = new Query("table").Limit(1).Distinct();
        Assert.Equal("SELECT DISTINCT TOP (@p0) * FROM [table]", mssql.Compile(query).Sql);
    }

    [Fact]
    public void InsertObject()
    {
        var query = new Query("Table").AsInsert(new
        {
            Name = "The User",
            Age = new DateTime(2018, 1, 1),
        });

        var c = Compile(query);

        Assert.Equal("INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01 00:00:00')", c[0]);


        Assert.Equal("INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01 00:00:00')", c[3]);
    }

    [Fact]
    public void UpdateObject()
    {
        var query = new Query("Table").AsUpdate(new
        {
            Name = "The User",
            Age = new DateTime(2018, 1, 1),
        });

        var c = Compile(query);

        Assert.Equal("UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01 00:00:00'", c[0]);


        Assert.Equal("UPDATE \"TABLE\" SET \"NAME\" = 'The User', \"AGE\" = '2018-01-01 00:00:00'", c[3]);
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

        Assert.Equal("WITH [old_cards] AS (SELECT * FROM [all_cars] WHERE [year] < 2000)\nINSERT INTO [expensive_cars] ([name], [model], [year]) SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [old_cars] WHERE [price] > 100) AS [results_wrapper] WHERE [row_num] BETWEEN 11 AND 20", c[0]);

        Assert.Equal("WITH `old_cards` AS (SELECT * FROM `all_cars` WHERE `year` < 2000)\nINSERT INTO `expensive_cars` (`name`, `model`, `year`) SELECT * FROM `old_cars` WHERE `price` > 100 LIMIT 10 OFFSET 10", c[1]);

        Assert.Equal("WITH \"old_cards\" AS (SELECT * FROM \"all_cars\" WHERE \"year\" < 2000)\nINSERT INTO \"expensive_cars\" (\"name\", \"model\", \"year\") SELECT * FROM \"old_cars\" WHERE \"price\" > 100 LIMIT 10 OFFSET 10", c[2]);
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


        Assert.Equal("INSERT INTO \"EXPENSIVE_CARS\" (\"NAME\", \"BRAND\", \"YEAR\") SELECT 'Chiron', 'Bugatti', NULL FROM RDB$DATABASE UNION ALL SELECT 'Huayra', 'Pagani', 2012 FROM RDB$DATABASE UNION ALL SELECT 'Reventon roadster', 'Lamborghini', 2009 FROM RDB$DATABASE", c[3]);
    }

    [Fact]
    public void InsertWithNullValues()
    {
        var query = new Query("Books").AsInsert(
            new[] { "Id", "Author", "ISBN", "Date" },
            new object[] { 1, "Author 1", "123456", null }
        );

        var c = Compile(query);

        Assert.Equal("INSERT INTO [Books] ([Id], [Author], [ISBN], [Date]) VALUES (1, 'Author 1', '123456', NULL)", c[0]);


        Assert.Equal("INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DATE\") VALUES (1, 'Author 1', '123456', NULL)", c[3]);
    }

    [Fact]
    public void InsertWithEmptyString()
    {
        var query = new Query("Books").AsInsert(
            new[] { "Id", "Author", "ISBN", "Description" },
            new object[] { 1, "Author 1", "123456", "" }
        );

        var c = Compile(query);

        Assert.Equal("INSERT INTO [Books] ([Id], [Author], [ISBN], [Description]) VALUES (1, 'Author 1', '123456', '')", c[0]);


        Assert.Equal("INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DESCRIPTION\") VALUES (1, 'Author 1', '123456', '')", c[3]);
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


        Assert.Equal("UPDATE \"BOOKS\" SET \"AUTHOR\" = 'Author 1', \"DATE\" = NULL, \"VERSION\" = NULL WHERE \"ID\" = 1", c[3]);
    }

    [Fact]
    public void UpdateWithEmptyString()
    {
        var query = new Query("Books").Where("Id", 1).AsUpdate(
            new[] { "Author", "Description" },
            new object[] { "Author 1", "" }
        );

        var c = Compile(query);

        Assert.Equal("UPDATE [Books] SET [Author] = 'Author 1', [Description] = '' WHERE [Id] = 1", c[0]);


        Assert.Equal("UPDATE \"BOOKS\" SET \"AUTHOR\" = 'Author 1', \"DESCRIPTION\" = '' WHERE \"ID\" = 1", c[3]);
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

        Assert.Equal("SELECT * FROM [Phones] UNION (SELECT * FROM [Laptops])", c[0]);


        Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\"", c[3]);
    }


    [Fact]
    public void UnionWithBindings()
    {
        var laptops = new Query("Laptops").Where("Type", "A");
        var mobiles = new Query("Phones").Union(laptops);

        var c = Compile(mobiles);

        Assert.Equal("SELECT * FROM [Phones] UNION (SELECT * FROM [Laptops] WHERE [Type] = 'A')", c[0]);
        Assert.Equal("SELECT * FROM `Phones` UNION (SELECT * FROM `Laptops` WHERE `Type` = 'A')", c[1]);

        Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" WHERE \"TYPE\" = 'A'", c[3]);
    }

    [Fact]
    public void RawUnionWithBindings()
    {
        var mobiles = new Query("Phones").UnionRaw("UNION (SELECT * FROM [Laptops] WHERE [Type] = ?)", "A");

        var c = Compile(mobiles);

        Assert.Equal("SELECT * FROM [Phones] UNION (SELECT * FROM [Laptops] WHERE [Type] = 'A')", c[0]);
        Assert.Equal("SELECT * FROM `Phones` UNION (SELECT * FROM `Laptops` WHERE `Type` = 'A')", c[1]);

    }

    [Fact]
    public void MultipleUnion()
    {
        var laptops = new Query("Laptops");
        var tablets = new Query("Tablets");

        var mobiles = new Query("Phones").Union(laptops).Union(tablets);

        var c = Compile(mobiles);

        Assert.Equal("SELECT * FROM [Phones] UNION (SELECT * FROM [Laptops]) UNION (SELECT * FROM [Tablets])", c[0]);


        Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" UNION SELECT * FROM \"TABLETS\"", c[3]);
    }

    [Fact]
    public void MultipleUnionWithBindings()
    {
        var laptops = new Query("Laptops").Where("Price", ">", 1000);
        var tablets = new Query("Tablets").Where("Price", ">", 2000);

        var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).Union(tablets);

        var c = Compile(mobiles);

        Assert.Equal("SELECT * FROM [Phones] WHERE [Price] < 3000 UNION (SELECT * FROM [Laptops] WHERE [Price] > 1000) UNION (SELECT * FROM [Tablets] WHERE [Price] > 2000)", c[0]);


        Assert.Equal("SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000", c[3]);
    }

    [Fact]
    public void MultipleUnionWithBindingsAndPagination()
    {
        var laptops = new Query("Laptops").Where("Price", ">", 1000);
        var tablets = new Query("Tablets").Where("Price", ">", 2000).ForPage(2);

        var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).UnionAll(tablets);


        var c = Compile(mobiles);

        Assert.Equal("SELECT * FROM [Phones] WHERE [Price] < 3000 UNION (SELECT * FROM [Laptops] WHERE [Price] > 1000) UNION ALL (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Tablets] WHERE [Price] > 2000) AS [results_wrapper] WHERE [row_num] BETWEEN 16 AND 30)", c[0]);


        Assert.Equal("SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000 ROWS 16 TO 30", c[3]);
    }

    [Fact]
    public void UnionWithCallbacks()
    {
        var mobiles = new Query("Phones")
            .Where("Price", "<", 3000)
            .Union(q => q.From("Laptops"))
            .UnionAll(q => q.From("Tablets"));

        var c = Compile(mobiles);

        Assert.Equal("SELECT * FROM [Phones] WHERE [Price] < 3000 UNION (SELECT * FROM [Laptops]) UNION ALL (SELECT * FROM [Tablets])", c[0]);


        Assert.Equal("SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" UNION ALL SELECT * FROM \"TABLETS\"", c[3]);
    }

    [Fact]
    public void UnionWithDifferentEngine()
    {
        var mobiles = new Query("Phones")
            .Where("Price", "<", 300)
            .ForSqlServer(scope => scope.Except(q => q.From("Phones").WhereNot("Os", "iOS")))
            .ForPostgres(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
            .ForMySql(scope => scope.IntersectAll(q => q.From("Watches").Where("Os", "Android")))
            .ForFirebird(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
            .UnionAll(q => q.From("Tablets").Where("Price", "<", 100));

        var c = Compile(mobiles);

        Assert.Equal("SELECT * FROM [Phones] WHERE [Price] < 300 EXCEPT (SELECT * FROM [Phones] WHERE NOT ([Os] = 'iOS')) UNION ALL (SELECT * FROM [Tablets] WHERE [Price] < 100)", c[0]);

        Assert.Equal("SELECT * FROM `Phones` WHERE `Price` < 300 INTERSECT ALL (SELECT * FROM `Watches` WHERE `Os` = 'Android') UNION ALL (SELECT * FROM `Tablets` WHERE `Price` < 100)", c[1]);

        Assert.Equal("SELECT * FROM \"Phones\" WHERE \"Price\" < 300 UNION (SELECT * FROM \"Laptops\" WHERE \"Price\" < 800) UNION ALL (SELECT * FROM \"Tablets\" WHERE \"Price\" < 100)", c[2]);

        Assert.Equal("SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 300 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" < 800 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" < 100", c[3]);
    }

    [Fact]
    public void CombineRaw()
    {
        var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM Devices");

        var c = Compile(query);

        Assert.Equal("SELECT * FROM [Mobiles] UNION ALL SELECT * FROM Devices", c[0]);
    }

    [Fact]
    public void CombineRawWithPlaceholders()
    {
        var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM {Devices}");

        var c = Compile(query);

        Assert.Equal("SELECT * FROM [Mobiles] UNION ALL SELECT * FROM [Devices]", c[0]);
        Assert.Equal("SELECT * FROM `Mobiles` UNION ALL SELECT * FROM `Devices`", c[1]);

        Assert.Equal("SELECT * FROM \"MOBILES\" UNION ALL SELECT * FROM \"Devices\"", c[3]);
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
    public void NestedQueryAfterNestedJoin()
    {

        // in this test, i am testing the compiler dynamic caching functionality
        var query = new Query("users")
        .Join("countries", j => j.On("countries.id", "users.country_id"))
        .Where(q => new Query());

        var c = Compile(query);

        Assert.Equal("SELECT * FROM [users] INNER JOIN [countries] ON ([countries].[id] = [users].[country_id])", c[0]);
    }

    [Fact]
    public void ItShouldCacheMethodInfoByType()
    {
        var compiler = new TestSqlServerCompiler();

        var call1 = compiler.Call_FindCompilerMethodInfo(
            typeof(BasicCondition), "CompileBasicCondition"
        );

        var call2 = compiler.Call_FindCompilerMethodInfo(
            typeof(BasicCondition), "CompileBasicCondition"
        );

        Assert.Same(call1, call2);
    }

    [Fact]
    public void Return_Different_MethodInfo_WhenSame_Method_With_Different_GenericTypes()
    {
        var compiler = new TestSqlServerCompiler();

        var call1 = compiler.Call_FindCompilerMethodInfo(
            typeof(NestedCondition<Query>), "CompileNestedCondition"
        );

        var call2 = compiler.Call_FindCompilerMethodInfo(
            typeof(NestedCondition<Join>), "CompileNestedCondition"
        );

        Assert.NotSame(call1, call2);
    }


    [Fact]
    public void Count()
    {
        var query = new Query("A").AsCount().Limit(1);

        var c = Compile(query);

        Assert.Equal("SELECT COUNT(*) AS [count] FROM [A]", c[0]);
        Assert.Equal("SELECT COUNT(*) AS `count` FROM `A`", c[1]);
        Assert.Equal("SELECT COUNT(*) AS \"count\" FROM \"A\"", c[2]);
        Assert.Equal("SELECT COUNT(*) AS \"COUNT\" FROM \"A\"", c[3]);
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
        Assert.Equal("SELECT * FROM (SELECT [Id], [Name], ROW_NUMBER() OVER (ORDER BY [Name]) AS [row_num] FROM [Table]) AS [results_wrapper] WHERE [row_num] BETWEEN 2 AND 21", first[0]);
        Assert.Equal("SELECT `Id`, `Name` FROM `Table` ORDER BY `Name` LIMIT 20 OFFSET 1", first[1]);
        Assert.Equal("SELECT \"Id\", \"Name\" FROM \"Table\" ORDER BY \"Name\" LIMIT 20 OFFSET 1", first[2]);
        Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"TABLE\" ORDER BY \"NAME\" ROWS 2 TO 21", first[3]);

        var second = Compile(query);

        Assert.Equal(first[0], second[0]);
        Assert.Equal(first[1], second[1]);
        Assert.Equal(first[2], second[2]);
        Assert.Equal(first[3], second[3]);
    }

    [Fact]
    public void Raw_WrapIdentifiers()
    {
        var query = new Query("Users").SelectRaw("[Id], [Name], {Age}");

        var c = Compile(query);

        Assert.Equal("SELECT [Id], [Name], [Age] FROM [Users]", c[0]);
        Assert.Equal("SELECT `Id`, `Name`, `Age` FROM `Users`", c[1]);
        Assert.Equal("SELECT \"Id\", \"Name\", \"Age\" FROM \"Users\"", c[2]);
        Assert.Equal("SELECT \"Id\", \"Name\", \"Age\" FROM \"USERS\"", c[3]);
    }

    [Fact]
    public void NestedEmptyWhereAtFirstCondition()
    {
        var query = new Query("table")
            .Where(q => new Query())
            .Where("id", 1);

        var c = Compile(query);

        Assert.Equal("SELECT * FROM [table] WHERE [id] = 1", c[0]);


        Assert.Equal("SELECT * FROM \"TABLE\" WHERE \"ID\" = 1", c[3]);
    }

    [Fact]
    public void WrapWithSpace()
    {
        var compiler = new SqlServerCompiler();


        Assert.Equal("[My Table] AS [Table]", compiler.Wrap("My Table as Table"));
    }

    [Fact]
    public void WrapWithDotes()
    {
        var compiler = new SqlServerCompiler();


        Assert.Equal("[My Schema].[My Table] AS [Table]", compiler.Wrap("My Schema.My Table as Table"));
    }

    [Fact]
    public void WrapWithMultipleSpaces()
    {
        var compiler = new SqlServerCompiler();


        Assert.Equal("[My Table One] AS [Table One]", compiler.Wrap("My Table One as Table One"));
    }

    [Fact]
    public void WhereTrue()
    {
        var query = new Query("Table").WhereTrue("IsActive");

        var c = Compile(query);

        Assert.Equal("SELECT * FROM [Table] WHERE [IsActive] = cast(1 as bit)", c[0]);
        Assert.Equal("SELECT * FROM `Table` WHERE `IsActive` = true", c[1]);
        Assert.Equal("SELECT * FROM \"Table\" WHERE \"IsActive\" = true", c[2]);
        Assert.Equal("SELECT * FROM \"TABLE\" WHERE \"ISACTIVE\" = 1", c[3]);
    }

    [Fact]
    public void WhereFalse()
    {
        var query = new Query("Table").WhereFalse("IsActive");

        var c = Compile(query);

        Assert.Equal("SELECT * FROM [Table] WHERE [IsActive] = cast(0 as bit)", c[0]);
        Assert.Equal("SELECT * FROM `Table` WHERE `IsActive` = false", c[1]);
        Assert.Equal("SELECT * FROM \"Table\" WHERE \"IsActive\" = false", c[2]);
        Assert.Equal("SELECT * FROM \"TABLE\" WHERE \"ISACTIVE\" = 0", c[3]);
    }




}
