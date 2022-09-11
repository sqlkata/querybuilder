using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System;
using Xunit;

namespace SqlKata.Tests
{
    public class AggregateTests : TestSupport
    {
        [Fact]
        public void AsAggregateEmpty()
        {
            Assert.Throws<ArgumentException>(() => new Query("A").AsAggregate("aggregate", new string[] { }));
        }

        [Fact]
        public void AsAggregate()
        {
            var query = new Query("A").AsAggregate("aggregate", new[] { "Column" });

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE([Column]) AS [aggregate] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(`Column`) AS `aggregate` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(\"Column\") AS \"aggregate\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT AGGREGATE(\"COLUMN\") AS \"AGGREGATE\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void AsAggregateAlias()
        {
            var query = new Query("A").AsAggregate("aggregate", new[] { "Column" }, "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE([Column]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(`Column`) AS `Alias` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(\"Column\") AS \"Alias\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT AGGREGATE(\"COLUMN\") AS \"ALIAS\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void AsAggregateMultipleColumns()
        {
            var query = new Query("A").AsAggregate("aggregate", new[] { "Column1", "Column2" });

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE(*) AS [aggregate] FROM (SELECT 1 FROM [A] WHERE [Column1] IS NOT NULL AND [Column2] IS NOT NULL) AS [AggregateQuery]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(*) AS `aggregate` FROM (SELECT 1 FROM `A` WHERE `Column1` IS NOT NULL AND `Column2` IS NOT NULL) AS `AggregateQuery`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"AGGREGATE\" FROM (SELECT 1 FROM \"A\" WHERE \"COLUMN1\" IS NOT NULL AND \"COLUMN2\" IS NOT NULL) AS \"AGGREGATEQUERY\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"aggregate\" FROM (SELECT 1 FROM \"A\" WHERE \"Column1\" IS NOT NULL AND \"Column2\" IS NOT NULL) AS \"AggregateQuery\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void AsAggregateMultipleColumnsAlias()
        {
            var query = new Query("A").AsAggregate("aggregate", new[] { "Column1", "Column2" }, "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE(*) AS [Alias] FROM (SELECT 1 FROM [A] WHERE [Column1] IS NOT NULL AND [Column2] IS NOT NULL) AS [AliasAggregateQuery]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(*) AS `Alias` FROM (SELECT 1 FROM `A` WHERE `Column1` IS NOT NULL AND `Column2` IS NOT NULL) AS `AliasAggregateQuery`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"ALIAS\" FROM (SELECT 1 FROM \"A\" WHERE \"COLUMN1\" IS NOT NULL AND \"COLUMN2\" IS NOT NULL) AS \"ALIASAGGREGATEQUERY\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT AGGREGATE(*) AS \"Alias\" FROM (SELECT 1 FROM \"A\" WHERE \"Column1\" IS NOT NULL AND \"Column2\" IS NOT NULL) AS \"AliasAggregateQuery\"", c[EngineCodes.PostgreSql]);
        }

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
        public void CountAsStarAlias()
        {
            var query = new Query("A").AsCountAs("*", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(*) AS `Alias` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(*) AS \"Alias\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT COUNT(*) AS \"ALIAS\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void CountAsColumnAlias()
        {
            var query = new Query("A").AsCountAs("Column", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT([Column]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(`Column`) AS `Alias` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(\"Column\") AS \"Alias\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT COUNT(\"COLUMN\") AS \"ALIAS\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void CountDoesntModifyColumns()
        {
            {
                var columns = new string[] { };
                var query = new Query("A").AsCount(columns);
                Compile(query);
                Assert.Equal(columns, new string[] { });
            }
            {
                var columns = new[] { "ColumnA", "ColumnB" };
                var query = new Query("A").AsCount(columns);
                Compile(query);
                Assert.Equal(columns, new[] { "ColumnA", "ColumnB" });
            }
        }

        [Fact]
        public void CountMultipleColumns()
        {
            var query = new Query("A").AsCount(new[] { "ColumnA", "ColumnB" });

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void CountAsMultipleColumns()
        {
            var query = new Query("A").AsCountAs(new[] { "ColumnA", "ColumnB" }, "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [Alias] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [AliasCountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCount()
        {
            var query = new Query("A").Distinct().AsCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT * FROM [A]) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCountMultipleColumns()
        {
            var query = new Query("A").Distinct().AsCount(new[] { "ColumnA", "ColumnB" });

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [ColumnA], [ColumnB] FROM [A]) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Average()
        {
            var query = new Query("A").AsAverage("TTL");

            var c = Compile(query);

            Assert.Equal("SELECT AVG([TTL]) AS [avg] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void AverageAlias()
        {
            var query = new Query("A").AsAverageAs("TTL", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT AVG([TTL]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Sum()
        {
            var query = new Query("A").AsSum("PacketsDropped");

            var c = Compile(query);

            Assert.Equal("SELECT SUM([PacketsDropped]) AS [sum] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void SumAlias()
        {
            var query = new Query("A").AsSumAs("PacketsDropped", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT SUM([PacketsDropped]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Max()
        {
            var query = new Query("A").AsMax("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MAX([LatencyMs]) AS [max] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void MaxAlias()
        {
            var query = new Query("A").AsMaxAs("LatencyMs", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT MAX([LatencyMs]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Min()
        {
            var query = new Query("A").AsMin("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MIN([LatencyMs]) AS [min] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void MinAlias()
        {
            var query = new Query("A").AsMinAs("LatencyMs", "Alias");

            var c = Compile(query);

            Assert.Equal("SELECT MIN([LatencyMs]) AS [Alias] FROM [A]", c[EngineCodes.SqlServer]);
        }
    }
}
