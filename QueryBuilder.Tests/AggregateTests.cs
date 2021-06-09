using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System;
using Xunit;

namespace SqlKata.Tests
{
    public class AggregateTests : TestSupport
    {
        [Fact]
        public void SelectAggregateEmpty()
        {
            Assert.Throws<ArgumentException>(() => new Query("A").SelectAggregate("aggregate", new string[] { }));
        }

        [Fact]
        public void SelectAggregate()
        {
            var query = new Query("A").SelectAggregate("aggregate", new[] { "Column" });

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE([Column]) AS [aggregate] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(`Column`) AS `aggregate` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(\"Column\") AS \"aggregate\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT AGGREGATE(\"COLUMN\") AS \"AGGREGATE\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void SelectAggregateAlias()
        {
            var query = new Query("A").SelectAggregate("aggregate", new[] { "Column" }, "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE([Column]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(`Column`) AS `Alias` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(\"Column\") AS \"Alias\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT AGGREGATE(\"COLUMN\") AS \"ALIAS\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void SelectAggregateMultipleColumns()
        {
            var query = new Query("A").SelectAggregate("aggregate", new[] { "Column1", "Column2" });

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE(*) AS [aggregate] FROM (SELECT 1 FROM [A] WHERE [Column1] IS NOT NULL AND [Column2] IS NOT NULL) AS [AggregateQuery]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(*) AS `aggregate` FROM (SELECT 1 FROM `A` WHERE `Column1` IS NOT NULL AND `Column2` IS NOT NULL) AS `AggregateQuery`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"AGGREGATE\" FROM (SELECT 1 FROM \"A\" WHERE \"COLUMN1\" IS NOT NULL AND \"COLUMN2\" IS NOT NULL) AS \"AGGREGATEQUERY\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"aggregate\" FROM (SELECT 1 FROM \"A\" WHERE \"Column1\" IS NOT NULL AND \"Column2\" IS NOT NULL) AS \"AggregateQuery\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void SelectAggregateMultipleColumnsAlias()
        {
            var query = new Query("A").SelectAggregate("aggregate", new[] { "Column1", "Column2" }, "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE(*) AS [Alias] FROM (SELECT 1 FROM [A] WHERE [Column1] IS NOT NULL AND [Column2] IS NOT NULL) AS [AliasAggregateQuery]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(*) AS `Alias` FROM (SELECT 1 FROM `A` WHERE `Column1` IS NOT NULL AND `Column2` IS NOT NULL) AS `AliasAggregateQuery`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"ALIAS\" FROM (SELECT 1 FROM \"A\" WHERE \"COLUMN1\" IS NOT NULL AND \"COLUMN2\" IS NOT NULL) AS \"ALIASAGGREGATEQUERY\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"Alias\" FROM (SELECT 1 FROM \"A\" WHERE \"Column1\" IS NOT NULL AND \"Column2\" IS NOT NULL) AS \"AliasAggregateQuery\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void SelectCount()
        {
            var query = new Query("A").SelectCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(*) AS `count` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(*) AS \"count\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT COUNT(*) AS \"COUNT\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void SelectCountStarAlias()
        {
            var query = new Query("A").SelectCount("*", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(*) AS `Alias` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(*) AS \"Alias\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT COUNT(*) AS \"ALIAS\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void SelectCountColumnAlias()
        {
            var query = new Query("A").SelectCount("Column", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT([Column]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(`Column`) AS `Alias` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(\"Column\") AS \"Alias\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT COUNT(\"COLUMN\") AS \"ALIAS\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void SelectCountDoesntModifyColumns()
        {
            {
                var columns = new string[] { };
                var query = new Query("A").SelectCount(columns);
                Compile(query);
                Assert.Equal(columns, new string[] { });
            }
            {
                var columns = new[] { "ColumnA", "ColumnB" };
                var query = new Query("A").SelectCount(columns);
                Compile(query);
                Assert.Equal(columns, new[] { "ColumnA", "ColumnB" });
            }
        }

        [Fact]
        public void CountMultipleColumns()
        {
            var query = new Query("A").SelectCount(new[] { "ColumnA", "ColumnB" });

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void SelectCountMultipleColumns()
        {
            var query = new Query("A").SelectCount(new[] { "ColumnA", "ColumnB" }, "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [Alias] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [AliasCountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCount()
        {
            var query = new Query("A").Distinct().SelectCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT * FROM [A]) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCountMultipleColumns()
        {
            var query = new Query("A").Distinct().SelectCount(new[] { "ColumnA", "ColumnB" });

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [ColumnA], [ColumnB] FROM [A]) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Average()
        {
            var query = new Query("A").SelectAverage("TTL");

            var c = Compile(query);

            Assert.Equal("SELECT AVG([TTL]) AS [avg] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void AverageAlias()
        {
            var query = new Query("A").SelectAverage("TTL", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT AVG([TTL]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Sum()
        {
            var query = new Query("A").SelectSum("PacketsDropped");

            var c = Compile(query);

            Assert.Equal("SELECT SUM([PacketsDropped]) AS [sum] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void SumAlias()
        {
            var query = new Query("A").SelectSum("PacketsDropped", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT SUM([PacketsDropped]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Max()
        {
            var query = new Query("A").SelectMax("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MAX([LatencyMs]) AS [max] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void MaxAlias()
        {
            var query = new Query("A").SelectMax("LatencyMs", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT MAX([LatencyMs]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Min()
        {
            var query = new Query("A").SelectMin("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MIN([LatencyMs]) AS [min] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void MinAlias()
        {
            var query = new Query("A").SelectMin("LatencyMs", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT MIN([LatencyMs]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }
    }
}
