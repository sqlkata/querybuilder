using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SqlKata.Tests
{
    public class GeneralTests : TestSupport
    {
        [Fact]
        public void ColumnsEscaping()
        {
            Query q = new Query().From("users")
                .Select("mycol[isthis]");

            IReadOnlyDictionary<string, string> c = Compile(q);

            Assert.Equal("SELECT [mycol[isthis]]] FROM [users]", c[EngineCodes.SqlServer]);
        }


        [Fact]
        public void InnerScopeEngineWithinCTE()
        {
            Query series = new Query("table")
                .ForPostgreSql(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
                .ForFirebird(q => q.WhereRaw("firebird = 1"));
            Query query = new Query("series").With("series", series);

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("WITH [series] AS (SELECT * FROM [table] WHERE sqlsrv = 1)\nSELECT * FROM [series]", c[EngineCodes.SqlServer]);

            Assert.Equal("WITH \"series\" AS (SELECT * FROM \"table\" WHERE postgres = true)\nSELECT * FROM \"series\"",
                c[EngineCodes.PostgreSql]);
            Assert.Equal("WITH \"SERIES\" AS (SELECT * FROM \"TABLE\" WHERE firebird = 1)\nSELECT * FROM \"SERIES\"",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InnerScopeEngineWithinSubQuery()
        {
            Query series = new Query("table")
                .ForPostgreSql(q => q.WhereRaw("postgres = true"))
                .ForSqlServer(q => q.WhereRaw("sqlsrv = 1"))
                .ForFirebird(q => q.WhereRaw("firebird = 1"));
            Query query = new Query("series").From(series.As("series"));

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT * FROM (SELECT * FROM [table] WHERE sqlsrv = 1) AS [series]", c[EngineCodes.SqlServer]);

            Assert.Equal("SELECT * FROM (SELECT * FROM \"table\" WHERE postgres = true) AS \"series\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT * FROM (SELECT * FROM \"TABLE\" WHERE firebird = 1) AS \"SERIES\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void ItShouldCacheMethodInfoByType()
        {
            TestSqlServerCompiler compiler = new TestSqlServerCompiler();

            MethodInfo call1 = compiler.Call_FindCompilerMethodInfo(
                typeof(BasicCondition), "CompileBasicCondition"
            );

            MethodInfo call2 = compiler.Call_FindCompilerMethodInfo(
                typeof(BasicCondition), "CompileBasicCondition"
            );

            Assert.Same(call1, call2);
        }

        [Fact]
        public void Return_Different_MethodInfo_WhenSame_Method_With_Different_GenericTypes()
        {
            TestSqlServerCompiler compiler = new TestSqlServerCompiler();

            MethodInfo call1 = compiler.Call_FindCompilerMethodInfo(
                typeof(NestedCondition<Query>), "CompileNestedCondition"
            );

            MethodInfo call2 = compiler.Call_FindCompilerMethodInfo(
                typeof(NestedCondition<Join>), "CompileNestedCondition"
            );

            Assert.NotSame(call1, call2);
        }

        [Fact]
        public void Should_Equal_AfterMultipleCompile()
        {
            Query query = new Query()
                .Select("Id", "Name")
                .From("Table")
                .OrderBy("Name")
                .Limit(20)
                .Offset(1);

            IReadOnlyDictionary<string, string> first = Compile(query);
            Assert.Equal(
                "SELECT * FROM (SELECT [Id], [Name], ROW_NUMBER() OVER (ORDER BY [Name]) AS [row_num] FROM [Table]) AS [results_wrapper] WHERE [row_num] BETWEEN 2 AND 21",
                first[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `Id`, `Name` FROM `Table` ORDER BY `Name` LIMIT 20 OFFSET 1", first[EngineCodes.MySql]);
            Assert.Equal("SELECT \"Id\", \"Name\" FROM \"Table\" ORDER BY \"Name\" LIMIT 20 OFFSET 1", first[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"ID\", \"NAME\" FROM \"TABLE\" ORDER BY \"NAME\" ROWS 2 TO 21", first[EngineCodes.Firebird]);

            IReadOnlyDictionary<string, string> second = Compile(query);

            Assert.Equal(first[EngineCodes.SqlServer], second[EngineCodes.SqlServer]);
            Assert.Equal(first[EngineCodes.MySql], second[EngineCodes.MySql]);
            Assert.Equal(first[EngineCodes.PostgreSql], second[EngineCodes.PostgreSql]);
            Assert.Equal(first[EngineCodes.Firebird], second[EngineCodes.Firebird]);
        }

        [Fact]
        public void Raw_WrapIdentifiers()
        {
            Query query = new Query("Users").SelectRaw("[Id], [Name], {Age}");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT [Id], [Name], [Age] FROM [Users]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `Id`, `Name`, `Age` FROM `Users`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"Id\", \"Name\", \"Age\" FROM \"Users\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT \"Id\", \"Name\", \"Age\" FROM \"USERS\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void Raw_WrapIdentifiers_Escaped()
        {
            Query query = new Query("Users").SelectRaw("'\\{1,2,3\\}'::int\\[\\]");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT '{1,2,3}'::int[] FROM \"Users\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void WrapWithSpace()
        {
            SqlServerCompiler compiler = new SqlServerCompiler();

            Assert.Equal("[My Table] AS [Table]", compiler.Wrap("My Table as Table"));
        }

        [Fact]
        public void WrapWithDotes()
        {
            SqlServerCompiler compiler = new SqlServerCompiler();

            Assert.Equal("[My Schema].[My Table] AS [Table]", compiler.Wrap("My Schema.My Table as Table"));
        }

        [Fact]
        public void WrapWithMultipleSpaces()
        {
            SqlServerCompiler compiler = new SqlServerCompiler();

            Assert.Equal("[My Table One] AS [Table One]", compiler.Wrap("My Table One as Table One"));
        }

        [Fact]
        public void CompilerSpecificFrom()
        {
            Query query = new Query()
                .ForSqlServer(q => q.From("mssql"))
                .ForPostgreSql(q => q.From("pgsql"))
                .ForMySql(q => q.From("mysql"));
            string[] engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [mssql]", c[EngineCodes.SqlServer].RawSql);
            Assert.Equal("SELECT * FROM \"pgsql\"", c[EngineCodes.PostgreSql].RawSql);
            Assert.Equal("SELECT * FROM `mysql`", c[EngineCodes.MySql].RawSql);
        }

        [Fact]
        public void CompilerSpecificFromRaw()
        {
            Query query = new Query()
                .ForSqlServer(q => q.FromRaw("[mssql]"))
                .ForPostgreSql(q => q.FromRaw("[pgsql]"))
                .ForMySql(q => q.FromRaw("[mysql]"));
            string[] engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [mssql]", c[EngineCodes.SqlServer].RawSql);
            Assert.Equal("SELECT * FROM \"pgsql\"", c[EngineCodes.PostgreSql].RawSql);
            Assert.Equal("SELECT * FROM `mysql`", c[EngineCodes.MySql].RawSql);
        }

        [Fact]
        public void CompilerSpecificFromMixed()
        {
            Query query = new Query()
                .ForSqlServer(q => q.From("mssql"))
                .ForPostgreSql(q => q.FromRaw("[pgsql]"))
                .ForMySql(q => q.From("mysql"));
            string[] engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [mssql]", c[EngineCodes.SqlServer].RawSql);
            Assert.Equal("SELECT * FROM \"pgsql\"", c[EngineCodes.PostgreSql].RawSql);
            Assert.Equal("SELECT * FROM `mysql`", c[EngineCodes.MySql].RawSql);
        }

        [Fact]
        public void OneFromPerEngine()
        {
            Query query = new Query("generic")
                .ForSqlServer(q => q.From("dnu"))
                .ForSqlServer(q => q.From("mssql"));
            string[] engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

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
            Query query = new Query();
            if (table != null)
                query.From(table);
            query.AddOrReplaceComponent("from", new FromClause() { Table = "updated", Engine = engine });
            IEnumerable<FromClause> froms = query.Clauses.OfType<FromClause>();

            Assert.Single(froms);
            Assert.Equal("updated", froms.Single().Table);
        }

        [Theory]
        [InlineData(null, "generic")]
        [InlineData(EngineCodes.SqlServer, "mssql")]
        [InlineData(EngineCodes.MySql, "generic")]
        public void GetOneComponent_Prefers_Engine(string engine, string column)
        {
            Query query = new Query()
                .Where("generic", "foo")
                .ForSqlServer(q => q.Where("mssql", "foo"));

            BasicCondition where = query.GetOneComponent("where", engine) as BasicCondition;

            Assert.NotNull(where);
            Assert.Equal(column, where.Column);
        }

        [Fact]
        public void AddOrReplace_Throws_MoreThanOne()
        {
            Query query = new Query()
                .Where("a", "b")
                .Where("c", "d");

            Action act = () => query.AddOrReplaceComponent("where", new BasicCondition());
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void OneLimitPerEngine()
        {
            Query query = new Query("mytable")
                .ForSqlServer(q => q.Limit(5))
                .ForSqlServer(q => q.Limit(10));

            List<LimitClause> limits = query.GetComponents<LimitClause>("limit", EngineCodes.SqlServer);
            Assert.Single(limits);
            Assert.Equal(10, limits.Single().Limit);
        }

        [Fact]
        public void CompilerSpecificLimit()
        {
            Query query = new Query("mytable")
                .ForSqlServer(q => q.Limit(5))
                .ForPostgreSql(q => q.Limit(10));

            string[] engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal(2, query.GetComponents("limit").Count());
            Assert.Equal("SELECT TOP (5) * FROM [mytable]", c[EngineCodes.SqlServer].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 10", c[EngineCodes.PostgreSql].ToString());
            Assert.Equal("SELECT * FROM `mytable`", c[EngineCodes.MySql].ToString());
        }

        [Fact]
        public void OneOffsetPerEngine()
        {
            Query query = new Query("mytable")
                .ForSqlServer(q => q.Offset(5))
                .ForSqlServer(q => q.Offset(10));

            List<OffsetClause> limits = query.GetComponents<OffsetClause>("offset", EngineCodes.SqlServer);
            Assert.Single(limits);
            Assert.Equal(10, limits.Single().Offset);
        }

        [Fact]
        public void CompilerSpecificOffset()
        {
            Query query = new Query("mytable")
                .ForMySql(q => q.Offset(5))
                .ForPostgreSql(q => q.Offset(10));

            string[] engines = new[] { EngineCodes.SqlServer, EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal(2, query.GetComponents("offset").Count());
            Assert.Equal("SELECT * FROM `mytable` LIMIT 18446744073709551615 OFFSET 5", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" OFFSET 10", c[EngineCodes.PostgreSql].ToString());
            Assert.Equal("SELECT * FROM [mytable]", c[EngineCodes.SqlServer].ToString());
        }

        [Fact]
        public void Limit_Takes_Generic_If_Needed()
        {
            Query query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Offset(20));

            string[] engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 10", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 5 OFFSET 20", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Offset_Takes_Generic_If_Needed()
        {
            Query query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Limit(20));

            string[] engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 10", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 20 OFFSET 10", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Can_Change_Generic_Limit_After_SpecificOffset()
        {
            Query query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Offset(20))
                .Limit(7);

            string[] engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 7 OFFSET 10", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 7 OFFSET 20", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Can_Change_Generic_Offset_After_SpecificLimit()
        {
            Query query = new Query("mytable")
                .Limit(5)
                .Offset(10)
                .ForPostgreSql(q => q.Limit(20))
                .Offset(7);

            string[] engines = new[] { EngineCodes.MySql, EngineCodes.PostgreSql };
            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM `mytable` LIMIT 5 OFFSET 7", c[EngineCodes.MySql].ToString());
            Assert.Equal("SELECT * FROM \"mytable\" LIMIT 20 OFFSET 7", c[EngineCodes.PostgreSql].ToString());
        }

        [Fact]
        public void Where_Nested()
        {
            Query query = new Query("table")
            .Where(q => q.Where("a", 1).OrWhere("a", 2));

            string[] engines = new[] {
                EngineCodes.SqlServer,
            };

            TestSqlResultContainer c = Compilers.Compile(engines, query);

            Assert.Equal("SELECT * FROM [table] WHERE ([a] = 1 OR [a] = 2)", c[EngineCodes.SqlServer].ToString());
        }
    }
}
