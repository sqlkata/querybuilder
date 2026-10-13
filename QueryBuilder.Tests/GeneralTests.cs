using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using System;
using System.Linq;
using Xunit;

namespace SqlKata.Tests
{
    public class GeneralTests : TestSupport
    {
        [Fact]
        public void ColumnsEscaping()
        {
            var q = new Query().From("users")
                .Select("mycol[isthis]");

            var c = Compile(q);

            Assert.Equal("SELECT [mycol[isthis]]] FROM [users]", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void InnerScopeEngineWithinCTE()
        {
            var series = new Query("table")
                .ForPostgreSql(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
                .ForFirebird(q => q.WhereRaw("firebird = 1"));
            var query = new Query("series").With("series", series);

            var c = Compile(query);

            Assert.Equal("WITH [series] AS (SELECT * FROM [table] WHERE sqlsrv = 1)\nSELECT * FROM [series]", c[EngineCodes.SqlServer]);

            Assert.Equal("WITH \"series\" AS (SELECT * FROM \"table\" WHERE postgres = true)\nSELECT * FROM \"series\"",
                c[EngineCodes.PostgreSql]);
            Assert.Equal("WITH \"SERIES\" AS (SELECT * FROM \"TABLE\" WHERE firebird = 1)\nSELECT * FROM \"SERIES\"",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InnerScopeEngineWithinSubQuery()
        {
            var series = new Query("table")
                .ForPostgreSql(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
                .ForFirebird(q => q.WhereRaw("firebird = 1"));
            var query = new Query("series").From(series.As("series"));

            var c = Compile(query);

            Assert.Equal("SELECT * FROM (SELECT * FROM [table] WHERE sqlsrv = 1) AS [series]", c[EngineCodes.SqlServer]);

            Assert.Equal("SELECT * FROM (SELECT * FROM \"table\" WHERE postgres = true) AS \"series\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT * FROM (SELECT * FROM \"TABLE\" WHERE firebird = 1) AS \"SERIES\"", c[EngineCodes.Firebird]);
        }



        [Fact]
        public void Custom_compiler_with_empty_identifier_overrides_should_remove_identifiers()
        {
            var compiler = new TestEmptyIdentifiersCompiler();

            var wrappedValue = compiler.WrapValue("Table");

            Assert.Equal("Table", wrappedValue);
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
            Assert.Equal(
                "SELECT * FROM (SELECT [Id], [Name], ROW_NUMBER() OVER (ORDER BY [Name]) AS [row_num] FROM [Table]) AS [results_wrapper] WHERE [row_num] BETWEEN 2 AND 21",
                first[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `Id`, `Name` FROM `Table` ORDER BY `Name` LIMIT 20 OFFSET 1", first[EngineCodes.MySql]);
            Assert.Equal("SELECT \"Id\", \"Name\" FROM \"Table\" ORDER BY \"Name\" LIMIT 20 OFFSET 1", first[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"TABLE\" ORDER BY \"NAME\" ROWS 2 TO 21", first[EngineCodes.Firebird]);

            var second = Compile(query);

            Assert.Equal(first[EngineCodes.SqlServer], second[EngineCodes.SqlServer]);
            Assert.Equal(first[EngineCodes.MySql], second[EngineCodes.MySql]);
            Assert.Equal(first[EngineCodes.PostgreSql], second[EngineCodes.PostgreSql]);
            Assert.Equal(first[EngineCodes.Firebird], second[EngineCodes.Firebird]);
        }

        [Fact]
        public void Raw_WrapIdentifiers()
        {
            var query = new Query("Users").SelectRaw("[Id], [Name], {Age}");

            var c = Compile(query);

            Assert.Equal("SELECT [Id], [Name], [Age] FROM [Users]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `Id`, `Name`, `Age` FROM `Users`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"Id\", \"Name\", \"Age\" FROM \"Users\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"Id\", \"Name\", \"Age\" FROM \"USERS\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void Raw_WrapIdentifiers_Escaped()
        {
            var query = new Query("Users").SelectRaw("'\\{1,2,3\\}'::int\\[\\]");

            var c = Compile(query);

            Assert.Equal("SELECT '{1,2,3}'::int[] FROM \"Users\"", c[EngineCodes.PostgreSql]);
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
        public void CompilerSpecificFrom()
        {
            var query = new Query()
                .ForSqlServer(q => q.From("mssql"))
                .ForPostgreSql(q => q.From("pgsql"))
                .ForMySql(q => q.From("mysql"));
            var engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [mssql]", c[EngineCodes.SqlServer].RawSql);
            Assert.Equal("SELECT * FROM \"pgsql\"", c[EngineCodes.PostgreSql].RawSql);
            Assert.Equal("SELECT * FROM `mysql`", c[EngineCodes.MySql].RawSql);
        }

        [Fact]
        public void CompilerSpecificFromRaw()
        {
            var query = new Query()
                .ForSqlServer(q => q.FromRaw("[mssql]"))
                .ForPostgreSql(q => q.FromRaw("[pgsql]"))
                .ForMySql(q => q.FromRaw("[mysql]"));
            var engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [mssql]", c[EngineCodes.SqlServer].RawSql);
            Assert.Equal("SELECT * FROM \"pgsql\"", c[EngineCodes.PostgreSql].RawSql);
            Assert.Equal("SELECT * FROM `mysql`", c[EngineCodes.MySql].RawSql);
        }

        [Fact]
        public void CompilerSpecificFromMixed()
        {
            var query = new Query()
                .ForSqlServer(q => q.From("mssql"))
                .ForPostgreSql(q => q.FromRaw("[pgsql]"))
                .ForMySql(q => q.From("mysql"));
            var engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [mssql]", c[EngineCodes.SqlServer].RawSql);
            Assert.Equal("SELECT * FROM \"pgsql\"", c[EngineCodes.PostgreSql].RawSql);
            Assert.Equal("SELECT * FROM `mysql`", c[EngineCodes.MySql].RawSql);
        }

        [Fact]
        public void OneFromPerEngine()
        {
            var query = new Query("generic")
                .ForSqlServer(q => q.From("dnu"))
                .ForSqlServer(q => q.From("mssql"));
            var engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal(2, query.Clauses.OfType<AbstractFrom>().Count());
            Assert.Equal("SELECT * FROM [mssql]", c[EngineCodes.SqlServer].RawSql);
            Assert.Equal("SELECT * FROM \"generic\"", c[EngineCodes.PostgreSql].RawSql);
            Assert.Equal("SELECT * FROM `generic`", c[EngineCodes.MySql].RawSql);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "mssql")]
        [InlineData("original", null)]
        [InlineData("original", "mssql")]
        public void AddOrReplace_Works(string table, string engine)
        {
            var query = new Query();
            if (table != null)
                query.From(table);
            query.AddOrReplaceComponent("from", new FromClause() { Table = "updated", Engine = engine });
            var froms = query.Clauses.OfType<FromClause>();

            Assert.Single(froms);
            Assert.Equal("updated", froms.Single().Table);
        }

        [Theory]
        [InlineData(null, "generic")]
        [InlineData(EngineCodes.SqlServer, "mssql")]
        [InlineData(EngineCodes.MySql, "generic")]
        public void GetOneComponent_Prefers_Engine(string engine, string column)
        {
            var query = new Query()
                .Where("generic", "foo")
                .ForSqlServer(q => q.Where("mssql", "foo"));

            var where = query.GetOneComponent("where", engine) as BasicCondition;

            Assert.NotNull(where);
            Assert.Equal(column, where.Column);
        }

        [Fact]
        public void AddOrReplace_Throws_MoreThanOne()
        {
            var query = new Query()
                .Where("a", "b")
                .Where("c", "d");

            Action act = () => query.AddOrReplaceComponent("where", new BasicCondition());
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void Join_AddOrReplace_Replaces_Condition()
        {
            var join = new Join();
            join.AddOrReplaceComponent("where", new BasicCondition { Column = "c1", Operator = "=", Value = 1 });
            join.AddOrReplaceComponent("where", new BasicCondition { Column = "c2", Operator = "=", Value = 2 });

            Assert.Single(join.Conditions.Clauses);
            var cond = join.Conditions.Clauses.Single() as BasicCondition;
            Assert.NotNull(cond);
            Assert.Equal("c2", cond.Column);
        }

        [Fact]
        public void Join_AddOrReplace_Throws_When_MoreThanOneCondition()
        {
            var join = new Join();
            join.On("a", "b");
            join.On("c", "d");

            Action act = () => join.AddOrReplaceComponent("where", new BasicCondition());
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void Join_AddOrReplace_FromClause_Respects_EngineSelection()
        {
            var join = new Join();
            join.AddOrReplaceComponent("from", new FromClause { Table = "generic_table" });
            join.AddOrReplaceComponent("from", new FromClause { Table = "mssql_table", Engine = EngineCodes.SqlServer }, EngineCodes.SqlServer);

            // Generic FromClause should not be overwritten
            Assert.NotNull(join.FromClause);
            Assert.Equal("generic_table", (join.FromClause as FromClause)?.Table);
            Assert.Null(join.FromClause.Engine);

            // SqlServer specific FromClause should be accessible via GetComponents with EngineCode
            var mssqlFrom = join.GetComponents<FromClause>("from", EngineCodes.SqlServer);
            Assert.Contains(mssqlFrom, f => f.Table == "mssql_table");

            // Replacing the SqlServer specific FromClause replaces it instead of accumulating
            join.AddOrReplaceComponent("from", new FromClause { Table = "mssql_table_updated", Engine = EngineCodes.SqlServer }, EngineCodes.SqlServer);
            var updatedMssqlFrom = join.GetComponents<FromClause>("from", EngineCodes.SqlServer).Where(f => f.Engine == EngineCodes.SqlServer).ToList();
            Assert.Single(updatedMssqlFrom);
            Assert.Equal("mssql_table_updated", updatedMssqlFrom.Single().Table);
        }

        [Fact]
        public void OneLimitPerEngine()
        {
            var query = new Query("mytable")
                .ForSqlServer(q => q.Limit(5))
                .ForSqlServer(q => q.Limit(10));

            var limits = query.GetComponents<LimitClause>("limit", EngineCodes.SqlServer);
            Assert.Single(limits);
            Assert.Equal(10, limits.Single().Limit);
        }

        [Fact]
        public void CompilerSpecificLimit()
        {
            var query = new Query("mytable")
                .ForSqlServer(q => q.Limit(5))
                .ForPostgreSql(q => q.Limit(10));

            var engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal(2, query.GetComponents("limit").Count);
            Assert.Equal("SELECT TOP (5) * FROM [mytable]", c[EngineCodes.SqlServer].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 10", c[EngineCodes.PostgreSql].ToString());
            Assert.Equal("SELECT * FROM `mytable`", c[EngineCodes.MySql].ToString());
        }

        [Fact]
        public void OneOffsetPerEngine()
        {
            var query = new Query("mytable")
                .ForSqlServer(q => q.Offset(5))
                .ForSqlServer(q => q.Offset(10));

            var limits = query.GetComponents<OffsetClause>("offset", EngineCodes.SqlServer);
            Assert.Single(limits);
            Assert.Equal(10, limits.Single().Offset);
        }

        [Fact]
        public void CompilerSpecificOffset()
        {
            var query = new Query("mytable")
                .ForMySql(q => q.Offset(5))
                .ForPostgreSql(q => q.Offset(10));

            var engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal(2, query.GetComponents("offset").Count);
            Assert.Equal("SELECT * FROM `mytable` LIMIT 18446744073709551615 OFFSET 5", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" OFFSET 10", c[EngineCodes.PostgreSql].ToString());
            Assert.Equal("SELECT * FROM [mytable]", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void Limit_Takes_Generic_If_Needed()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Offset(20));

            var engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 10", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 5 OFFSET 20", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Offset_Takes_Generic_If_Needed()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Limit(20));

            var engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 10", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 20 OFFSET 10", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Can_Change_Generic_Limit_After_SpecificOffset()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Offset(20))
                .Limit(7);

            var engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 7 OFFSET 10", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 7 OFFSET 20", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Can_Change_Generic_Offset_After_SpecificLimit()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Limit(20))
                .Offset(7);

            var engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 7", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 20 OFFSET 7", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Where_Nested()
        {
            var query = new Query("table")
            .Where(q => q.Where("a", 1).OrWhere("a", 2));

            var engines = new[] {
                EngineCodes.SqlServer,
            };

            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [table] WHERE ([a] = 1 OR [a] = 2)", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void AdHoc_Throws_WhenNoColumnsProvided() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new string[0],
                    new object[][] {
                        new object[] {},
                        new object[] {},
                    }));

        [Fact]
        public void AdHoc_Throws_WhenNoValueRowsProvided() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new[] { "a", "b", "c" },
                    new object[][] {
                    }));

        [Fact]
        public void AdHoc_Throws_WhenColumnsOutnumberFieldValues() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new[] { "a", "b", "c", "d" },
                    new object[][] {
                        new object[] { 1, 2, 3 },
                        new object[] { 4, 5, 6 },
                    }));

        [Fact]
        public void AdHoc_Throws_WhenFieldValuesOutNumberColumns() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new[] { "a", "b" },
                    new object[][] {
                        new object[] { 1, 2, 3 },
                        new object[] { 4, 5, 6 },
                    }));

        [Fact]
        public void AdHoc_SingletonRow()
        {
            var query = new Query("rows").With("rows",
                new[] { "a" },
                new object[][] {
                    new object[] { 1 },
                });

            var c = Compilers.Compile(query);

            Assert.Equal("WITH [rows] AS (SELECT [a] FROM (VALUES (1)) AS tbl ([a]))\nSELECT * FROM [rows]", c[EngineCodes.SqlServer].ToString());
            Assert.Equal("WITH \"rows\" AS (SELECT 1 AS \"a\")\nSELECT * FROM \"rows\"", c[EngineCodes.PostgreSql].ToString());
            Assert.Equal("WITH `rows` AS (SELECT 1 AS `a`)\nSELECT * FROM `rows`", c[EngineCodes.MySql].ToString());
            Assert.Equal("WITH \"rows\" AS (SELECT 1 AS \"a\")\nSELECT * FROM \"rows\"", c[EngineCodes.Sqlite].ToString());
            Assert.Equal("WITH \"ROWS\" AS (SELECT 1 AS \"A\" FROM RDB$DATABASE)\nSELECT * FROM \"ROWS\"", c[EngineCodes.Firebird].ToString());
            Assert.Equal("WITH \"rows\" AS (SELECT 1 AS \"a\" FROM DUAL)\nSELECT * FROM \"rows\"", c[EngineCodes.Oracle].ToString());
        }

        [Fact]
        public void AdHoc_TwoRows()
        {
            var query = new Query("rows").With("rows",
                new[] { "a", "b", "c" },
                new object[][] {
                    new object[] { 1, 2, 3 },
                    new object[] { 4, 5, 6 },
                });

            var c = Compilers.Compile(query);

            Assert.Equal("WITH [rows] AS (SELECT [a], [b], [c] FROM (VALUES (1, 2, 3), (4, 5, 6)) AS tbl ([a], [b], [c]))\nSELECT * FROM [rows]", c[EngineCodes.SqlServer].ToString());
            Assert.Equal("WITH \"rows\" AS (SELECT 1 AS \"a\", 2 AS \"b\", 3 AS \"c\" UNION ALL SELECT 4 AS \"a\", 5 AS \"b\", 6 AS \"c\")\nSELECT * FROM \"rows\"", c[EngineCodes.PostgreSql].ToString());
            Assert.Equal("WITH `rows` AS (SELECT 1 AS `a`, 2 AS `b`, 3 AS `c` UNION ALL SELECT 4 AS `a`, 5 AS `b`, 6 AS `c`)\nSELECT * FROM `rows`", c[EngineCodes.MySql].ToString());
            Assert.Equal("WITH \"rows\" AS (SELECT 1 AS \"a\", 2 AS \"b\", 3 AS \"c\" UNION ALL SELECT 4 AS \"a\", 5 AS \"b\", 6 AS \"c\")\nSELECT * FROM \"rows\"", c[EngineCodes.Sqlite].ToString());
            Assert.Equal("WITH \"ROWS\" AS (SELECT 1 AS \"A\", 2 AS \"B\", 3 AS \"C\" FROM RDB$DATABASE UNION ALL SELECT 4 AS \"A\", 5 AS \"B\", 6 AS \"C\" FROM RDB$DATABASE)\nSELECT * FROM \"ROWS\"", c[EngineCodes.Firebird].ToString());
            Assert.Equal("WITH \"rows\" AS (SELECT 1 AS \"a\", 2 AS \"b\", 3 AS \"c\" FROM DUAL UNION ALL SELECT 4 AS \"a\", 5 AS \"b\", 6 AS \"c\" FROM DUAL)\nSELECT * FROM \"rows\"", c[EngineCodes.Oracle].ToString());
        }

        [Fact]
        public void AdHoc_ProperBindingsPlacement()
        {
            var query = new Query("rows")
                .With("othercte", q => q.From("othertable").Where("othertable.status", "A"))
                .Where("rows.foo", "bar")
                .With("rows",
                new[] { "a", "b", "c" },
                new object[][] {
                    new object[] { 1, 2, 3 },
                    new object[] { 4, 5, 6 },
                })
                .Where("rows.baz", "buzz");

            var c = Compilers.Compile(query);

            Assert.Equal(string.Join("\n", new[] {
                "WITH [othercte] AS (SELECT * FROM [othertable] WHERE [othertable].[status] = 'A'),",
                "[rows] AS (SELECT [a], [b], [c] FROM (VALUES (1, 2, 3), (4, 5, 6)) AS tbl ([a], [b], [c]))",
                "SELECT * FROM [rows] WHERE [rows].[foo] = 'bar' AND [rows].[baz] = 'buzz'",
            }), c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void UnsafeLiteral_Insert()
        {
            var query = new Query("Table").AsInsert(new
            {
                Count = new UnsafeLiteral("Count + 1")
            });

            var engines = new[] {
                EngineCodes.SqlServer,
            };

            var c = Compilers.Compile(engines, query);

            Assert.Equal("INSERT INTO [Table] ([Count]) VALUES (Count + 1)", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void UnsafeLiteral_Update()
        {
            var query = new Query("Table").AsUpdate(new
            {
                Count = new UnsafeLiteral("Count + 1")
            });

            var engines = new[] {
                EngineCodes.SqlServer,
            };

            var c = Compilers.Compile(engines, query);

            Assert.Equal("UPDATE [Table] SET [Count] = Count + 1", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void Passing_Boolean_To_Where_Should_Call_WhereTrue_Or_WhereFalse()
        {
            var query = new Query("Table").Where("Col", true);

            var engines = new[] {
                EngineCodes.SqlServer,
            };

            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [Table] WHERE [Col] = cast(1 as bit)", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void Passing_Boolean_False_To_Where_Should_Call_WhereTrue_Or_WhereFalse()
        {
            var query = new Query("Table").Where("Col", false);

            var engines = new[] {
                EngineCodes.SqlServer,
            };

            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [Table] WHERE [Col] = cast(0 as bit)", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void Passing_Negative_Boolean_To_Where_Should_Call_WhereTrue_Or_WhereFalse()
        {
            var query = new Query("Table").Where("Col", "!=", true);

            var engines = new[] {
                EngineCodes.SqlServer,
            };

            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [Table] WHERE [Col] != cast(1 as bit)", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void Passing_Negative_Boolean_False_To_Where_Should_Call_WhereTrue_Or_WhereFalse()
        {
            var query = new Query("Table").Where("Col", "!=", false);

            var engines = new[] {
                EngineCodes.SqlServer,
            };

            var c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [Table] WHERE [Col] != cast(0 as bit)", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void Clone_ShouldProduceIndependentIncludesList()
        {
            var query = new Query("users")
                .Include("posts", new Query("posts"), "user_id", "id");

            var clone = query.Clone();

            // Adding an include to the clone should not affect the original
            clone.Include("comments", new Query("comments"), "user_id", "id");

            Assert.Single(query.Includes);
            Assert.Equal(2, clone.Includes.Count);
        }

        [Fact]
        public void Clone_ShouldProduceIndependentIncludeObjects()
        {
            var query = new Query("users")
                .Include("posts", new Query("posts"), "user_id", "id");

            var clone = query.Clone();

            // Modifying an include property on the clone should not affect the original
            clone.Includes[0].Name = "modified_name";

            Assert.Equal("posts", query.Includes[0].Name);
            Assert.Equal("modified_name", clone.Includes[0].Name);
        }

        [Fact]
        public void Clone_ShouldProduceIndependentVariablesDictionary()
        {
            var query = new Query("users").Define("limit", 10);

            var clone = query.Clone();

            // Adding a variable to the clone should not affect the original
            clone.Define("offset", 5);

            Assert.False(query.Variables.ContainsKey("offset"));
            Assert.True(clone.Variables.ContainsKey("offset"));
        }

        [Fact]
        public void Clone_ShouldPreserveAllProperties()
        {
            var query = new Query("users")
                .Select("id", "name")
                .Where("active", true)
                .Distinct()
                .As("u")
                .Include("posts", new Query("posts"), "user_id", "id")
                .Define("myvar", 42);

            var clone = query.Clone();

            Assert.Equal(query.QueryAlias, clone.QueryAlias);
            Assert.Equal(query.IsDistinct, clone.IsDistinct);
            Assert.Equal(query.Method, clone.Method);
            Assert.Equal(query.Includes.Count, clone.Includes.Count);
            Assert.Equal(query.Includes[0].Name, clone.Includes[0].Name);
            Assert.Equal(query.Variables["myvar"], clone.Variables["myvar"]);
        }
    }
}
