using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System.Collections.Generic;
using Xunit;

namespace SqlKata.Tests
{
    public class AggregateTests : TestSupport
    {
        [Fact]
        public void Count()
        {
            var query = new Query("A").AsCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(*) AS `count` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(*) AS \"count\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT COUNT(*) AS \"COUNT\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void CountMultipleColumns()
        {
            var query = new Query("A").AsCount(new[] { "ColumnA", "ColumnB" });

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [countQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCount()
        {
            var query = new Query("A").Distinct().AsCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT * FROM [A]) AS [countQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCountMultipleColumns()
        {
            var query = new Query("A").Distinct().AsCount(new[] { "ColumnA", "ColumnB" });

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [ColumnA], [ColumnB] FROM [A]) AS [countQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Average()
        {
            var query = new Query("A").AsAverage("TTL");

            var c = Compile(query);

            Assert.Equal("SELECT AVG([TTL]) AS [avg] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Sum()
        {
            var query = new Query("A").AsSum("PacketsDropped");

            var c = Compile(query);

            Assert.Equal("SELECT SUM([PacketsDropped]) AS [sum] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Max()
        {
            var query = new Query("A").AsMax("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MAX([LatencyMs]) AS [max] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Min()
        {
            var query = new Query("A").AsMin("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MIN([LatencyMs]) AS [min] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void HavingAggregate()
        {
            var query = new Query().From("TABLENAME").GroupBy("Title").HavingSum("Title", ">", 21).OrHavingAvg("Title", ">", 21).HavingCount("Title", ">", 21).Select("Title");

            var compiler = Compile(query);

            Assert.Equal("SELECT [Title] FROM [TABLENAME] GROUP BY [Title] HAVING SUM([Title]) > 21 OR AVG([Title]) > 21 AND COUNT([Title]) > 21", compiler[EngineCodes.SqlServer]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(\"Title\") > 21 OR AVG(\"Title\") > 21 AND COUNT(\"Title\") > 21", compiler[EngineCodes.PostgreSql]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(\"Title\") > 21 OR AVG(\"Title\") > 21 AND COUNT(\"Title\") > 21", compiler[EngineCodes.Oracle]);

            Assert.Equal("SELECT `Title` FROM `TABLENAME` GROUP BY `Title` HAVING SUM(`Title`) > 21 OR AVG(`Title`) > 21 AND COUNT(`Title`) > 21", compiler[EngineCodes.MySql]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(\"Title\") > 21 OR AVG(\"Title\") > 21 AND COUNT(\"Title\") > 21", compiler[EngineCodes.Sqlite]);
        }

        [Fact]
        public void HavingAggregateWithKeyword()
        {
            var query = new Query().From("TABLENAME").GroupBy("Title").HavingDistinctSum("Title", ">", 21).OrHavingAllCount("Title", ">", 21).Select("Title");

            var compiler = Compile(query);

            Assert.Equal("SELECT [Title] FROM [TABLENAME] GROUP BY [Title] HAVING SUM(DISTINCT [Title]) > 21 OR COUNT(ALL [Title]) > 21", compiler[EngineCodes.SqlServer]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(DISTINCT \"Title\") > 21 OR COUNT(ALL \"Title\") > 21", compiler[EngineCodes.PostgreSql]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(DISTINCT \"Title\") > 21 OR COUNT(ALL \"Title\") > 21", compiler[EngineCodes.Oracle]);

            Assert.Equal("SELECT `Title` FROM `TABLENAME` GROUP BY `Title` HAVING SUM(DISTINCT `Title`) > 21 OR COUNT(ALL `Title`) > 21", compiler[EngineCodes.MySql]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(DISTINCT \"Title\") > 21 OR COUNT(ALL \"Title\") > 21", compiler[EngineCodes.Sqlite]);

        }

        [Fact]
        public void HavingAggregateWithFilter()
        {
            var query = new Query().From("TABLENAME").GroupBy("Title").HavingSum(d => d.WhereIn("Title", new List<int>() { 11, 12, 13 })).OrHavingAvg(d => d.WhereBetween("Title", 11, 12)).HavingCount(d => d.WhereLike("Title", "having")).HavingCount(d => d).Select("Title");

            var compiler = Compile(query);

            Assert.Equal("SELECT [Title] FROM [TABLENAME] GROUP BY [Title] HAVING SUM([Title]) IN (11, 12, 13) OR AVG([Title]) BETWEEN 11 AND 12 AND COUNT(LOWER([Title])) like 'having'", compiler[EngineCodes.SqlServer]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(\"Title\") IN (11, 12, 13) OR AVG(\"Title\") BETWEEN 11 AND 12 AND COUNT(\"Title\") ilike 'having'", compiler[EngineCodes.PostgreSql]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(\"Title\") IN (11, 12, 13) OR AVG(\"Title\") BETWEEN 11 AND 12 AND COUNT(LOWER(\"Title\")) like 'having'", compiler[EngineCodes.Oracle]);

            Assert.Equal("SELECT `Title` FROM `TABLENAME` GROUP BY `Title` HAVING SUM(`Title`) IN (11, 12, 13) OR AVG(`Title`) BETWEEN 11 AND 12 AND COUNT(LOWER(`Title`)) like 'having'", compiler[EngineCodes.MySql]);

            Assert.Equal("SELECT \"Title\" FROM \"TABLENAME\" GROUP BY \"Title\" HAVING SUM(\"Title\") IN (11, 12, 13) OR AVG(\"Title\") BETWEEN 11 AND 12 AND COUNT(LOWER(\"Title\")) like 'having'", compiler[EngineCodes.Sqlite]);

        }
    }
}
