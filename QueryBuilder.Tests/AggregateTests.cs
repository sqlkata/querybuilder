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
            Assert.Throws<ArgumentException>(() =>
                new Query("A").SelectAggregate("aggregate", new[] { "Column1", "Column2" })
            );
        }

        [Fact]
        public void SelectAggregateMultipleColumnsAlias()
        {
            Assert.Throws<ArgumentException>(() =>
                new Query("A").SelectAggregate("aggregate", new[] { "Column1", "Column2" }, "Alias")
            );
        }

        [Fact]
        public void MultipleAggregatesPerQuery()
        {
            var query = new Query()
                .SelectMin("MinColumn")
                .SelectMax("MaxColumn")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MIN([MinColumn]) AS [min], MAX([MaxColumn]) AS [max] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT MIN(`MinColumn`) AS `min`, MAX(`MaxColumn`) AS `max` FROM `Table`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT MIN(\"MINCOLUMN\") AS \"MIN\", MAX(\"MAXCOLUMN\") AS \"MAX\" FROM \"TABLE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT MIN(\"MinColumn\") AS \"min\", MAX(\"MaxColumn\") AS \"max\" FROM \"Table\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void AggregatesAndNonAggregatesCanBeMixedInQueries1()
        {
            var query = new Query()
                .Select("ColumnA")
                .SelectMax("ColumnB")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT [ColumnA], MAX([ColumnB]) AS [max] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT `ColumnA`, MAX(`ColumnB`) AS `max` FROM `Table`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT \"COLUMNA\", MAX(\"COLUMNB\") AS \"MAX\" FROM \"TABLE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT \"ColumnA\", MAX(\"ColumnB\") AS \"max\" FROM \"Table\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void AggregatesAndNonAggregatesCanBeMixedInQueries2()
        {
            var query = new Query()
                .SelectMax("ColumnA")
                .Select("ColumnB")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MAX([ColumnA]) AS [max], [ColumnB] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT MAX(`ColumnA`) AS `max`, `ColumnB` FROM `Table`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT MAX(\"COLUMNA\") AS \"MAX\", \"COLUMNB\" FROM \"TABLE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT MAX(\"ColumnA\") AS \"max\", \"ColumnB\" FROM \"Table\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void AggregatesCanHaveALimit()
        {
            var query = new Query()
                .SelectMin("ColumnA", "MinValue")
                .SelectMax("ColumnB", "MaxValue")
                .From("Table")
                .Limit(100)
                ;

            var c = Compile(query);

            Assert.Equal("SELECT TOP (100) MIN([ColumnA]) AS [MinValue], MAX([ColumnB]) AS [MaxValue] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT FIRST 100 MIN(\"COLUMNA\") AS \"MINVALUE\", MAX(\"COLUMNB\") AS \"MAXVALUE\" FROM \"TABLE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT MIN(`ColumnA`) AS `MinValue`, MAX(`ColumnB`) AS `MaxValue` FROM `Table` LIMIT 100", c[EngineCodes.MySql]);
        }

        [Fact]
        public void AggregatesCanHaveAnOrderBy()
        {
            var query = new Query()
                .SelectMin("ColumnA", "MinValue")
                .SelectMax("ColumnB", "MaxValue")
                .From("Table")
                .OrderBy("MinValue")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MIN([ColumnA]) AS [MinValue], MAX([ColumnB]) AS [MaxValue] FROM [Table] ORDER BY [MinValue]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT MIN(\"COLUMNA\") AS \"MINVALUE\", MAX(\"COLUMNB\") AS \"MAXVALUE\" FROM \"TABLE\" ORDER BY \"MINVALUE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT MIN(`ColumnA`) AS `MinValue`, MAX(`ColumnB`) AS `MaxValue` FROM `Table` ORDER BY `MinValue`", c[EngineCodes.MySql]);
        }

        [Fact]
        public void AggregatesCanHaveAGroupBy()
        {
            var query = new Query()
                .SelectMin("ColumnA", "MinValue")
                .SelectMax("ColumnB", "MaxValue")
                .From("Table")
                .GroupBy("MinValue")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MIN([ColumnA]) AS [MinValue], MAX([ColumnB]) AS [MaxValue] FROM [Table] GROUP BY [MinValue]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT MIN(\"COLUMNA\") AS \"MINVALUE\", MAX(\"COLUMNB\") AS \"MAXVALUE\" FROM \"TABLE\" GROUP BY \"MINVALUE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT MIN(`ColumnA`) AS `MinValue`, MAX(`ColumnB`) AS `MaxValue` FROM `Table` GROUP BY `MinValue`", c[EngineCodes.MySql]);
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
