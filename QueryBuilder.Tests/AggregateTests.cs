using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests
{
    public class AggregateTests : TestSupport
    {
        [Theory]
        [InlineData(EngineCodes.Firebird, "SELECT COUNT(*) AS \"COUNT\" FROM \"A\"")]
        [InlineData(EngineCodes.MySql, "SELECT COUNT(*) AS `count` FROM `A`")]
        [InlineData(EngineCodes.Oracle, "SELECT COUNT(*) \"count\" FROM \"A\"")]
        [InlineData(EngineCodes.PostgreSql, "SELECT COUNT(*) AS \"count\" FROM \"A\"")]
        [InlineData(EngineCodes.Sqlite, "SELECT COUNT(*) AS \"count\" FROM \"A\"")]
        [InlineData(EngineCodes.SqlServer, "SELECT COUNT(*) AS [count] FROM [A]")]
        public void Count(string engine, string query)
        {
            var q = new Query("A").AsCount();

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird,
            "SELECT COUNT(*) AS \"COUNT\" FROM (SELECT 1 FROM \"A\" WHERE \"COLUMNA\" IS NOT NULL AND \"COLUMNB\" IS NOT NULL) AS \"COUNTQUERY\"")]
        [InlineData(EngineCodes.MySql,
            "SELECT COUNT(*) AS `count` FROM (SELECT 1 FROM `A` WHERE `ColumnA` IS NOT NULL AND `ColumnB` IS NOT NULL) AS `countQuery`")]
        [InlineData(EngineCodes.Oracle,
            "SELECT COUNT(*) \"count\" FROM (SELECT 1 FROM \"A\" WHERE \"ColumnA\" IS NOT NULL AND \"ColumnB\" IS NOT NULL) \"countQuery\"")]
        [InlineData(EngineCodes.PostgreSql,
            "SELECT COUNT(*) AS \"count\" FROM (SELECT 1 FROM \"A\" WHERE \"ColumnA\" IS NOT NULL AND \"ColumnB\" IS NOT NULL) AS \"countQuery\"")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT COUNT(*) AS \"count\" FROM (SELECT 1 FROM \"A\" WHERE \"ColumnA\" IS NOT NULL AND \"ColumnB\" IS NOT NULL) AS \"countQuery\"")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT COUNT(*) AS [count] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [countQuery]")]
        public void CountMultipleColumns(string engine, string query)
        {
            var q = new Query("A").AsCount(["ColumnA", "ColumnB"]);

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird,
            "SELECT COUNT(*) AS \"COUNT\" FROM (SELECT DISTINCT * FROM \"A\") AS \"COUNTQUERY\"")]
        [InlineData(EngineCodes.MySql, "SELECT COUNT(*) AS `count` FROM (SELECT DISTINCT * FROM `A`) AS `countQuery`")]
        [InlineData(EngineCodes.Oracle, "SELECT COUNT(*) \"count\" FROM (SELECT DISTINCT * FROM \"A\") \"countQuery\"")]
        [InlineData(EngineCodes.PostgreSql,
            "SELECT COUNT(*) AS \"count\" FROM (SELECT DISTINCT * FROM \"A\") AS \"countQuery\"")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT COUNT(*) AS \"count\" FROM (SELECT DISTINCT * FROM \"A\") AS \"countQuery\"")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT * FROM [A]) AS [countQuery]")]
        public void DistinctCount(string engine, string query)
        {
            var q = new Query("A").Distinct().AsCount();

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird,
            "SELECT COUNT(*) AS \"COUNT\" FROM (SELECT DISTINCT \"COLUMNA\", \"COLUMNB\" FROM \"A\") AS \"COUNTQUERY\"")]
        [InlineData(EngineCodes.MySql,
            "SELECT COUNT(*) AS `count` FROM (SELECT DISTINCT `ColumnA`, `ColumnB` FROM `A`) AS `countQuery`")]
        [InlineData(EngineCodes.Oracle,
            "SELECT COUNT(*) \"count\" FROM (SELECT DISTINCT \"ColumnA\", \"ColumnB\" FROM \"A\") \"countQuery\"")]
        [InlineData(EngineCodes.PostgreSql,
            "SELECT COUNT(*) AS \"count\" FROM (SELECT DISTINCT \"ColumnA\", \"ColumnB\" FROM \"A\") AS \"countQuery\"")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT COUNT(*) AS \"count\" FROM (SELECT DISTINCT \"ColumnA\", \"ColumnB\" FROM \"A\") AS \"countQuery\"")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [ColumnA], [ColumnB] FROM [A]) AS [countQuery]")]
        public void DistinctCountMultipleColumns(string engine, string query)
        {
            var q = new Query("A").Distinct().AsCount(new[] { "ColumnA", "ColumnB" });

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird,
            "SELECT AVG(\"TTL\") AS \"AVG\" FROM \"A\"")]
        [InlineData(EngineCodes.MySql,
            "SELECT AVG(`TTL`) AS `avg` FROM `A`")]
        [InlineData(EngineCodes.Oracle,
            "SELECT AVG(\"TTL\") \"avg\" FROM \"A\"")]
        [InlineData(EngineCodes.PostgreSql,
            "SELECT AVG(\"TTL\") AS \"avg\" FROM \"A\"")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT AVG(\"TTL\") AS \"avg\" FROM \"A\"")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT AVG([TTL]) AS [avg] FROM [A]")]
        public void Average(string engine, string query)
        {
            var q = new Query("A").AsAverage("TTL");

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird,
            "SELECT SUM(\"PACKETSDROPPED\") AS \"SUM\" FROM \"A\"")]
        [InlineData(EngineCodes.MySql,
            "SELECT SUM(`PacketsDropped`) AS `sum` FROM `A`")]
        [InlineData(EngineCodes.Oracle,
            "SELECT SUM(\"PacketsDropped\") \"sum\" FROM \"A\"")]
        [InlineData(EngineCodes.PostgreSql,
            "SELECT SUM(\"PacketsDropped\") AS \"sum\" FROM \"A\"")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT SUM(\"PacketsDropped\") AS \"sum\" FROM \"A\"")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT SUM([PacketsDropped]) AS [sum] FROM [A]")]
        public void Sum(string engine, string query)
        {
            var q = new Query("A").AsSum("PacketsDropped");

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird,
            "SELECT MAX(\"LATENCYMS\") AS \"MAX\" FROM \"A\"")]
        [InlineData(EngineCodes.MySql,
            "SELECT MAX(`LatencyMs`) AS `max` FROM `A`")]
        [InlineData(EngineCodes.Oracle,
            "SELECT MAX(\"LatencyMs\") \"max\" FROM \"A\"")]
        [InlineData(EngineCodes.PostgreSql,
            "SELECT MAX(\"LatencyMs\") AS \"max\" FROM \"A\"")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT MAX(\"LatencyMs\") AS \"max\" FROM \"A\"")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT MAX([LatencyMs]) AS [max] FROM [A]")]
        public void Max(string engine, string query)
        {
            var q = new Query("A").AsMax("LatencyMs");

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird,
            "SELECT MIN(\"LATENCYMS\") AS \"MIN\" FROM \"A\"")]
        [InlineData(EngineCodes.MySql,
            "SELECT MIN(`LatencyMs`) AS `min` FROM `A`")]
        [InlineData(EngineCodes.Oracle,
            "SELECT MIN(\"LatencyMs\") \"min\" FROM \"A\"")]
        [InlineData(EngineCodes.PostgreSql,
            "SELECT MIN(\"LatencyMs\") AS \"min\" FROM \"A\"")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT MIN(\"LatencyMs\") AS \"min\" FROM \"A\"")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT MIN([LatencyMs]) AS [min] FROM [A]")]
        public void Min(string engine, string query)
        {
            var q = new Query("A").AsMin("LatencyMs");

            var result = CompileFor(engine, q);

            Assert.Equal(query, result.ToString());
        }
    }
}
