using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using Xunit;

namespace SqlKata.Tests
{
    public class SelectTests : TestSupport
    {
        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [id], [name] FROM [users]")]
        [InlineData(EngineCodes.MySql, "SELECT `id`, `name` FROM `users`")]
        [InlineData(EngineCodes.PostgreSql, "SELECT \"id\", \"name\" FROM \"users\"")]
        [InlineData(EngineCodes.Firebird, "SELECT \"ID\", \"NAME\" FROM \"USERS\"")]
        [InlineData(EngineCodes.Oracle, "SELECT \"id\", \"name\" FROM \"users\"")]
        public void BasicSelect(string engine, string sqlText)
        {
            var query = new Query().From("users").Select("id", "name");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [id], [name] FROM [users]")]
        [InlineData(EngineCodes.MySql, "SELECT `id`, `name` FROM `users`")]
        [InlineData(EngineCodes.PostgreSql, "SELECT \"id\", \"name\" FROM \"users\"")]
        [InlineData(EngineCodes.Firebird, "SELECT \"ID\", \"NAME\" FROM \"USERS\"")]
        [InlineData(EngineCodes.Oracle, "SELECT \"id\", \"name\" FROM \"users\"")]
        public void BasicSelectEnumerable(string engine, string sqlText)
        {
            var query = new Query().From("users").Select(new List<string>() { "id", "name" });

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [id], [name] FROM [users] WHERE [author] = '' OR [author] IS NULL")]
        [InlineData(EngineCodes.MySql, "SELECT `id`, `name` FROM `users` WHERE `author` = '' OR `author` IS NULL")]
        [InlineData(EngineCodes.PostgreSql, "SELECT \"id\", \"name\" FROM \"users\" WHERE \"author\" = '' OR \"author\" IS NULL")]
        [InlineData(EngineCodes.Firebird, "SELECT \"ID\", \"NAME\" FROM \"USERS\" WHERE \"AUTHOR\" = '' OR \"AUTHOR\" IS NULL")]
        [InlineData(EngineCodes.Oracle, "SELECT \"id\", \"name\" FROM \"users\" WHERE \"author\" = '' OR \"author\" IS NULL")]
        public void BasicSelectWhereBindingIsEmptyOrNull(string engine, string sqlText)
        {
            var query = new Query()
                .From("users")
                .Select("id", "name")
                .Where("author", "")
                .OrWhere("author", null);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [id], [name] FROM [users] AS [u]")]
        [InlineData(EngineCodes.MySql, "SELECT `id`, `name` FROM `users` AS `u`")]
        [InlineData(EngineCodes.PostgreSql, "SELECT \"id\", \"name\" FROM \"users\" AS \"u\"")]
        [InlineData(EngineCodes.Firebird, "SELECT \"ID\", \"NAME\" FROM \"USERS\" AS \"U\"")]
        [InlineData(EngineCodes.Oracle, "SELECT \"id\", \"name\" FROM \"users\" \"u\"")]
        public void BasicSelectWithAlias(string engine, string sqlText)
        {
            var query = new Query().From("users as u").Select("id", "name");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [users].[id], [users].[name], [users].[age] FROM [users]")]
        [InlineData(EngineCodes.MySql, "SELECT `users`.`id`, `users`.`name`, `users`.`age` FROM `users`")]
        public void ExpandedSelect(string engine, string sqlText)
        {
            var query = new Query().From("users").Select("users.{id,name, age}");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [users].[id], [users].[name] AS [Name], [users].[age] FROM [users]")]
        [InlineData(EngineCodes.MySql, "SELECT `users`.`id`, `users`.`name` AS `Name`, `users`.`age` FROM `users`")]
        public void ExpandedSelectMultiline(string engine, string sqlText)
        {
            var query = new Query().From("users").Select(@"users.{
                                                                id,
                                                                name as Name,
                                                                age
                                                              }");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void ExpandedSelectWithSchema()
        {
            var query = new Query().From("users").Select("dbo.users.{id,name, age}");
            var c = Compile(query);

            Assert.Equal("SELECT [dbo].[users].[id], [dbo].[users].[name], [dbo].[users].[age] FROM [users]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void ExpandedSelectMultilineWithSchema()
        {
            var query = new Query().From("users").Select(@"dbo.users.{
                                                                id,
                                                                name as Name,
                                                                age
                                                              }");
            var c = Compile(query);

            Assert.Equal("SELECT [dbo].[users].[id], [dbo].[users].[name] AS [Name], [dbo].[users].[age] FROM [users]", c[EngineCodes.SqlServer]);
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [table] WHERE [id] = 1")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"TABLE\" WHERE \"ID\" = 1")]
        public void NestedEmptyWhereAtFirstCondition(string engine, string sqlText)
        {
            var query = new Query("table")
                .Where(q => new Query())
                .Where("id", 1);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [IsActive] = cast(1 as bit)")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `Table` WHERE `IsActive` = true")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table\" WHERE \"IsActive\" = true")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"TABLE\" WHERE \"ISACTIVE\" = 1")]
        public void WhereTrue(string engine, string sqlText)
        {
            var query = new Query("Table").WhereTrue("IsActive");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [IsActive] = cast(0 as bit)")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `Table` WHERE `IsActive` = false")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table\" WHERE \"IsActive\" = false")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"TABLE\" WHERE \"ISACTIVE\" = 0")]
        public void WhereFalse(string engine, string sqlText)
        {
            var query = new Query("Table").WhereFalse("IsActive");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [MyCol] = 'abc' OR [IsActive] = cast(0 as bit)")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table\" WHERE \"MyCol\" = 'abc' OR \"IsActive\" = false")]
        public void OrWhereFalse(string engine, string sqlText)
        {
            var query = new Query("Table").Where("MyCol", "abc").OrWhereFalse("IsActive");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [MyCol] = 'abc' OR [IsActive] = cast(1 as bit)")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table\" WHERE \"MyCol\" = 'abc' OR \"IsActive\" = true")]
        public void OrWhereTrue(string engine, string sqlText)
        {
            var query = new Query("Table").Where("MyCol", "abc").OrWhereTrue("IsActive");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());

        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [MyCol] = 'abc' OR [IsActive] IS NULL")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table\" WHERE \"MyCol\" = 'abc' OR \"IsActive\" IS NULL")]
        public void OrWhereNull(string engine, string sqlText)
        {
            var query = new Query("Table").Where("MyCol", "abc").OrWhereNull("IsActive");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE (SELECT COUNT(*) AS [count] FROM [Table2] WHERE [Table2].[Column] = [Table].[MyCol]) = 1")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table\" WHERE (SELECT COUNT(*) AS \"count\" FROM \"Table2\" WHERE \"Table2\".\"Column\" = \"Table\".\"MyCol\") = 1")]
        public void WhereSub(string engine, string sqlText)
        {
            var subQuery = new Query("Table2").WhereColumns("Table2.Column", "=", "Table.MyCol").AsCount();

            var query = new Query("Table").WhereSub(subQuery, 1);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [MyCol] IS NULL OR (SELECT COUNT(*) AS [count] FROM [Table2] WHERE [Table2].[Column] = [Table].[MyCol]) < 1")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table\" WHERE \"MyCol\" IS NULL OR (SELECT COUNT(*) AS \"count\" FROM \"Table2\" WHERE \"Table2\".\"Column\" = \"Table\".\"MyCol\") < 1")]
        public void OrWhereSub(string engine, string sqlText)
        {
            var subQuery = new Query("Table2").WhereColumns("Table2.Column", "=", "Table.MyCol").AsCount();

            var query = new Query("Table").WhereNull("MyCol").OrWhereSub(subQuery, "<", 1);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void PassingArrayAsParameter()
        {
            var query = new Query("Table").WhereRaw("[Id] in (?)", new object[] { new object[] { 1, 2, 3 } });

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE [Id] in (1,2,3)", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void UsingJsonArray()
        {
            var query = new Query("Table").WhereRaw("[Json]->'address'->>'country' in (?)", new[] { 1, 2, 3, 4 });

            var c = Compile(query);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"Json\"->'address'->>'country' in (1,2,3,4)", c[EngineCodes.PostgreSql]);
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] UNION SELECT * FROM [Laptops]")]
        [InlineData(EngineCodes.Sqlite, "SELECT * FROM \"Phones\" UNION SELECT * FROM \"Laptops\"")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\"")]
        public void Union(string engine, string sqlText)
        {
            var laptops = new Query("Laptops");
            var mobiles = new Query("Phones").Union(laptops);

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] WHERE [Type] = 'A'")]
        [InlineData(EngineCodes.Sqlite, "SELECT * FROM \"Phones\" UNION SELECT * FROM \"Laptops\" WHERE \"Type\" = 'A'")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `Phones` UNION SELECT * FROM `Laptops` WHERE `Type` = 'A'")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" WHERE \"TYPE\" = 'A'")]
        public void UnionWithBindings(string engine, string sqlText)
        {
            var laptops = new Query("Laptops").Where("Type", "A");
            var mobiles = new Query("Phones").Union(laptops);

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] WHERE [Type] = 'A'")]
        [InlineData(EngineCodes.Sqlite, "SELECT * FROM \"Phones\" UNION SELECT * FROM \"Laptops\" WHERE \"Type\" = 'A'")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `Phones` UNION SELECT * FROM `Laptops` WHERE `Type` = 'A'")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" UNION SELECT * FROM \"Laptops\" WHERE \"Type\" = 'A'")] // Is this good?
        public void RawUnionWithBindings(string engine, string sqlText)
        {
            var mobiles = new Query("Phones").UnionRaw("UNION SELECT * FROM [Laptops] WHERE [Type] = ?", "A");

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] UNION SELECT * FROM [Tablets]")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" UNION SELECT * FROM \"TABLETS\"")]
        public void MultipleUnion(string engine, string sqlText)
        {
            var laptops = new Query("Laptops");
            var tablets = new Query("Tablets");

            var mobiles = new Query("Phones").Union(laptops).Union(tablets);

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());

        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] WHERE [Price] > 1000 UNION SELECT * FROM [Tablets] WHERE [Price] > 2000")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000")]
        public void MultipleUnionWithBindings(string engine, string sqlText)
        {
            var laptops = new Query("Laptops").Where("Price", ">", 1000);
            var tablets = new Query("Tablets").Where("Price", ">", 2000);

            var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).Union(tablets);

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] WHERE [Price] > 1000 UNION ALL SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Tablets] WHERE [Price] > 2000) AS [results_wrapper] WHERE [row_num] BETWEEN 16 AND 30")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000 ROWS 16 TO 30")]
        public void MultipleUnionWithBindingsAndPagination(string engine, string sqlText)
        {
            var laptops = new Query("Laptops").Where("Price", ">", 1000);
            var tablets = new Query("Tablets").Where("Price", ">", 2000).ForPage(2);

            var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).UnionAll(tablets);

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] UNION ALL SELECT * FROM [Tablets]")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" UNION ALL SELECT * FROM \"TABLETS\"")]
        public void UnionWithCallbacks(string engine, string sqlText)
        {
            var mobiles = new Query("Phones")
                .Where("Price", "<", 3000)
                .Union(q => q.From("Laptops"))
                .UnionAll(q => q.From("Tablets"));

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Phones] WHERE [Price] < 300 EXCEPT SELECT * FROM [Phones] WHERE NOT ([Os] = 'iOS') UNION ALL SELECT * FROM [Tablets] WHERE [Price] < 100")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `Phones` WHERE `Price` < 300 INTERSECT ALL SELECT * FROM `Watches` WHERE `Os` = 'Android' UNION ALL SELECT * FROM `Tablets` WHERE `Price` < 100")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Phones\" WHERE \"Price\" < 300 UNION SELECT * FROM \"Laptops\" WHERE \"Price\" < 800 UNION ALL SELECT * FROM \"Tablets\" WHERE \"Price\" < 100")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 300 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" < 800 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" < 100")]
        public void UnionWithDifferentEngine(string engine, string sqlText)
        {
            var mobiles = new Query("Phones")
                .Where("Price", "<", 300)
                .ForSqlServer(scope => scope.Except(q => q.From("Phones").WhereNot("Os", "iOS")))
                .ForPostgreSql(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
                .ForMySql(scope => scope.IntersectAll(q => q.From("Watches").Where("Os", "Android")))
                .ForFirebird(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
                .UnionAll(q => q.From("Tablets").Where("Price", "<", 100));

            var c = CompileFor(engine, mobiles);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void CombineRaw()
        {
            var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM Devices");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Mobiles] UNION ALL SELECT * FROM Devices", c[EngineCodes.SqlServer]);
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Mobiles] UNION ALL SELECT * FROM [Devices]")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `Mobiles` UNION ALL SELECT * FROM `Devices`")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"MOBILES\" UNION ALL SELECT * FROM \"Devices\"")]
        public void CombineRawWithPlaceholders(string engine, string sqlText)
        {
            var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM {Devices}");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void NestedEmptyWhere()
        {
            // Empty nested where should be ignored
            var query = new Query("A").Where(q => new Query().Where(q2 => new Query().Where(q3 => new Query())));

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void NestedQuery()
        {
            var query = new Query("A").Where(q => new Query("B"));

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void NestedQueryAfterNestedJoin()
        {
            // in this test, i am testing the compiler dynamic caching functionality
            var query = new Query("users")
                .Join("countries", j => j.On("countries.id", "users.country_id"))
                .Where(q => new Query());

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [users] \nINNER JOIN [countries] ON ([countries].[id] = [users].[country_id])",
                c[EngineCodes.SqlServer]);
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

            Assert.Equal(
                "WITH [A] AS (SELECT * FROM [A]),\n[B] AS (SELECT * FROM [B]),\n[C] AS (SELECT * FROM [C])\nSELECT * FROM [A]",
                c[EngineCodes.SqlServer]);
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "WITH [range] AS (SELECT [Number] FROM [Sequence] WHERE [Number] < 78)\nSELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Races] WHERE [Id] > 55 AND [Value] BETWEEN 18 AND 24) AS [results_wrapper] WHERE [row_num] BETWEEN 21 AND 45")]
        [InlineData(EngineCodes.MySql, "WITH `range` AS (SELECT `Id` FROM `seqtbl` WHERE `Id` < 33)\nSELECT * FROM `Races` WHERE `RaceAuthor` IN (SELECT `Name` FROM `Users` WHERE `Status` = 'Available') AND `Id` > 55 AND `Value` BETWEEN 18 AND 24")]
        [InlineData(EngineCodes.PostgreSql, "WITH \"range\" AS (SELECT \"d\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"Races\" WHERE \"Name\" = '3778' AND \"Id\" > 55 AND \"Value\" BETWEEN 18 AND 24")]
        [InlineData(EngineCodes.Firebird, "WITH \"RANGE\" AS (SELECT \"D\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"RACES\" WHERE \"NAME\" = '3778' AND \"ID\" > 55 AND \"VALUE\" BETWEEN 18 AND 24")]
        public void CteAndBindings(string engine, string sqlText)
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
                    s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d"))
                        .Where("Name", "3778")
                )
                .For("firebird",
                    s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d"))
                        .Where("Name", "3778")
                )
                .Where("Id", ">", 55)
                .WhereBetween("Value", 18, 24);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        // test for issue #50
        [Theory]
        [InlineData(EngineCodes.SqlServer, "WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2)\nSELECT * FROM [cte2] WHERE [Column3] = 5")]
        [InlineData(EngineCodes.MySql, "WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2)\nSELECT * FROM `cte2` WHERE `Column3` = 5")]
        [InlineData(EngineCodes.PostgreSql, "WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2)\nSELECT * FROM \"cte2\" WHERE \"Column3\" = 5")]
        [InlineData(EngineCodes.Firebird, "WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2)\nSELECT * FROM \"CTE2\" WHERE \"COLUMN3\" = 5")]
        public void CascadedCteAndBindings(string engine, string sqlText)
        {
            var cte1 = new Query("Table1");
            cte1.Select("Column1", "Column2");
            cte1.Where("Column2", 1);

            var cte2 = new Query("Table2");
            cte2.With("cte1", cte1);
            cte2.Select("Column3", "Column4");
            cte2.Join("cte1", join => join.On("Column1", "Column3"));
            cte2.Where("Column4", 2);

            var mainQuery = new Query("Table3");
            mainQuery.With("cte2", cte2);
            mainQuery.Select("*");
            mainQuery.From("cte2");
            mainQuery.Where("Column3", 5);

            var c = CompileFor(engine, mainQuery);

            Assert.Equal(sqlText, c.ToString());
        }

        // test for issue #50
        [Theory]
        [InlineData(EngineCodes.SqlServer, "WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2),\n[cte3] AS (SELECT [Column3_3], [Column3_4] FROM [Table3] \nINNER JOIN [cte1] ON ([Column1] = [Column3_3]) WHERE [Column3_4] = 33)\nSELECT * FROM [cte2] WHERE [Column3] = 5")]
        [InlineData(EngineCodes.MySql, "WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2),\n`cte3` AS (SELECT `Column3_3`, `Column3_4` FROM `Table3` \nINNER JOIN `cte1` ON (`Column1` = `Column3_3`) WHERE `Column3_4` = 33)\nSELECT * FROM `cte2` WHERE `Column3` = 5")]
        [InlineData(EngineCodes.PostgreSql, "WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2),\n\"cte3\" AS (SELECT \"Column3_3\", \"Column3_4\" FROM \"Table3\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3_3\") WHERE \"Column3_4\" = 33)\nSELECT * FROM \"cte2\" WHERE \"Column3\" = 5")]
        [InlineData(EngineCodes.Firebird, "WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2),\n\"CTE3\" AS (SELECT \"COLUMN3_3\", \"COLUMN3_4\" FROM \"TABLE3\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3_3\") WHERE \"COLUMN3_4\" = 33)\nSELECT * FROM \"CTE2\" WHERE \"COLUMN3\" = 5")]
        public void CascadedAndMultiReferencedCteAndBindings(string engine, string sqlText)
        {
            var cte1 = new Query("Table1");
            cte1.Select("Column1", "Column2");
            cte1.Where("Column2", 1);

            var cte2 = new Query("Table2");
            cte2.With("cte1", cte1);
            cte2.Select("Column3", "Column4");
            cte2.Join("cte1", join => join.On("Column1", "Column3"));
            cte2.Where("Column4", 2);

            var cte3 = new Query("Table3");
            cte3.With("cte1", cte1);
            cte3.Select("Column3_3", "Column3_4");
            cte3.Join("cte1", join => join.On("Column1", "Column3_3"));
            cte3.Where("Column3_4", 33);

            var mainQuery = new Query("Table3");
            mainQuery.With("cte2", cte2);
            mainQuery.With("cte3", cte3);
            mainQuery.Select("*");
            mainQuery.From("cte2");
            mainQuery.Where("Column3", 5);

            var c = CompileFor(engine, mainQuery);

            Assert.Equal(sqlText, c.ToString());
        }

        // test for issue #50
        [Theory]
        [InlineData(EngineCodes.SqlServer, "WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2),\n[cte3] AS (SELECT [Column3_3], [Column3_4] FROM [Table3] \nINNER JOIN [cte1] ON ([Column1] = [Column3_3]) WHERE [Column3_4] = 33)\nSELECT * FROM [cte3] WHERE [Column3_4] = 5")]
        [InlineData(EngineCodes.MySql, "WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2),\n`cte3` AS (SELECT `Column3_3`, `Column3_4` FROM `Table3` \nINNER JOIN `cte1` ON (`Column1` = `Column3_3`) WHERE `Column3_4` = 33)\nSELECT * FROM `cte3` WHERE `Column3_4` = 5")]
        [InlineData(EngineCodes.PostgreSql, "WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2),\n\"cte3\" AS (SELECT \"Column3_3\", \"Column3_4\" FROM \"Table3\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3_3\") WHERE \"Column3_4\" = 33)\nSELECT * FROM \"cte3\" WHERE \"Column3_4\" = 5")]
        [InlineData(EngineCodes.Firebird, "WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2),\n\"CTE3\" AS (SELECT \"COLUMN3_3\", \"COLUMN3_4\" FROM \"TABLE3\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3_3\") WHERE \"COLUMN3_4\" = 33)\nSELECT * FROM \"CTE3\" WHERE \"COLUMN3_4\" = 5")]
        public void MultipleCtesAndBindings(string engine, string sqlText)
        {
            var cte1 = new Query("Table1");
            cte1.Select("Column1", "Column2");
            cte1.Where("Column2", 1);

            var cte2 = new Query("Table2");
            cte2.Select("Column3", "Column4");
            cte2.Join("cte1", join => join.On("Column1", "Column3"));
            cte2.Where("Column4", 2);

            var cte3 = new Query("Table3");
            cte3.Select("Column3_3", "Column3_4");
            cte3.Join("cte1", join => join.On("Column1", "Column3_3"));
            cte3.Where("Column3_4", 33);

            var mainQuery = new Query("Table3");
            mainQuery.With("cte1", cte1);
            mainQuery.With("cte2", cte2);
            mainQuery.With("cte3", cte3);
            mainQuery.Select("*");
            mainQuery.From("cte3");
            mainQuery.Where("Column3_4", 5);

            var c = CompileFor(engine, mainQuery);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT TOP (10) [id], [name] FROM [users]")]
        [InlineData(EngineCodes.MySql, "SELECT `id`, `name` FROM `users` LIMIT 10")]
        [InlineData(EngineCodes.PostgreSql, "SELECT \"id\", \"name\" FROM \"users\" LIMIT 10")]
        [InlineData(EngineCodes.Firebird, "SELECT FIRST 10 \"ID\", \"NAME\" FROM \"USERS\"")]
        public void Limit(string engine, string sqlText)
        {
            var query = new Query().From("users").Select("id", "name").Limit(10);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= 11")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `users` LIMIT 18446744073709551615 OFFSET 10")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"users\" OFFSET 10")]
        [InlineData(EngineCodes.Firebird, "SELECT SKIP 10 * FROM \"USERS\"")]
        public void Offset(string engine, string sqlText)
        {
            var query = new Query().From("users").Offset(10);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] BETWEEN 11 AND 15")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `users` LIMIT 5 OFFSET 10")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"users\" LIMIT 5 OFFSET 10")]
        [InlineData(EngineCodes.Firebird, "SELECT * FROM \"USERS\" ROWS 11 TO 15")]
        public void LimitOffset(string engine, string sqlText)
        {
            var query = new Query().From("users").Offset(10).Limit(5);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [users] \nINNER JOIN [countries] ON [countries].[id] = [users].[country_id]")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `users` \nINNER JOIN `countries` ON `countries`.`id` = `users`.`country_id`")]
        public void BasicJoin(string engine, string sqlText)
        {
            var query = new Query().From("users").Join("countries", "countries.id", "users.country_id");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
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

        public void OrWhereRawEscaped()
        {
            var query = new Query("Table").WhereRaw("[MyCol] = ANY(?::int\\[\\])", "{1,2,3}");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"MyCol\" = ANY('{1,2,3}'::int[])", c[EngineCodes.PostgreSql]);
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] HAVING [Column1] > 1")]
        public void Having(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .Having("Column1", ">", 1);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] HAVING [Column1] > 1 AND [Column2] = 1")]
        public void MultipleHaving(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .Having("Column1", ">", 1)
                .Having("Column2", "=", 1);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] HAVING [Column1] > 1 OR [Column2] = 1")]
        public void MultipleOrHaving(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .Having("Column1", ">", 1)
                .OrHaving("Column2", "=", 1);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%upper word%'")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Table1\" WHERE \"Column1\" ilike '%Upper Word%'")]
        public void ShouldUseILikeOnPostgresWhenNonCaseSensitive(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .WhereLike("Column1", "%Upper Word%", false);

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] WHERE LOWER([Column1]) like 'teststring\\%' ESCAPE '\\'")]
        public void EscapedWhereLike(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .WhereLike("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] WHERE LOWER([Column1]) like 'teststring\\%%' ESCAPE '\\'")]
        public void EscapedWhereStarts(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .WhereStarts("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%teststring\\%' ESCAPE '\\'")]
        public void EscapedWhereEnds(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .WhereEnds("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%teststring\\%%' ESCAPE '\\'")]
        public void EscapedWhereContains(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .WhereContains("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] HAVING LOWER([Column1]) like 'teststring\\%' ESCAPE '\\'")]
        public void EscapedHavingLike(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .HavingLike("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] HAVING LOWER([Column1]) like 'teststring\\%%' ESCAPE '\\'")]
        public void EscapedHavingStarts(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .HavingStarts("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] HAVING LOWER([Column1]) like '%teststring\\%' ESCAPE '\\'")]
        public void EscapedHavingEnds(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .HavingEnds("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table1] HAVING LOWER([Column1]) like '%teststring\\%%' ESCAPE '\\'")]
        public void EscapedHavingContains(string engine, string sqlText)
        {
            var query = new Query("Table1")
                .HavingContains("Column1", @"TestString\%", false, @"\");

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void EscapeClauseThrowsForMultipleCharacters()
        {
            Assert.ThrowsAny<ArgumentException>(() =>
            {
                var q = new Query("Table1")
                    .HavingContains("Column1", @"TestString\%", false, @"\aa");
            });
        }


        [Fact]
        public void BasicSelectRaw_WithNoTable()
        {
            var q = new Query().SelectRaw("somefunction() as c1");

            var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT somefunction() as c1", c.ToString());
        }

        [Fact]
        public void BasicSelect_WithNoTable()
        {
            var q = new Query().Select("c1");
            var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT [c1]", c.ToString());
        }

        [Fact]
        public void BasicSelect_WithNoTableAndWhereClause()
        {
            var q = new Query().Select("c1").Where("p", 1);
            var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT [c1] WHERE [p] = 1", c.ToString());
        }

        [Fact]
        public void BasicSelect_WithNoTableWhereRawClause()
        {
            var q = new Query().Select("c1").WhereRaw("1 = 1");
            var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT [c1] WHERE 1 = 1", c.ToString());
        }

        [Fact]
        public void BasicSelectAggregate()
        {
            var q = new Query("Posts").Select("Title")
                .SelectAggregate("sum", "ViewCount");

            var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT [Title], SUM([ViewCount]) FROM [Posts]", sqlServer.ToString());
        }

        [Fact]
        public void SelectAggregateShouldIgnoreEmptyFilter()
        {
            var q = new Query("Posts").Select("Title")
                .SelectAggregate("sum", "ViewCount", q => q);

            var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT [Title], SUM([ViewCount]) FROM [Posts]", sqlServer.ToString());
        }

        [Fact]
        public void SelectAggregateShouldIgnoreEmptyQueryFilter()
        {
            var q = new Query("Posts").Select("Title")
                .SelectAggregate("sum", "ViewCount", new Query());

            var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT [Title], SUM([ViewCount]) FROM [Posts]", sqlServer.ToString());
        }

        [Fact]
        public void BasicSelectAggregateWithAlias()
        {
            var q = new Query("Posts").Select("Title")
                .SelectAggregate("sum", "ViewCount as TotalViews");

            var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal("SELECT [Title], SUM([ViewCount]) AS [TotalViews] FROM [Posts]", sqlServer.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [Title], SUM(CASE WHEN [Published_Month] = 'Jan' THEN [ViewCount] END) AS [Published_Jan], SUM(CASE WHEN [Published_Month] = 'Feb' THEN [ViewCount] END) AS [Published_Feb] FROM [Posts]")]
        [InlineData(EngineCodes.PostgreSql, "SELECT \"Title\", SUM(\"ViewCount\") FILTER (WHERE \"Published_Month\" = 'Jan') AS \"Published_Jan\", SUM(\"ViewCount\") FILTER (WHERE \"Published_Month\" = 'Feb') AS \"Published_Feb\" FROM \"Posts\"")]
        public void SelectWithFilter(string engine, string sqlText)
        {
            var query = new Query("Posts").Select("Title")
                .SelectAggregate("sum", "ViewCount as Published_Jan", q => q.Where("Published_Month", "Jan"))
                .SelectAggregate("sum", "ViewCount as Published_Feb", q => q.Where("Published_Month", "Feb"));

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Posts] WHERE EXISTS (SELECT 1 FROM [Comments] WHERE [Comments].[PostId] = [Posts].[Id])")]
        public void SelectWithExists(string engine, string sqlText)
        {
            var query = new Query("Posts").WhereExists(
                new Query("Comments").WhereColumns("Comments.PostId", "=", "Posts.Id")
            );

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void SelectWithExists_OmitSelectIsFalse()
        {
            var q = new Query("Posts").WhereExists(
                new Query("Comments").Select("Id").WhereColumns("Comments.PostId", "=", "Posts.Id")
            );


            var compiler = new SqlServerCompiler
            {
                OmitSelectInsideExists = false,
            };

            var sqlServer = compiler.Compile(q).ToString();
            Assert.Equal("SELECT * FROM [Posts] WHERE EXISTS (SELECT [Id] FROM [Comments] WHERE [Comments].[PostId] = [Posts].[Id])", sqlServer.ToString());
        }
    }
}
