using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using System;
using System.Linq;
using SqlKata.Tests.Infrastructure.TestCompilers;
using Xunit;

namespace SqlKata.Tests
{
    public class GeneralTests : TestSupport
    {
        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [mycol[isthis]]] FROM [users]")]
        public void ColumnsEscaping(string engine, string sqlText)
        {
            var query = new Query().From("users")
                .Select("mycol[isthis]");

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "WITH [series] AS (SELECT * FROM [table] WHERE sqlsrv = 1)\nSELECT * FROM [series]")]
        [InlineData(
            EngineCodes.PostgreSql,
            "WITH \"series\" AS (SELECT * FROM \"table\" WHERE postgres = true)\nSELECT * FROM \"series\"")]
        [InlineData(
            EngineCodes.Firebird,
            "WITH \"SERIES\" AS (SELECT * FROM \"TABLE\" WHERE firebird = 1)\nSELECT * FROM \"SERIES\"")]
        public void InnerScopeEngineWithinCTE(string engine, string sqlText)
        {
            var series = new Query("table")
                .ForPostgreSql(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
                .ForFirebird(q => q.WhereRaw("firebird = 1"));
            var query = new Query("series").With("series", series);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT * FROM (SELECT * FROM [table] WHERE sqlsrv = 1) AS [series]")]
        [InlineData(
            EngineCodes.PostgreSql,
            "SELECT * FROM (SELECT * FROM \"table\" WHERE postgres = true) AS \"series\"")]
        [InlineData(
            EngineCodes.Firebird,
            "SELECT * FROM (SELECT * FROM \"TABLE\" WHERE firebird = 1) AS \"SERIES\"")]
        public void InnerScopeEngineWithinSubQuery(string engine, string sqlText)
        {
            var series = new Query("table")
                .ForPostgreSql(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
                .ForFirebird(q => q.WhereRaw("firebird = 1"));
            var query = new Query("series").From(series.As("series"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
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
        public void Custom_compiler_with_empty_identifier_overrides_should_remove_identifiers()
        {
            var compiler = new TestEmptyIdentifiersCompiler();

            var wrappedValue = compiler.WrapValue("Table");

            Assert.Equal("Table", wrappedValue);
        }

        [Theory]
        [InlineData(EngineCodes.Firebird)]
        [InlineData(EngineCodes.MySql)]
        [InlineData(EngineCodes.Oracle)]
        [InlineData(EngineCodes.PostgreSql)]
        [InlineData(EngineCodes.Sqlite)]
        [InlineData(EngineCodes.SqlServer)]
        public void Should_Equal_AfterMultipleCompile(string engine)
        {
            var query = new Query()
                .Select("Id", "Name")
                .From("Table")
                .OrderBy("Name")
                .Limit(20)
                .Offset(1);

            var first = CompileFor(engine, query);
            var second = CompileFor(engine, query);

            Assert.Equal(first.ToString(), second.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT [Id], [Name], [Age] FROM [Users]")]
        [InlineData(EngineCodes.MySql, "SELECT `Id`, `Name`, `Age` FROM `Users`")]
        [InlineData(EngineCodes.PostgreSql, "SELECT \"Id\", \"Name\", \"Age\" FROM \"Users\"")]
        [InlineData(EngineCodes.Firebird, "SELECT \"Id\", \"Name\", \"Age\" FROM \"USERS\"")]
        public void Raw_WrapIdentifiers(string engine, string sqlText)
        {
            var query = new Query("Users").SelectRaw("[Id], [Name], {Age}");

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.PostgreSql, "SELECT '{1,2,3}'::int[] FROM \"Users\"")]
        public void Raw_WrapIdentifiers_Escaped(string engine, string sqlText)
        {
            var query = new Query("Users").SelectRaw("'\\{1,2,3\\}'::int\\[\\]");

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
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

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [mssql]")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"pgsql\"")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `mysql`")]
        public void CompilerSpecificFrom(string engine, string sqlText)
        {
            var query = new Query()
                .ForSqlServer(q => q.From("mssql"))
                .ForPostgreSql(q => q.From("pgsql"))
                .ForMySql(q => q.From("mysql"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [mssql]")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"pgsql\"")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `mysql`")]
        public void CompilerSpecificFromRaw(string engine, string sqlText)
        {
            var query = new Query()
                .ForSqlServer(q => q.FromRaw("[mssql]"))
                .ForPostgreSql(q => q.FromRaw("[pgsql]"))
                .ForMySql(q => q.FromRaw("[mysql]"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [mssql]")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"pgsql\"")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `mysql`")]
        public void CompilerSpecificFromMixed(string engine, string sqlText)
        {
            var query = new Query()
                .ForSqlServer(q => q.From("mssql"))
                .ForPostgreSql(q => q.FromRaw("[pgsql]"))
                .ForMySql(q => q.From("mysql"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Fact]
        public void OneFromPerEngine()
        {
            var query = new Query("generic")
                .ForSqlServer(q => q.From("dnu"))
                .ForSqlServer(q => q.From("mssql"));

            var c = CompileFor(EngineCodes.SqlServer, query);
            var c2 = CompileFor(EngineCodes.PostgreSql, query);
            var c3 = CompileFor(EngineCodes.MySql, query);

            Assert.Equal(2, query.Clauses.OfType<AbstractFrom>().Count());
            Assert.Equal("SELECT * FROM [mssql]", c.RawSql);
            Assert.Equal("SELECT * FROM \"generic\"", c2.RawSql);
            Assert.Equal("SELECT * FROM `generic`", c3.RawSql);
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
            query.AddOrReplaceComponent("from", new FromClause { Table = "updated", Engine = engine });
            var froms = query.Clauses.OfType<FromClause>().ToList();

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
        public void OneLimitPerEngine()
        {
            var query = new Query("mytable")
                .ForSqlServer(q => q.Limit(5))
                .ForSqlServer(q => q.Limit(10));

            var limits = query.GetComponents<LimitClause>("limit", EngineCodes.SqlServer);

            Assert.Single(limits);
            Assert.Equal(10, limits.Single().Limit);
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT TOP (5) * FROM [mytable]")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"mytable\" LIMIT 10")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `mytable`")]
        public void CompilerSpecificLimit(string engine, string sqlText)
        {
            var query = new Query("mytable")
                .ForSqlServer(q => q.Limit(5))
                .ForPostgreSql(q => q.Limit(10));

            var result = CompileFor(engine, query);

            Assert.Equal(2, query.GetComponents("limit").Count);
            Assert.Equal(sqlText, result.ToString());
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

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [mytable]")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"mytable\" OFFSET 10")]
        [InlineData(EngineCodes.MySql, "SELECT * FROM `mytable` LIMIT 18446744073709551615 OFFSET 5")]
        public void CompilerSpecificOffset(string engine, string sqlText)
        {
            var query = new Query("mytable")
                .ForMySql(q => q.Offset(5))
                .ForPostgreSql(q => q.Offset(10));

            var result = CompileFor(engine, query);

            Assert.Equal(2, query.GetComponents("offset").Count);
            Assert.Equal(sqlText, result.ToString());
        }

        [Fact]
        public void Offset_Takes_Generic_If_Needed()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Offset(20));

            var c = CompileFor(EngineCodes.MySql, query);
            var c2 = CompileFor(EngineCodes.PostgreSql, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 10", c.ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 5 OFFSET 20", c2.ToString());
        }

        [Fact]
        public void Limit_Takes_Generic_If_Needed()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Limit(20));

            var c = CompileFor(EngineCodes.MySql, query);
            var c2 = CompileFor(EngineCodes.PostgreSql, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 10", c.ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 20 OFFSET 10", c2.ToString());
        }

        [Fact]
        public void Can_Change_Generic_Limit_After_SpecificOffset()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Offset(20))
                .Limit(7);

            var c = CompileFor(EngineCodes.MySql, query);
            var c2 = CompileFor(EngineCodes.PostgreSql, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 7 OFFSET 10", c.ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 7 OFFSET 20", c2.ToString());
        }

        [Fact]
        public void Can_Change_Generic_Offset_After_SpecificLimit()
        {
            var query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Limit(20))
                .Offset(7);

            var c = CompileFor(EngineCodes.MySql, query);
            var c2 = CompileFor(EngineCodes.PostgreSql, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 7", c.ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 20 OFFSET 7", c2.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT * FROM [table] WHERE ([a] = 1 OR [a] = 2)")]
        public void Where_Nested(string engine, string sqlText)
        {
            var query = new Query("table")
                .Where(q => q.Where("a", 1).OrWhere("a", 2));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Fact]
        public void AdHoc_Throws_WhenNoColumnsProvided() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new string[0],
                    new object[][]
                    {
                        new object[] { },
                        new object[] { },
                    }));

        [Fact]
        public void AdHoc_Throws_WhenNoValueRowsProvided() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new[] { "a", "b", "c" },
                    new object[][]
                    {
                    }));

        [Fact]
        public void AdHoc_Throws_WhenColumnsOutnumberFieldValues() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new[] { "a", "b", "c", "d" },
                    new object[][]
                    {
                        new object[] { 1, 2, 3 },
                        new object[] { 4, 5, 6 },
                    }));

        [Fact]
        public void AdHoc_Throws_WhenFieldValuesOutNumberColumns() =>
            Assert.Throws<InvalidOperationException>(() =>
                new Query("rows").With("rows",
                    new[] { "a", "b" },
                    new object[][]
                    {
                        new object[] { 1, 2, 3 },
                        new object[] { 4, 5, 6 },
                    }));

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "WITH [rows] AS (SELECT [a] FROM (VALUES (1)) AS tbl ([a]))\nSELECT * FROM [rows]")]
        [InlineData(
            EngineCodes.PostgreSql,
            "WITH \"rows\" AS (SELECT 1 AS \"a\")\nSELECT * FROM \"rows\"")]
        [InlineData(
            EngineCodes.MySql,
            "WITH `rows` AS (SELECT 1 AS `a`)\nSELECT * FROM `rows`")]
        [InlineData(
            EngineCodes.Sqlite,
            "WITH \"rows\" AS (SELECT 1 AS \"a\")\nSELECT * FROM \"rows\"")]
        [InlineData(
            EngineCodes.Firebird,
            "WITH \"ROWS\" AS (SELECT 1 AS \"A\" FROM RDB$DATABASE)\nSELECT * FROM \"ROWS\"")]
        [InlineData(
            EngineCodes.Oracle,
            "WITH \"rows\" AS (SELECT 1 AS \"a\" FROM DUAL)\nSELECT * FROM \"rows\"")]
        public void AdHoc_SingletonRow(string engine, string sqlText)
        {
            var query = new Query("rows").With("rows",
                new[] { "a" },
                new object[][]
                {
                    new object[] { 1 },
                });

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "WITH [rows] AS (SELECT [a], [b], [c] FROM (VALUES (1, 2, 3), (4, 5, 6)) AS tbl ([a], [b], [c]))\nSELECT * FROM [rows]")]
        [InlineData(
            EngineCodes.PostgreSql,
            "WITH \"rows\" AS (SELECT 1 AS \"a\", 2 AS \"b\", 3 AS \"c\" UNION ALL SELECT 4 AS \"a\", 5 AS \"b\", 6 AS \"c\")\nSELECT * FROM \"rows\"")]
        [InlineData(
            EngineCodes.MySql,
            "WITH `rows` AS (SELECT 1 AS `a`, 2 AS `b`, 3 AS `c` UNION ALL SELECT 4 AS `a`, 5 AS `b`, 6 AS `c`)\nSELECT * FROM `rows`")]
        [InlineData(
            EngineCodes.Sqlite,
            "WITH \"rows\" AS (SELECT 1 AS \"a\", 2 AS \"b\", 3 AS \"c\" UNION ALL SELECT 4 AS \"a\", 5 AS \"b\", 6 AS \"c\")\nSELECT * FROM \"rows\"")]
        [InlineData(
            EngineCodes.Firebird,
            "WITH \"ROWS\" AS (SELECT 1 AS \"A\", 2 AS \"B\", 3 AS \"C\" FROM RDB$DATABASE UNION ALL SELECT 4 AS \"A\", 5 AS \"B\", 6 AS \"C\" FROM RDB$DATABASE)\nSELECT * FROM \"ROWS\"")]
        [InlineData(
            EngineCodes.Oracle,
            "WITH \"rows\" AS (SELECT 1 AS \"a\", 2 AS \"b\", 3 AS \"c\" FROM DUAL UNION ALL SELECT 4 AS \"a\", 5 AS \"b\", 6 AS \"c\" FROM DUAL)\nSELECT * FROM \"rows\"")]
        public void AdHoc_TwoRows(string engine, string sqlText)
        {
            var query = new Query("rows").With("rows",
                new[] { "a", "b", "c" },
                new object[][]
                {
                    new object[] { 1, 2, 3 },
                    new object[] { 4, 5, 6 },
                });

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void AdHoc_ProperBindingsPlacement()
        {
            var query = new Query("rows")
                .With("othercte", q => q.From("othertable").Where("othertable.status", "A"))
                .Where("rows.foo", "bar")
                .With("rows",
                    new[] { "a", "b", "c" },
                    new object[][]
                    {
                        new object[] { 1, 2, 3 },
                        new object[] { 4, 5, 6 },
                    })
                .Where("rows.baz", "buzz");

            var c = CompileFor(EngineCodes.SqlServer, query);

            Assert.Equal(string.Join("\n", new[]
            {
                "WITH [othercte] AS (SELECT * FROM [othertable] WHERE [othertable].[status] = 'A'),",
                "[rows] AS (SELECT [a], [b], [c] FROM (VALUES (1, 2, 3), (4, 5, 6)) AS tbl ([a], [b], [c]))",
                "SELECT * FROM [rows] WHERE [rows].[foo] = 'bar' AND [rows].[baz] = 'buzz'",
            }), c.ToString());
        }

        [Fact]
        public void UnsafeLiteral_Insert()
        {
            var query = new Query("Table").AsInsert(new
            {
                Count = new UnsafeLiteral("Count + 1")
            });

            var c = CompileFor(EngineCodes.SqlServer, query);

            Assert.Equal("INSERT INTO [Table] ([Count]) VALUES (Count + 1)", c.ToString());
        }

        [Fact]
        public void UnsafeLiteral_Update()
        {
            var query = new Query("Table").AsUpdate(new
            {
                Count = new UnsafeLiteral("Count + 1")
            });

            var c = CompileFor(EngineCodes.SqlServer, query);

            Assert.Equal("UPDATE [Table] SET [Count] = Count + 1", c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [Col] = cast(1 as bit)")]
        public void Passing_Boolean_To_Where_Should_Call_WhereTrue_Or_WhereFalse(string engine, string sqlText)
        {
            var query = new Query("Table").Where("Col", true);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [Col] = cast(0 as bit)")]
        public void Passing_Boolean_False_To_Where_Should_Call_WhereTrue_Or_WhereFalse(string engine, string sqlText)
        {
            var query = new Query("Table").Where("Col", false);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [Col] != cast(1 as bit)")]
        public void Passing_Negative_Boolean_To_Where_Should_Call_WhereTrue_Or_WhereFalse(
            string engine,
            string sqlText)
        {
            var query = new Query("Table").Where("Col", "!=", true);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] WHERE [Col] != cast(0 as bit)")]
        public void Passing_Negative_Boolean_False_To_Where_Should_Call_WhereTrue_Or_WhereFalse(
            string engine,
            string sqlText)
        {
            var query = new Query("Table").Where("Col", "!=", false);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }
    }
}
