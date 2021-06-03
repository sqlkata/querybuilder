using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using System;
using Xunit;

namespace SqlKata.Tests
{
    public class SelectTests : TestSupport
    {
        [Fact]
        public void BasicSelect()
        {
            var q = new Query().From("users").Select("id", "name");
            var c = Compile(q);

            Assert.Equal("SELECT [id], [name] FROM [users]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `id`, `name` FROM `users`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"USERS\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\"", c[EngineCodes.Oracle]);
        }

        [Fact]
        public void SelectAs()
        {
            var query = new Query().SelectAs(("Row", "Alias")).From("Table");

            var c = Compile(query);
            Assert.Equal("SELECT [Row] AS [Alias] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `Row` AS `Alias` FROM `Table`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"Row\" AS \"Alias\" FROM \"Table\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"ROW\" AS \"ALIAS\" FROM \"TABLE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT \"Row\" \"Alias\" FROM \"Table\"", c[EngineCodes.Oracle]);
        }

        [Fact]
        public void SelectAsMultipleColumns()
        {
            var query = new Query().SelectAs(("Row1", "Alias1"), ("Row2", "Alias2")).From("Table");

            var c = Compile(query);
            Assert.Equal("SELECT [Row1] AS [Alias1], [Row2] AS [Alias2] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `Row1` AS `Alias1`, `Row2` AS `Alias2` FROM `Table`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"Row1\" AS \"Alias1\", \"Row2\" AS \"Alias2\" FROM \"Table\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"ROW1\" AS \"ALIAS1\", \"ROW2\" AS \"ALIAS2\" FROM \"TABLE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT \"Row1\" \"Alias1\", \"Row2\" \"Alias2\" FROM \"Table\"", c[EngineCodes.Oracle]);
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

            Assert.Equal("SELECT [id], [name] FROM [users] WHERE [author] = '' OR [author] IS NULL", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `id`, `name` FROM `users` WHERE `author` = '' OR `author` IS NULL", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" WHERE \"author\" = '' OR \"author\" IS NULL", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"USERS\" WHERE \"AUTHOR\" = '' OR \"AUTHOR\" IS NULL", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void BasicSelectWithAlias()
        {
            var q = new Query().From("users as u").Select("id", "name");
            var c = Compile(q);

            Assert.Equal("SELECT [id], [name] FROM [users] AS [u]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `id`, `name` FROM `users` AS `u`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" AS \"u\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"USERS\" AS \"U\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void ExpandedSelect()
        {
            var q = new Query().From("users").Select("users.{id,name, age}");
            var c = Compile(q);

            Assert.Equal("SELECT [users].[id], [users].[name], [users].[age] FROM [users]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `users`.`id`, `users`.`name`, `users`.`age` FROM `users`", c[EngineCodes.MySql]);
        }

        [Fact]
        public void ExpandedSelectAs()
        {
            var q = new Query().From("users").SelectAs(("users.{id,name, age}", "Alias"));
            var c = Compile(q);

            // This result is weird (but valid syntax), and at least it works in
            // a somewhat explainable way, as opposed to regular Select() when
            // combining the expanded syntax and the 'as' SQLKata keyword support
            // which simply silently stops working when the {...} expansion is
            // applied.
            Assert.Equal("SELECT [users].[id] AS [Alias], [users].[name] AS [Alias], [users].[age] AS [Alias] FROM [users]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `users`.`id` AS `Alias`, `users`.`name` AS `Alias`, `users`.`age` AS `Alias` FROM `users`", c[EngineCodes.MySql]);
        }

        [Fact]
        public void ExpandedSelectWithSchema()
        {
            var q = new Query().From("users").Select("dbo.users.{id,name, age}");
            var c = Compile(q);

            Assert.Equal("SELECT [dbo].[users].[id], [dbo].[users].[name], [dbo].[users].[age] FROM [users]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void NestedEmptyWhereAtFirstCondition()
        {
            var query = new Query("table")
                .Where(q => new Query())
                .Where("id", 1);

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [table] WHERE [id] = 1", c[EngineCodes.SqlServer]);


            Assert.Equal("SELECT * FROM \"TABLE\" WHERE \"ID\" = 1", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void WhereTrue()
        {
            var query = new Query("Table").WhereTrue("IsActive");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE [IsActive] = cast(1 as bit)", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM `Table` WHERE `IsActive` = true", c[EngineCodes.MySql]);
            Assert.Equal("SELECT * FROM \"Table\" WHERE \"IsActive\" = true", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT * FROM \"TABLE\" WHERE \"ISACTIVE\" = 1", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void WhereFalse()
        {
            var query = new Query("Table").WhereFalse("IsActive");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE [IsActive] = cast(0 as bit)", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM `Table` WHERE `IsActive` = false", c[EngineCodes.MySql]);
            Assert.Equal("SELECT * FROM \"Table\" WHERE \"IsActive\" = false", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT * FROM \"TABLE\" WHERE \"ISACTIVE\" = 0", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void OrWhereFalse()
        {
            var query = new Query("Table").Where("MyCol", "abc").OrWhereFalse("IsActive");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE [MyCol] = 'abc' OR [IsActive] = cast(0 as bit)", c[EngineCodes.SqlServer]);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"MyCol\" = 'abc' OR \"IsActive\" = false", c[EngineCodes.PostgreSql]);

        }

        [Fact]
        public void OrWhereTrue()
        {
            var query = new Query("Table").Where("MyCol", "abc").OrWhereTrue("IsActive");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE [MyCol] = 'abc' OR [IsActive] = cast(1 as bit)", c[EngineCodes.SqlServer]);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"MyCol\" = 'abc' OR \"IsActive\" = true", c[EngineCodes.PostgreSql]);

        }

        [Fact]
        public void OrWhereNull()
        {
            var query = new Query("Table").Where("MyCol", "abc").OrWhereNull("IsActive");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE [MyCol] = 'abc' OR [IsActive] IS NULL", c[EngineCodes.SqlServer]);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"MyCol\" = 'abc' OR \"IsActive\" IS NULL", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void WhereSub()
        {
            var subQuery = new Query("Table2").WhereColumns("Table2.Column", "=", "Table.MyCol").CountAs();

            var query = new Query("Table").WhereSub(subQuery, 1);

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE (SELECT COUNT(*) AS [count] FROM [Table2] WHERE [Table2].[Column] = [Table].[MyCol]) = 1", c[EngineCodes.SqlServer]);

            Assert.Equal("SELECT * FROM \"Table\" WHERE (SELECT COUNT(*) AS \"count\" FROM \"Table2\" WHERE \"Table2\".\"Column\" = \"Table\".\"MyCol\") = 1", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void OrWhereSub()
        {
            var subQuery = new Query("Table2").WhereColumns("Table2.Column", "=", "Table.MyCol").CountAs();

            var query = new Query("Table").WhereNull("MyCol").OrWhereSub(subQuery, "<", 1);

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Table] WHERE [MyCol] IS NULL OR (SELECT COUNT(*) AS [count] FROM [Table2] WHERE [Table2].[Column] = [Table].[MyCol]) < 1", c[EngineCodes.SqlServer]);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"MyCol\" IS NULL OR (SELECT COUNT(*) AS \"count\" FROM \"Table2\" WHERE \"Table2\".\"Column\" = \"Table\".\"MyCol\") < 1", c[EngineCodes.PostgreSql]);
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

        [Fact]
        public void Union()
        {
            var laptops = new Query("Laptops");
            var mobiles = new Query("Phones").Union(laptops);

            var c = Compile(mobiles);

            Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM \"Phones\" UNION SELECT * FROM \"Laptops\"", c[EngineCodes.Sqlite]);
            Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\"", c[EngineCodes.Firebird]);
        }


        [Fact]
        public void UnionWithBindings()
        {
            var laptops = new Query("Laptops").Where("Type", "A");
            var mobiles = new Query("Phones").Union(laptops);

            var c = Compile(mobiles);

            Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] WHERE [Type] = 'A'", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM \"Phones\" UNION SELECT * FROM \"Laptops\" WHERE \"Type\" = 'A'", c[EngineCodes.Sqlite]);
            Assert.Equal("SELECT * FROM `Phones` UNION SELECT * FROM `Laptops` WHERE `Type` = 'A'", c[EngineCodes.MySql]);
            Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" WHERE \"TYPE\" = 'A'", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void RawUnionWithBindings()
        {
            var mobiles = new Query("Phones").UnionRaw("UNION SELECT * FROM [Laptops] WHERE [Type] = ?", "A");

            var c = Compile(mobiles);

            Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] WHERE [Type] = 'A'", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM `Phones` UNION SELECT * FROM `Laptops` WHERE `Type` = 'A'", c[EngineCodes.MySql]);
        }

        [Fact]
        public void MultipleUnion()
        {
            var laptops = new Query("Laptops");
            var tablets = new Query("Tablets");

            var mobiles = new Query("Phones").Union(laptops).Union(tablets);

            var c = Compile(mobiles);

            Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] UNION SELECT * FROM [Tablets]",
                c[EngineCodes.SqlServer]);


            Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" UNION SELECT * FROM \"TABLETS\"",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void MultipleUnionWithBindings()
        {
            var laptops = new Query("Laptops").Where("Price", ">", 1000);
            var tablets = new Query("Tablets").Where("Price", ">", 2000);

            var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).Union(tablets);

            var c = Compile(mobiles);

            Assert.Equal(
                "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] WHERE [Price] > 1000 UNION SELECT * FROM [Tablets] WHERE [Price] > 2000",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void MultipleUnionWithBindingsAndPagination()
        {
            var laptops = new Query("Laptops").Where("Price", ">", 1000);
            var tablets = new Query("Tablets").Where("Price", ">", 2000).ForPage(2);

            var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).UnionAll(tablets);


            var c = Compile(mobiles);

            Assert.Equal(
                "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] WHERE [Price] > 1000 UNION ALL SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Tablets] WHERE [Price] > 2000) AS [results_wrapper] WHERE [row_num] BETWEEN 16 AND 30",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000 ROWS 16 TO 30",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UnionWithCallbacks()
        {
            var mobiles = new Query("Phones")
                .Where("Price", "<", 3000)
                .Union(q => q.From("Laptops"))
                .UnionAll(q => q.From("Tablets"));

            var c = Compile(mobiles);

            Assert.Equal(
                "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] UNION ALL SELECT * FROM [Tablets]",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" UNION ALL SELECT * FROM \"TABLETS\"",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UnionWithDifferentEngine()
        {
            var mobiles = new Query("Phones")
                .Where("Price", "<", 300)
                .ForSqlServer(scope => scope.Except(q => q.From("Phones").WhereNot("Os", "iOS")))
                .ForPostgreSql(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
                .ForMySql(scope => scope.IntersectAll(q => q.From("Watches").Where("Os", "Android")))
                .ForFirebird(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
                .UnionAll(q => q.From("Tablets").Where("Price", "<", 100));

            var c = Compile(mobiles);

            Assert.Equal(
                "SELECT * FROM [Phones] WHERE [Price] < 300 EXCEPT SELECT * FROM [Phones] WHERE NOT ([Os] = 'iOS') UNION ALL SELECT * FROM [Tablets] WHERE [Price] < 100",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "SELECT * FROM `Phones` WHERE `Price` < 300 INTERSECT ALL SELECT * FROM `Watches` WHERE `Os` = 'Android' UNION ALL SELECT * FROM `Tablets` WHERE `Price` < 100",
                c[EngineCodes.MySql]);

            Assert.Equal(
                "SELECT * FROM \"Phones\" WHERE \"Price\" < 300 UNION SELECT * FROM \"Laptops\" WHERE \"Price\" < 800 UNION ALL SELECT * FROM \"Tablets\" WHERE \"Price\" < 100",
                c[EngineCodes.PostgreSql]);

            Assert.Equal(
                "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 300 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" < 800 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" < 100",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void CombineRaw()
        {
            var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM Devices");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Mobiles] UNION ALL SELECT * FROM Devices", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void CombineRawWithPlaceholders()
        {
            var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM {Devices}");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM [Mobiles] UNION ALL SELECT * FROM [Devices]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM `Mobiles` UNION ALL SELECT * FROM `Devices`", c[EngineCodes.MySql]);

            Assert.Equal("SELECT * FROM \"MOBILES\" UNION ALL SELECT * FROM \"Devices\"", c[EngineCodes.Firebird]);
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
                    s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d"))
                        .Where("Name", "3778")
                )
                .For("firebird",
                    s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d"))
                        .Where("Name", "3778")
                )
                .Where("Id", ">", 55)
                .WhereBetween("Value", 18, 24);

            var c = Compile(query);

            Assert.Equal(
                "WITH [range] AS (SELECT [Number] FROM [Sequence] WHERE [Number] < 78)\nSELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Races] WHERE [Id] > 55 AND [Value] BETWEEN 18 AND 24) AS [results_wrapper] WHERE [row_num] BETWEEN 21 AND 45",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "WITH `range` AS (SELECT `Id` FROM `seqtbl` WHERE `Id` < 33)\nSELECT * FROM `Races` WHERE `RaceAuthor` IN (SELECT `Name` FROM `Users` WHERE `Status` = 'Available') AND `Id` > 55 AND `Value` BETWEEN 18 AND 24",
                c[EngineCodes.MySql]);

            Assert.Equal(
                "WITH \"range\" AS (SELECT \"d\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"Races\" WHERE \"Name\" = '3778' AND \"Id\" > 55 AND \"Value\" BETWEEN 18 AND 24",
                c[EngineCodes.PostgreSql]);

            Assert.Equal(
                "WITH \"RANGE\" AS (SELECT \"D\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"RACES\" WHERE \"NAME\" = '3778' AND \"ID\" > 55 AND \"VALUE\" BETWEEN 18 AND 24",
                c[EngineCodes.Firebird]);
        }

        // test for issue #50
        [Fact]
        public void CascadedCteAndBindings()
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

            var c = Compile(mainQuery);

            Assert.Equal("WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2)\nSELECT * FROM [cte2] WHERE [Column3] = 5", c[EngineCodes.SqlServer]);

            Assert.Equal("WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2)\nSELECT * FROM `cte2` WHERE `Column3` = 5", c[EngineCodes.MySql]);

            Assert.Equal("WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2)\nSELECT * FROM \"cte2\" WHERE \"Column3\" = 5", c[EngineCodes.PostgreSql]);

            Assert.Equal("WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2)\nSELECT * FROM \"CTE2\" WHERE \"COLUMN3\" = 5", c[EngineCodes.Firebird]);
        }

        // test for issue #50
        [Fact]
        public void CascadedAndMultiReferencedCteAndBindings()
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

            var c = Compile(mainQuery);

            Assert.Equal("WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2),\n[cte3] AS (SELECT [Column3_3], [Column3_4] FROM [Table3] \nINNER JOIN [cte1] ON ([Column1] = [Column3_3]) WHERE [Column3_4] = 33)\nSELECT * FROM [cte2] WHERE [Column3] = 5", c[EngineCodes.SqlServer]);

            Assert.Equal("WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2),\n`cte3` AS (SELECT `Column3_3`, `Column3_4` FROM `Table3` \nINNER JOIN `cte1` ON (`Column1` = `Column3_3`) WHERE `Column3_4` = 33)\nSELECT * FROM `cte2` WHERE `Column3` = 5", c[EngineCodes.MySql]);

            Assert.Equal("WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2),\n\"cte3\" AS (SELECT \"Column3_3\", \"Column3_4\" FROM \"Table3\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3_3\") WHERE \"Column3_4\" = 33)\nSELECT * FROM \"cte2\" WHERE \"Column3\" = 5", c[EngineCodes.PostgreSql]);

            Assert.Equal("WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2),\n\"CTE3\" AS (SELECT \"COLUMN3_3\", \"COLUMN3_4\" FROM \"TABLE3\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3_3\") WHERE \"COLUMN3_4\" = 33)\nSELECT * FROM \"CTE2\" WHERE \"COLUMN3\" = 5", c[EngineCodes.Firebird]);
        }

        // test for issue #50
        [Fact]
        public void MultipleCtesAndBindings()
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

            var c = Compile(mainQuery);

            Assert.Equal("WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2),\n[cte3] AS (SELECT [Column3_3], [Column3_4] FROM [Table3] \nINNER JOIN [cte1] ON ([Column1] = [Column3_3]) WHERE [Column3_4] = 33)\nSELECT * FROM [cte3] WHERE [Column3_4] = 5", c[EngineCodes.SqlServer]);

            Assert.Equal("WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2),\n`cte3` AS (SELECT `Column3_3`, `Column3_4` FROM `Table3` \nINNER JOIN `cte1` ON (`Column1` = `Column3_3`) WHERE `Column3_4` = 33)\nSELECT * FROM `cte3` WHERE `Column3_4` = 5", c[EngineCodes.MySql]);

            Assert.Equal("WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2),\n\"cte3\" AS (SELECT \"Column3_3\", \"Column3_4\" FROM \"Table3\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3_3\") WHERE \"Column3_4\" = 33)\nSELECT * FROM \"cte3\" WHERE \"Column3_4\" = 5", c[EngineCodes.PostgreSql]);

            Assert.Equal("WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2),\n\"CTE3\" AS (SELECT \"COLUMN3_3\", \"COLUMN3_4\" FROM \"TABLE3\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3_3\") WHERE \"COLUMN3_4\" = 33)\nSELECT * FROM \"CTE3\" WHERE \"COLUMN3_4\" = 5", c[EngineCodes.Firebird]);
        }


        [Fact]
        public void Limit()
        {
            var q = new Query().From("users").Select("id", "name").Limit(10);
            var c = Compile(q);

            // Assert.Equal(c[EngineCodes.SqlServer], "SELECT * FROM (SELECT [id], [name],ROW_NUMBER() OVER (SELECT 0) AS [row_num] FROM [users]) AS [temp_table] WHERE [row_num] >= 10");
            Assert.Equal("SELECT TOP (10) [id], [name] FROM [users]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `id`, `name` FROM `users` LIMIT 10", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" LIMIT 10", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT FIRST 10 \"ID\", \"NAME\" FROM \"USERS\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void Offset()
        {
            var q = new Query().From("users").Offset(10);
            var c = Compile(q);

            Assert.Equal(
                "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= 11",
                c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM `users` LIMIT 18446744073709551615 OFFSET 10", c[EngineCodes.MySql]);
            Assert.Equal("SELECT * FROM \"users\" OFFSET 10", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT SKIP 10 * FROM \"USERS\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void LimitOffset()
        {
            var q = new Query().From("users").Offset(10).Limit(5);

            var c = Compile(q);

            Assert.Equal(
                "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] BETWEEN 11 AND 15",
                c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT * FROM `users` LIMIT 5 OFFSET 10", c[EngineCodes.MySql]);
            Assert.Equal("SELECT * FROM \"users\" LIMIT 5 OFFSET 10", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT * FROM \"USERS\" ROWS 11 TO 15", c[EngineCodes.Firebird]);
        }

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

        [Fact]
        public void OrWhereRawEscaped()
        {
            var query = new Query("Table").WhereRaw("[MyCol] = ANY(?::int\\[\\])", "{1,2,3}");

            var c = Compile(query);

            Assert.Equal("SELECT * FROM \"Table\" WHERE \"MyCol\" = ANY('{1,2,3}'::int[])", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void Having()
        {
            var q = new Query("Table1")
                .Having("Column1", ">", 1);
            var c = Compile(q);

            Assert.Equal("SELECT * FROM [Table1] HAVING [Column1] > 1", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void MultipleHaving()
        {
            var q = new Query("Table1")
                .Having("Column1", ">", 1)
                .Having("Column2", "=", 1);
            var c = Compile(q);

            Assert.Equal("SELECT * FROM [Table1] HAVING [Column1] > 1 AND [Column2] = 1", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void MultipleOrHaving()
        {
            var q = new Query("Table1")
                .Having("Column1", ">", 1)
                .OrHaving("Column2", "=", 1);
            var c = Compile(q);

            Assert.Equal("SELECT * FROM [Table1] HAVING [Column1] > 1 OR [Column2] = 1", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedWhereLike()
        {
            var q = new Query("Table1")
                .WhereLike("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like 'teststring\%' ESCAPE '\'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedWhereStarts()
        {
            var q = new Query("Table1")
                .WhereStarts("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like 'teststring\%%' ESCAPE '\'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedWhereEnds()
        {
            var q = new Query("Table1")
                .WhereEnds("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%teststring\%' ESCAPE '\'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedWhereContains()
        {
            var q = new Query("Table1")
                .WhereContains("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%teststring\%%' ESCAPE '\'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedHavingLike()
        {
            var q = new Query("Table1")
                .HavingLike("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like 'teststring\%' ESCAPE '\'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedHavingStarts()
        {
            var q = new Query("Table1")
                .HavingStarts("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like 'teststring\%%' ESCAPE '\'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedHavingEnds()
        {
            var q = new Query("Table1")
                .HavingEnds("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like '%teststring\%' ESCAPE '\'", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void EscapedHavingContains()
        {
            var q = new Query("Table1")
                .HavingContains("Column1", @"TestString\%", false, @"\");
            var c = Compile(q);

            Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like '%teststring\%%' ESCAPE '\'", c[EngineCodes.SqlServer]);
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
    }
}
