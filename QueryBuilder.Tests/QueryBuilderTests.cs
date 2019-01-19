using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Execution;
using SqlKata.Compilers;
using SqlKata.Extensions;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public partial class QueryBuilderTests : TestSupport
    {
       [Fact]
        public void ColumnsEscaping()
        {
            var q = new Query().From("users").Select("mycol[isthis]");
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
    }
}
