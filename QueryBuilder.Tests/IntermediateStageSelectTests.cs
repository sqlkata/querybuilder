using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public sealed class IntermediateStageSelectTests : TestSupport
    {
        [Fact]
        public void BasicSelect()
        {
            CompareWithCompiler(new Query().From("users").Select("id", "name"));
        }

        [Fact]
        public void BasicSelectWhereBindingIsEmptyOrNull()
        {
            CompareWithCompiler(new Query()
                .From("users")
                .Select("id", "name")
                .Where("author", "")
                .OrWhere("author", null));
        }
        [Fact]
        public void NotNull()
        {
            // BUG: the second IsNOT is lost!
            CompareWithCompiler(new Query()
                .From("users")
                .Select("id", "name")
                .WhereNot("author", "")
                .OrWhereNot("author", null));
        }

        [Fact]
        public void BasicSelectWithAlias()
        {
            CompareWithCompiler(new Query()
                .From("users as u")
                .Select("id", "name"));
        }

        [Fact]
        public void NestedEmptyWhereAtFirstCondition()
        {
            CompareWithCompiler(new Query("table")
                .Where(_ => new Query())
                .Where("id", 1));
        }

        [Fact]
        public void WhereTrue()
        {
            CompareWithCompiler(new Query("Table").WhereTrue("IsActive"));
        }
        [Fact]
        public void WhereFalse()
        {
            CompareWithCompiler(new Query("Table").WhereFalse("IsActive"));
        }


        [Fact]
        public void WhereSub()
        {
            CompareWithCompiler(new Query("Table")
                .WhereSub(
                    new Query("Table2")
                        .WhereColumns("Table2.Column", "=", "Table.MyCol")
                        .AsCount(), 1));
        }

        [Fact]
        public void OrWhereSub()
        {
            var subQuery = new Query("Table2")
                .WhereColumns("Table2.Column", "=", "Table.MyCol")
                .Distinct()
                .AsCount();

            CompareWithCompiler(new Query("Table")
                .WhereNull("MyCol")
                .Distinct()
                .OrWhereSub(subQuery, "<", 1));

        }

        //[Fact]
        //public void PassingArrayAsParameter()
        //{
        //    var query = new Query("Table").WhereRaw("[Id] in (?)", new object[] { new object[] { 1, 2, 3 } });

        //    var c = Compile(query);

        //    Assert.Equal("SELECT * FROM [Table] WHERE [Id] in (1,2,3)", c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void UsingJsonArray()
        //{
        //    var query = new Query("Table").WhereRaw("[Json]->'address'->>'country' in (?)", new[] { 1, 2, 3, 4 });

        //    var c = Compile(query);

        //    Assert.Equal("SELECT * FROM \"Table\" WHERE \"Json\"->'address'->>'country' in (1,2,3,4)",
        //        c[EngineCodes.PostgreSql]);
        //}

        //[Fact]
        //public void Union()
        //{
        //    var laptops = new Query("Laptops");
        //    var mobiles = new Query("Phones").Union(laptops);

        //    var c = Compile(mobiles);

        //    Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops]", c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM \"Phones\" UNION SELECT * FROM \"Laptops\"", c[EngineCodes.Sqlite]);
        //    Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\"", c[EngineCodes.Firebird]);
        //}


        //[Fact]
        //public void UnionWithBindings()
        //{
        //    var laptops = new Query("Laptops").Where("Type", "A");
        //    var mobiles = new Query("Phones").Union(laptops);

        //    var c = Compile(mobiles);

        //    Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] WHERE [Type] = 'A'",
        //        c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM \"Phones\" UNION SELECT * FROM \"Laptops\" WHERE \"Type\" = 'A'",
        //        c[EngineCodes.Sqlite]);
        //    Assert.Equal("SELECT * FROM `Phones` UNION SELECT * FROM `Laptops` WHERE `Type` = 'A'",
        //        c[EngineCodes.MySql]);
        //    Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" WHERE \"TYPE\" = 'A'",
        //        c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void RawUnionWithBindings()
        //{
        //    var mobiles = new Query("Phones").UnionRaw("UNION SELECT * FROM [Laptops] WHERE [Type] = ?", "A");

        //    var c = Compile(mobiles);

        //    Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] WHERE [Type] = 'A'",
        //        c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM `Phones` UNION SELECT * FROM `Laptops` WHERE `Type` = 'A'",
        //        c[EngineCodes.MySql]);
        //}

        //[Fact]
        //public void MultipleUnion()
        //{
        //    var laptops = new Query("Laptops");
        //    var tablets = new Query("Tablets");

        //    var mobiles = new Query("Phones").Union(laptops).Union(tablets);

        //    var c = Compile(mobiles);

        //    Assert.Equal("SELECT * FROM [Phones] UNION SELECT * FROM [Laptops] UNION SELECT * FROM [Tablets]",
        //        c[EngineCodes.SqlServer]);


        //    Assert.Equal("SELECT * FROM \"PHONES\" UNION SELECT * FROM \"LAPTOPS\" UNION SELECT * FROM \"TABLETS\"",
        //        c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void MultipleUnionWithBindings()
        //{
        //    var laptops = new Query("Laptops").Where("Price", ">", 1000);
        //    var tablets = new Query("Tablets").Where("Price", ">", 2000);

        //    var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).Union(tablets);

        //    var c = Compile(mobiles);

        //    Assert.Equal(
        //        "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] WHERE [Price] > 1000 UNION SELECT * FROM [Tablets] WHERE [Price] > 2000",
        //        c[EngineCodes.SqlServer]);


        //    Assert.Equal(
        //        "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000",
        //        c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void MultipleUnionWithBindingsAndPagination()
        //{
        //    var laptops = new Query("Laptops").Where("Price", ">", 1000);
        //    var tablets = new Query("Tablets").Where("Price", ">", 2000).ForPage(2);

        //    var mobiles = new Query("Phones").Where("Price", "<", 3000).Union(laptops).UnionAll(tablets);


        //    var c = Compile(mobiles);

        //    Assert.Equal(
        //        "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] WHERE [Price] > 1000 UNION ALL SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Tablets] WHERE [Price] > 2000) AS [results_wrapper] WHERE [row_num] BETWEEN 16 AND 30",
        //        c[EngineCodes.SqlServer]);


        //    Assert.Equal(
        //        "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" > 1000 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" > 2000 ROWS 16 TO 30",
        //        c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void UnionWithCallbacks()
        //{
        //    var mobiles = new Query("Phones")
        //        .Where("Price", "<", 3000)
        //        .Union(q => q.From("Laptops"))
        //        .UnionAll(q => q.From("Tablets"));

        //    var c = Compile(mobiles);

        //    Assert.Equal(
        //        "SELECT * FROM [Phones] WHERE [Price] < 3000 UNION SELECT * FROM [Laptops] UNION ALL SELECT * FROM [Tablets]",
        //        c[EngineCodes.SqlServer]);


        //    Assert.Equal(
        //        "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 3000 UNION SELECT * FROM \"LAPTOPS\" UNION ALL SELECT * FROM \"TABLETS\"",
        //        c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void UnionWithDifferentEngine()
        //{
        //    var mobiles = new Query("Phones")
        //        .Where("Price", "<", 300)
        //        .ForSqlServer(scope => scope.Except(q => q.From("Phones").WhereNot("Os", "iOS")))
        //        .ForPostgreSql(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
        //        .ForMySql(scope => scope.IntersectAll(q => q.From("Watches").Where("Os", "Android")))
        //        .ForFirebird(scope => scope.Union(q => q.From("Laptops").Where("Price", "<", 800)))
        //        .UnionAll(q => q.From("Tablets").Where("Price", "<", 100));

        //    var c = Compile(mobiles);

        //    Assert.Equal(
        //        "SELECT * FROM [Phones] WHERE [Price] < 300 EXCEPT SELECT * FROM [Phones] WHERE NOT ([Os] = 'iOS') UNION ALL SELECT * FROM [Tablets] WHERE [Price] < 100",
        //        c[EngineCodes.SqlServer]);

        //    Assert.Equal(
        //        "SELECT * FROM `Phones` WHERE `Price` < 300 INTERSECT ALL SELECT * FROM `Watches` WHERE `Os` = 'Android' UNION ALL SELECT * FROM `Tablets` WHERE `Price` < 100",
        //        c[EngineCodes.MySql]);

        //    Assert.Equal(
        //        "SELECT * FROM \"Phones\" WHERE \"Price\" < 300 UNION SELECT * FROM \"Laptops\" WHERE \"Price\" < 800 UNION ALL SELECT * FROM \"Tablets\" WHERE \"Price\" < 100",
        //        c[EngineCodes.PostgreSql]);

        //    Assert.Equal(
        //        "SELECT * FROM \"PHONES\" WHERE \"PRICE\" < 300 UNION SELECT * FROM \"LAPTOPS\" WHERE \"PRICE\" < 800 UNION ALL SELECT * FROM \"TABLETS\" WHERE \"PRICE\" < 100",
        //        c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void CombineRaw()
        //{
        //    var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM Devices");

        //    var c = Compile(query);

        //    Assert.Equal("SELECT * FROM [Mobiles] UNION ALL SELECT * FROM Devices", c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void CombineRawWithPlaceholders()
        //{
        //    var query = new Query("Mobiles").CombineRaw("UNION ALL SELECT * FROM {Devices}");

        //    var c = Compile(query);

        //    Assert.Equal("SELECT * FROM [Mobiles] UNION ALL SELECT * FROM [Devices]", c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM `Mobiles` UNION ALL SELECT * FROM `Devices`", c[EngineCodes.MySql]);

        //    Assert.Equal("SELECT * FROM \"MOBILES\" UNION ALL SELECT * FROM \"Devices\"", c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void NestedEmptyWhere()
        //{
        //    // Empty nested where should be ignored
        //    var query = new Query("A").Where(_ => new Query().Where(_ => new Query().Where(_ => new Query())));

        //    var c = Compile(query);

        //    Assert.Equal("SELECT * FROM [A]", c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void NestedQuery()
        //{
        //    var query = new Query("A").Where(_ => new Query("B"));

        //    var c = Compile(query);

        //    Assert.Equal("SELECT * FROM [A]", c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void NestedQueryAfterNestedJoin()
        //{
        //    // in this test, i am testing the compiler dynamic caching functionality
        //    var query = new Query("users")
        //        .Join("countries", j => j.On("countries.id", "users.country_id"))
        //        .Where(_ => new Query());

        //    var c = Compile(query)[EngineCodes.SqlServer];

        //    Assert.Equal(
        //        """
        //        SELECT * FROM [users]
        //        INNER JOIN [countries] ON ([countries].[id] = [users].[country_id])
        //        """, c.Replace("\n", "\r\n"));
        //}

        //[Fact]
        //public void MultipleCte()
        //{
        //    var q1 = new Query("A");
        //    var q2 = new Query("B");
        //    var q3 = new Query("C");

        //    var query = new Query("A")
        //        .With("A", q1)
        //        .With("B", q2)
        //        .With("C", q3);

        //    var c = Compile(query);

        //    Assert.Equal(
        //        "WITH [A] AS (SELECT * FROM [A]),\n[B] AS (SELECT * FROM [B]),\n[C] AS (SELECT * FROM [C])\nSELECT * FROM [A]",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void CteAndBindings()
        //{
        //    var query = new Query("Races")
        //        .For("mysql", s =>
        //            s.With("range", q =>
        //                    q.From("seqtbl")
        //                        .Select("Id").Where("Id", "<", 33))
        //                .WhereIn("RaceAuthor", q => q.From("Users")
        //                    .Select("Name").Where("Status", "Available")
        //                )
        //        )
        //        .For("sqlsrv", s =>
        //            s.With("range",
        //                    q => q.From("Sequence").Select("Number").Where("Number", "<", 78)
        //                )
        //                .Limit(25).Offset(20)
        //        )
        //        .For("postgres",
        //            s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d"))
        //                .Where("Name", "3778")
        //        )
        //        .For("firebird",
        //            s => s.With("range", q => q.FromRaw("generate_series(1, 33) as d").Select("d"))
        //                .Where("Name", "3778")
        //        )
        //        .Where("Id", ">", 55)
        //        .WhereBetween("Value", 18, 24);

        //    var c = Compile(query);

        //    Assert.Equal(
        //        "WITH [range] AS (SELECT [Number] FROM [Sequence] WHERE [Number] < 78)\nSELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [Races] WHERE [Id] > 55 AND [Value] BETWEEN 18 AND 24) AS [results_wrapper] WHERE [row_num] BETWEEN 21 AND 45",
        //        c[EngineCodes.SqlServer]);

        //    Assert.Equal(
        //        "WITH `range` AS (SELECT `Id` FROM `seqtbl` WHERE `Id` < 33)\nSELECT * FROM `Races` WHERE `RaceAuthor` IN (SELECT `Name` FROM `Users` WHERE `Status` = 'Available') AND `Id` > 55 AND `Value` BETWEEN 18 AND 24",
        //        c[EngineCodes.MySql]);

        //    Assert.Equal(
        //        "WITH \"range\" AS (SELECT \"d\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"Races\" WHERE \"Name\" = '3778' AND \"Id\" > 55 AND \"Value\" BETWEEN 18 AND 24",
        //        c[EngineCodes.PostgreSql]);

        //    Assert.Equal(
        //        "WITH \"RANGE\" AS (SELECT \"D\" FROM generate_series(1, 33) as d)\nSELECT * FROM \"RACES\" WHERE \"NAME\" = '3778' AND \"ID\" > 55 AND \"VALUE\" BETWEEN 18 AND 24",
        //        c[EngineCodes.Firebird]);
        //}

        //// test for issue #50
        //[Fact]
        //public void CascadedCteAndBindings()
        //{
        //    var cte1 = new Query("Table1");
        //    cte1.Select("Column1", "Column2");
        //    cte1.Where("Column2", 1);

        //    var cte2 = new Query("Table2");
        //    cte2.With("cte1", cte1);
        //    cte2.Select("Column3", "Column4");
        //    cte2.Join("cte1", join => join.On("Column1", "Column3"));
        //    cte2.Where("Column4", 2);

        //    var mainQuery = new Query("Table3");
        //    mainQuery.With("cte2", cte2);
        //    mainQuery.Select("*");
        //    mainQuery.From("cte2");
        //    mainQuery.Where("Column3", 5);

        //    var c = Compile(mainQuery);

        //    Assert.Equal(
        //        "WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2)\nSELECT * FROM [cte2] WHERE [Column3] = 5",
        //        c[EngineCodes.SqlServer]);

        //    Assert.Equal(
        //        "WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2)\nSELECT * FROM `cte2` WHERE `Column3` = 5",
        //        c[EngineCodes.MySql]);

        //    Assert.Equal(
        //        "WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2)\nSELECT * FROM \"cte2\" WHERE \"Column3\" = 5",
        //        c[EngineCodes.PostgreSql]);

        //    Assert.Equal(
        //        "WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2)\nSELECT * FROM \"CTE2\" WHERE \"COLUMN3\" = 5",
        //        c[EngineCodes.Firebird]);
        //}

        //// test for issue #50
        //[Fact]
        //public void CascadedAndMultiReferencedCteAndBindings()
        //{
        //    var cte1 = new Query("Table1");
        //    cte1.Select("Column1", "Column2");
        //    cte1.Where("Column2", 1);

        //    var cte2 = new Query("Table2");
        //    cte2.With("cte1", cte1);
        //    cte2.Select("Column3", "Column4");
        //    cte2.Join("cte1", join => join.On("Column1", "Column3"));
        //    cte2.Where("Column4", 2);

        //    var cte3 = new Query("Table3");
        //    cte3.With("cte1", cte1);
        //    cte3.Select("Column3_3", "Column3_4");
        //    cte3.Join("cte1", join => join.On("Column1", "Column3_3"));
        //    cte3.Where("Column3_4", 33);

        //    var mainQuery = new Query("Table3");
        //    mainQuery.With("cte2", cte2);
        //    mainQuery.With("cte3", cte3);
        //    mainQuery.Select("*");
        //    mainQuery.From("cte2");
        //    mainQuery.Where("Column3", 5);

        //    var c = Compile(mainQuery);

        //    Assert.Equal(
        //        """
        //        WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),
        //        [cte2] AS (SELECT [Column3], [Column4] FROM [Table2]
        //        INNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2),
        //        [cte3] AS (SELECT [Column3_3], [Column3_4] FROM [Table3]
        //        INNER JOIN [cte1] ON ([Column1] = [Column3_3]) WHERE [Column3_4] = 33)
        //        SELECT * FROM [cte2] WHERE [Column3] = 5
        //        """,
        //        c[EngineCodes.SqlServer].Replace("\n", "\r\n"));

        //    Assert.Equal(
        //        """
        //        WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),
        //        `cte2` AS (SELECT `Column3`, `Column4` FROM `Table2`
        //        INNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2),
        //        `cte3` AS (SELECT `Column3_3`, `Column3_4` FROM `Table3`
        //        INNER JOIN `cte1` ON (`Column1` = `Column3_3`) WHERE `Column3_4` = 33)
        //        SELECT * FROM `cte2` WHERE `Column3` = 5
        //        """,
        //        c[EngineCodes.MySql].Replace("\n", "\r\n"));

        //    Assert.Equal(
        //        """
        //        WITH "cte1" AS (SELECT "Column1", "Column2" FROM "Table1" WHERE "Column2" = 1),
        //        "cte2" AS (SELECT "Column3", "Column4" FROM "Table2"
        //        INNER JOIN "cte1" ON ("Column1" = "Column3") WHERE "Column4" = 2),
        //        "cte3" AS (SELECT "Column3_3", "Column3_4" FROM "Table3"
        //        INNER JOIN "cte1" ON ("Column1" = "Column3_3") WHERE "Column3_4" = 33)
        //        SELECT * FROM "cte2" WHERE "Column3" = 5
        //        """,
        //        c[EngineCodes.PostgreSql].Replace("\n", "\r\n"));

        //    Assert.Equal(
        //        """
        //        WITH "CTE1" AS (SELECT "COLUMN1", "COLUMN2" FROM "TABLE1" WHERE "COLUMN2" = 1),
        //        "CTE2" AS (SELECT "COLUMN3", "COLUMN4" FROM "TABLE2"
        //        INNER JOIN "CTE1" ON ("COLUMN1" = "COLUMN3") WHERE "COLUMN4" = 2),
        //        "CTE3" AS (SELECT "COLUMN3_3", "COLUMN3_4" FROM "TABLE3"
        //        INNER JOIN "CTE1" ON ("COLUMN1" = "COLUMN3_3") WHERE "COLUMN3_4" = 33)
        //        SELECT * FROM "CTE2" WHERE "COLUMN3" = 5
        //        """,
        //        c[EngineCodes.Firebird].Replace("\n", "\r\n"));
        //}

        //// test for issue #50
        //[Fact]
        //public void MultipleCtesAndBindings()
        //{
        //    var cte1 = new Query("Table1");
        //    cte1.Select("Column1", "Column2");
        //    cte1.Where("Column2", 1);

        //    var cte2 = new Query("Table2");
        //    cte2.Select("Column3", "Column4");
        //    cte2.Join("cte1", join => join.On("Column1", "Column3"));
        //    cte2.Where("Column4", 2);

        //    var cte3 = new Query("Table3");
        //    cte3.Select("Column3_3", "Column3_4");
        //    cte3.Join("cte1", join => join.On("Column1", "Column3_3"));
        //    cte3.Where("Column3_4", 33);

        //    var mainQuery = new Query("Table3");
        //    mainQuery.With("cte1", cte1);
        //    mainQuery.With("cte2", cte2);
        //    mainQuery.With("cte3", cte3);
        //    mainQuery.Select("*");
        //    mainQuery.From("cte3");
        //    mainQuery.Where("Column3_4", 5);

        //    var c = Compile(mainQuery);

        //    Assert.Equal(
        //        "WITH [cte1] AS (SELECT [Column1], [Column2] FROM [Table1] WHERE [Column2] = 1),\n[cte2] AS (SELECT [Column3], [Column4] FROM [Table2] \nINNER JOIN [cte1] ON ([Column1] = [Column3]) WHERE [Column4] = 2),\n[cte3] AS (SELECT [Column3_3], [Column3_4] FROM [Table3] \nINNER JOIN [cte1] ON ([Column1] = [Column3_3]) WHERE [Column3_4] = 33)\nSELECT * FROM [cte3] WHERE [Column3_4] = 5",
        //        c[EngineCodes.SqlServer]);

        //    Assert.Equal(
        //        "WITH `cte1` AS (SELECT `Column1`, `Column2` FROM `Table1` WHERE `Column2` = 1),\n`cte2` AS (SELECT `Column3`, `Column4` FROM `Table2` \nINNER JOIN `cte1` ON (`Column1` = `Column3`) WHERE `Column4` = 2),\n`cte3` AS (SELECT `Column3_3`, `Column3_4` FROM `Table3` \nINNER JOIN `cte1` ON (`Column1` = `Column3_3`) WHERE `Column3_4` = 33)\nSELECT * FROM `cte3` WHERE `Column3_4` = 5",
        //        c[EngineCodes.MySql]);

        //    Assert.Equal(
        //        "WITH \"cte1\" AS (SELECT \"Column1\", \"Column2\" FROM \"Table1\" WHERE \"Column2\" = 1),\n\"cte2\" AS (SELECT \"Column3\", \"Column4\" FROM \"Table2\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3\") WHERE \"Column4\" = 2),\n\"cte3\" AS (SELECT \"Column3_3\", \"Column3_4\" FROM \"Table3\" \nINNER JOIN \"cte1\" ON (\"Column1\" = \"Column3_3\") WHERE \"Column3_4\" = 33)\nSELECT * FROM \"cte3\" WHERE \"Column3_4\" = 5",
        //        c[EngineCodes.PostgreSql]);

        //    Assert.Equal(
        //        "WITH \"CTE1\" AS (SELECT \"COLUMN1\", \"COLUMN2\" FROM \"TABLE1\" WHERE \"COLUMN2\" = 1),\n\"CTE2\" AS (SELECT \"COLUMN3\", \"COLUMN4\" FROM \"TABLE2\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3\") WHERE \"COLUMN4\" = 2),\n\"CTE3\" AS (SELECT \"COLUMN3_3\", \"COLUMN3_4\" FROM \"TABLE3\" \nINNER JOIN \"CTE1\" ON (\"COLUMN1\" = \"COLUMN3_3\") WHERE \"COLUMN3_4\" = 33)\nSELECT * FROM \"CTE3\" WHERE \"COLUMN3_4\" = 5",
        //        c[EngineCodes.Firebird]);
        //}


        //[Fact]
        //public void Limit()
        //{
        //    var q = new Query().From("users").Select("id", "name").Limit(10);
        //    var c = Compile(q);

        //    // Assert.Equal(c[EngineCodes.SqlServer], "SELECT * FROM (SELECT [id], [name],ROW_NUMBER() OVER (SELECT 0) AS [row_num] FROM [users]) AS [temp_table] WHERE [row_num] >= 10");
        //    Assert.Equal("SELECT TOP (10) [id], [name] FROM [users]", c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT `id`, `name` FROM `users` LIMIT 10", c[EngineCodes.MySql]);
        //    Assert.Equal("SELECT \"id\", \"name\" FROM \"users\" LIMIT 10", c[EngineCodes.PostgreSql]);
        //    Assert.Equal("SELECT FIRST 10 \"ID\", \"NAME\" FROM \"USERS\"", c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void Offset()
        //{
        //    var q = new Query().From("users").Offset(10);
        //    var c = Compile(q);

        //    Assert.Equal(
        //        "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= 11",
        //        c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM `users` LIMIT 18446744073709551615 OFFSET 10", c[EngineCodes.MySql]);
        //    Assert.Equal("SELECT * FROM \"users\" OFFSET 10", c[EngineCodes.PostgreSql]);
        //    Assert.Equal("SELECT SKIP 10 * FROM \"USERS\"", c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void LimitOffset()
        //{
        //    var q = new Query().From("users").Offset(10).Limit(5);

        //    var c = Compile(q);

        //    Assert.Equal(
        //        "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] BETWEEN 11 AND 15",
        //        c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM `users` LIMIT 5 OFFSET 10", c[EngineCodes.MySql]);
        //    Assert.Equal("SELECT * FROM \"users\" LIMIT 5 OFFSET 10", c[EngineCodes.PostgreSql]);
        //    Assert.Equal("SELECT * FROM \"USERS\" ROWS 11 TO 15", c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void BasicJoin()
        //{
        //    var q = new Query().From("users").Join("countries", "countries.id", "users.country_id");

        //    var c = Compile(q);

        //    Assert.Equal("SELECT * FROM [users] \nINNER JOIN [countries] ON [countries].[id] = [users].[country_id]",
        //        c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM `users` \nINNER JOIN `countries` ON `countries`.`id` = `users`.`country_id`",
        //        c[EngineCodes.MySql]);
        //}

        //[Theory]
        //[InlineData("inner join", "INNER JOIN")]
        //[InlineData("left join", "LEFT JOIN")]
        //[InlineData("right join", "RIGHT JOIN")]
        //[InlineData("cross join", "CROSS JOIN")]
        //public void JoinTypes(string given, string output)
        //{
        //    var q = new Query().From("users")
        //        .Join("countries", "countries.id", "users.country_id", "=", given);

        //    var c = Compile(q);

        //    Assert.Equal($"SELECT * FROM [users] \n{output} [countries] ON [countries].[id] = [users].[country_id]",
        //        c[EngineCodes.SqlServer]);

        //    Assert.Equal($"SELECT * FROM `users` \n{output} `countries` ON `countries`.`id` = `users`.`country_id`",
        //        c[EngineCodes.MySql]);

        //    Assert.Equal(
        //        $"SELECT * FROM \"users\" \n{output} \"countries\" ON \"countries\".\"id\" = \"users\".\"country_id\"",
        //        c[EngineCodes.PostgreSql]);

        //    Assert.Equal(
        //        $"SELECT * FROM \"USERS\" \n{output} \"COUNTRIES\" ON \"COUNTRIES\".\"ID\" = \"USERS\".\"COUNTRY_ID\"",
        //        c[EngineCodes.Firebird]);
        //}

        //[Fact]
        //public void OrWhereRawEscaped()
        //{
        //    var query = new Query("Table").WhereRaw("[MyCol] = ANY(?::int\\[\\])", "{1,2,3}");

        //    var c = Compile(query);

        //    Assert.Equal("SELECT * FROM \"Table\" WHERE \"MyCol\" = ANY('{1,2,3}'::int[])", c[EngineCodes.PostgreSql]);
        //}

        //[Fact]
        //public void Having()
        //{
        //    var q = new Query("Table1")
        //        .Having("Column1", ">", 1);
        //    var c = Compile(q);

        //    Assert.Equal("SELECT * FROM [Table1] HAVING [Column1] > 1", c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void MultipleHaving()
        //{
        //    var q = new Query("Table1")
        //        .Having("Column1", ">", 1)
        //        .Having("Column2", "=", 1);
        //    var c = Compile(q);

        //    Assert.Equal("SELECT * FROM [Table1] HAVING [Column1] > 1 AND [Column2] = 1", c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void MultipleOrHaving()
        //{
        //    var q = new Query("Table1")
        //        .Having("Column1", ">", 1)
        //        .OrHaving("Column2", "=", 1);
        //    var c = Compile(q);

        //    Assert.Equal("SELECT * FROM [Table1] HAVING [Column1] > 1 OR [Column2] = 1", c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void ShouldUseILikeOnPostgresWhenNonCaseSensitive()
        //{
        //    var q = new Query("Table1")
        //        .WhereLike("Column1", "%Upper Word%");
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%upper word%'",
        //        c[EngineCodes.SqlServer]);
        //    Assert.Equal("SELECT * FROM \"Table1\" WHERE \"Column1\" ilike '%Upper Word%'", c[EngineCodes.PostgreSql]);
        //}

        //[Fact]
        //public void EscapedWhereLike()
        //{
        //    var q = new Query("Table1")
        //        .WhereLike("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like 'teststring\%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void EscapedWhereStarts()
        //{
        //    var q = new Query("Table1")
        //        .WhereStarts("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like 'teststring\%%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void EscapedWhereEnds()
        //{
        //    var q = new Query("Table1")
        //        .WhereEnds("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%teststring\%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void EscapedWhereContains()
        //{
        //    var q = new Query("Table1")
        //        .WhereContains("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] WHERE LOWER([Column1]) like '%teststring\%%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void EscapedHavingLike()
        //{
        //    var q = new Query("Table1")
        //        .HavingLike("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like 'teststring\%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void EscapedHavingStarts()
        //{
        //    var q = new Query("Table1")
        //        .HavingStarts("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like 'teststring\%%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void EscapedHavingEnds()
        //{
        //    var q = new Query("Table1")
        //        .HavingEnds("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like '%teststring\%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void EscapedHavingContains()
        //{
        //    var q = new Query("Table1")
        //        .HavingContains("Column1", @"TestString\%", false, '\\');
        //    var c = Compile(q);

        //    Assert.Equal(@"SELECT * FROM [Table1] HAVING LOWER([Column1]) like '%teststring\%%' ESCAPE '\'",
        //        c[EngineCodes.SqlServer]);
        //}

        //[Fact]
        //public void BasicSelectRaw_WithNoTable()
        //{
        //    var q = new Query().SelectRaw("somefunction() as c1");

        //    var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT somefunction() as c1", c.ToString());
        //}

        //[Fact]
        //public void BasicSelect_WithNoTable()
        //{
        //    var q = new Query().Select("c1");
        //    var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT [c1]", c.ToString());
        //}

        //[Fact]
        //public void BasicSelect_WithNoTableAndWhereClause()
        //{
        //    var q = new Query().Select("c1").Where("p", 1);
        //    var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT [c1] WHERE [p] = 1", c.ToString());
        //}

        //[Fact]
        //public void BasicSelect_WithNoTableWhereRawClause()
        //{
        //    var q = new Query().Select("c1").WhereRaw("1 = 1");
        //    var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT [c1] WHERE 1 = 1", c.ToString());
        //}

        //[Fact]
        //public void BasicSelectAggregate()
        //{
        //    var q = new Query("Posts").Select("Title")
        //        .SelectAggregate("sum", "ViewCount");

        //    var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT [Title], SUM([ViewCount]) FROM [Posts]", sqlServer.ToString());
        //}

        //[Fact]
        //public void SelectAggregateShouldIgnoreEmptyFilter()
        //{
        //    var q = new Query("Posts").Select("Title")
        //        .SelectAggregate("sum", "ViewCount", q => q);

        //    var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT [Title], SUM([ViewCount]) FROM [Posts]", sqlServer.ToString());
        //}

        //[Fact]
        //public void SelectAggregateShouldIgnoreEmptyQueryFilter()
        //{
        //    var q = new Query("Posts").Select("Title")
        //        .SelectAggregate("sum", "ViewCount", new Query());

        //    var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT [Title], SUM([ViewCount]) FROM [Posts]", sqlServer.ToString());
        //}

        //[Fact]
        //public void BasicSelectAggregateWithAlias()
        //{
        //    var q = new Query("Posts").Select("Title")
        //        .SelectAggregate("sum", "ViewCount as TotalViews");

        //    var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal("SELECT [Title], SUM([ViewCount]) AS [TotalViews] FROM [Posts]", sqlServer.ToString());
        //}

        //[Fact]
        //public void SelectWithFilter()
        //{
        //    var q = new Query("Posts").Select("Title")
        //        .SelectAggregate("sum", "ViewCount as Published_Jan", q => q.Where("Published_Month", "Jan"))
        //        .SelectAggregate("sum", "ViewCount as Published_Feb", q => q.Where("Published_Month", "Feb"));

        //    var pgSql = Compilers.CompileFor(EngineCodes.PostgreSql, q);
        //    Assert.Equal(
        //        "SELECT \"Title\", SUM(\"ViewCount\") FILTER (WHERE \"Published_Month\" = 'Jan') AS \"Published_Jan\", SUM(\"ViewCount\") FILTER (WHERE \"Published_Month\" = 'Feb') AS \"Published_Feb\" FROM \"Posts\"",
        //        pgSql.ToString());

        //    var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal(
        //        "SELECT [Title], SUM(CASE WHEN [Published_Month] = 'Jan' THEN [ViewCount] END) AS [Published_Jan], SUM(CASE WHEN [Published_Month] = 'Feb' THEN [ViewCount] END) AS [Published_Feb] FROM [Posts]",
        //        sqlServer.ToString());
        //}

        //[Fact]
        //public void SelectWithExists()
        //{
        //    var q = new Query("Posts").WhereExists(
        //        new Query("Comments").WhereColumns("Comments.PostId", "=", "Posts.Id")
        //    );

        //    var sqlServer = Compilers.CompileFor(EngineCodes.SqlServer, q);
        //    Assert.Equal(
        //        "SELECT * FROM [Posts] WHERE EXISTS (SELECT 1 FROM [Comments] WHERE [Comments].[PostId] = [Posts].[Id])",
        //        sqlServer.ToString());
        //}

        //[Fact]
        //public void SelectWithExists_OmitSelectIsFalse()
        //{
        //    var q = new Query("Posts").WhereExists(
        //        new Query("Comments").Select("Id").WhereColumns("Comments.PostId", "=", "Posts.Id")
        //    );


        //    var compiler = new SqlServerCompiler
        //    {
        //        OmitSelectInsideExists = false
        //    };

        //    var sqlServer = compiler.Compile(q).ToString();
        //    Assert.Equal(
        //        "SELECT * FROM [Posts] WHERE EXISTS (SELECT [Id] FROM [Comments] WHERE [Comments].[PostId] = [Posts].[Id])",
        //        sqlServer);
        //}
    }
}
