using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace SqlKata.Tests
{
    public class AggregateTests : TestSupport
    {
        [Fact]
        public void SelectAggregateEmpty()
        {
            Assert.Throws<ArgumentException>(() => new Query("A").SelectAggregate("aggregate", new string[] { }, AbstractAggregateColumn.AggregateDistinct.aggregateNonDistinct));
        }

        [Fact]
        public void SelectAggregate()
        {
            var query = new Query("A").SelectAggregate("aggregate", new[] { "Column" }, AbstractAggregateColumn.AggregateDistinct.aggregateNonDistinct);

            var c = Compile(query);

            Assert.Equal("SELECT AGGREGATE([Column]) AS [aggregate] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT AGGREGATE(`Column`) AS `aggregate` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT AGGREGATE(\"Column\") AS \"aggregate\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT AGGREGATE(\"COLUMN\") AS \"AGGREGATE\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void SelectAggregateAlias()
        {
            var query = new Query("A").SelectAggregate("aggregate", new[] { "Column" }, AbstractAggregateColumn.AggregateDistinct.aggregateNonDistinct, "Alias");

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
                new Query("A").SelectAggregate("aggregate", new[] { "Column1", "Column2" }, AbstractAggregateColumn.AggregateDistinct.aggregateNonDistinct)
            );
        }

        [Fact]
        public void SelectAggregateMultipleColumnsAlias()
        {
            Assert.Throws<ArgumentException>(() =>
                new Query("A").SelectAggregate("aggregate", new[] { "Column1", "Column2" }, AbstractAggregateColumn.AggregateDistinct.aggregateNonDistinct, "Alias")
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
        public void CountDistinct() // Different from DistinctCount()
        {
            var query = new Query()
                .SelectCountDistinct("Column")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(DISTINCT [Column]) AS [count] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(DISTINCT `Column`) AS `count` FROM `Table`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(DISTINCT \"COLUMN\") AS \"COUNT\" FROM \"TABLE\"", c[EngineCodes.Firebird]);
            Assert.Equal("SELECT COUNT(DISTINCT \"Column\") AS \"count\" FROM \"Table\"", c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void DistinctCountDistinct()
        {
            var query = new Query()
                .Distinct()
                .SelectCountDistinct("Column")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [Column] FROM [Table]) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCountDistinctMultipleCounts()
        {
            // Cannot add more than one top-level aggregate clause:
            // Because the query itself is SELECT DISTINCT, a COUNT() will
            // be compiled to a sub-query (see DistinctCountDistinct() test).
            // This can only be done once, as we would need to generate multiple
            // sub-queries other wise.
            // Idea: this might still be possible to emulate in some cases (i.e.
            // when not already having a JOIN using several WITH clauses which
            // SELECT ROW_NUMBER based on the conditions from the original
            // query).
            Assert.Throws<InvalidOperationException>(() =>
                new Query()
                    .Distinct()
                    .SelectCountDistinct("ColumnA")
                    .SelectCountDistinct("ColumnB")
                    .From("Table")
            );
        }

        [Fact]
        public void DistinctCountMultipleColumns()
        {
            var query = new Query("A").Distinct().SelectCount(new[] { "ColumnA", "ColumnB" });

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [ColumnA], [ColumnB] FROM [A]) AS [CountQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void CountDistinctMultipleColumns()
        {
            Assert.Throws<NotImplementedException>(() =>
                new Query("A").SelectCountDistinct(new[] { "ColumnA", "ColumnB" })
            );
        }

        [Fact]
        public void DistinctCountDistinctMultipleColumns()
        {
            Assert.Throws<NotImplementedException>(() =>
                new Query("A").Distinct().SelectCountDistinct(new[] { "ColumnA", "ColumnB" })
            );
        }

        [Fact]
        public void DistinctMax()
        {
            var query = new Query()
                .Distinct()
                .SelectMax("Column")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT DISTINCT MAX([Column]) AS [max] FROM [Table]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void MaxDistinct() // Different from DistinctCount()
        {
            var query = new Query()
                .SelectMaxDistinct("Column")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MAX(DISTINCT [Column]) AS [max] FROM [Table]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctMaxDistinct()
        {
            var query = new Query()
                .Distinct()
                .SelectMaxDistinct("Column")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT DISTINCT MAX(DISTINCT [Column]) AS [max] FROM [Table]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void AnyValue()
        {
            var query = new Query()
                .SelectAnyValue("Column")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MIN([Column]) AS [any_value] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COALESCE(NULL, \"Column\") AS \"any_value\" FROM \"Table\" GROUP BY \"\"", c[EngineCodes.Sqlite]);
            Assert.Equal("SELECT ANY_VALUE(\"Column\") AS \"any_value\" FROM \"Table\"", c[EngineCodes.Snowflake]);
        }

        [Fact]
        public void AnyValueWithAlias()
        {
            var query = new Query()
                .SelectAnyValue("Column", "Alias")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MIN([Column]) AS [Alias] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COALESCE(NULL, \"Column\") AS \"Alias\" FROM \"Table\" GROUP BY \"\"", c[EngineCodes.Sqlite]);
            Assert.Equal("SELECT ANY_VALUE(\"Column\") AS \"Alias\" FROM \"Table\"", c[EngineCodes.Snowflake]);
        }

        [Fact]
        public void AnyValueDistinct()
        {
            var query = new Query()
                .SelectAnyValueDistinct("Column")
                .From("Table")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MIN(DISTINCT [Column]) AS [any_value] FROM [Table]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COALESCE(NULL, \"Column\") AS \"any_value\" FROM \"Table\" GROUP BY \"\"", c[EngineCodes.Sqlite]);
            Assert.Equal("SELECT ANY_VALUE(DISTINCT \"Column\") AS \"any_value\" FROM \"Table\"", c[EngineCodes.Snowflake]);
        }

        [Fact]
        public void AnyValueWithGroupBy()
        {
            var query = new Query()
                .SelectAnyValue("Column")
                .From("Table")
                .GroupBy("ColumnB")
                ;

            var c = Compile(query);

            Assert.Equal("SELECT MIN([Column]) AS [any_value] FROM [Table] GROUP BY [ColumnB]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COALESCE(NULL, \"Column\") AS \"any_value\" FROM \"Table\" GROUP BY \"ColumnB\"", c[EngineCodes.Sqlite]);
            Assert.Equal("SELECT ANY_VALUE(\"Column\") AS \"any_value\" FROM \"Table\" GROUP BY \"ColumnB\"", c[EngineCodes.Snowflake]);
        }

        [Fact]
        public void AnyValueComplexExample()
        {
            var query = new Query()
                .Select("customer.id")
                .SelectAnyValue("customer.name")
                .SelectSum("orders.value")
                .From("customer")
                .Join("orders", "customer.id", "orders.customer_id")
                .GroupBy("customer.id")
                ;

            var c = Compile(query);

            /**
             * SELECT customer.id , ANY_VALUE(customer.name) , SUM(orders.value)
             * FROM customer
             * JOIN orders ON customer.id = orders.customer_id
             * GROUP BY customer.id;
             *
             * From https://docs.snowflake.com/en/sql-reference/functions/any_value.html
             */
            Assert.Equal("SELECT \"customer\".\"id\", ANY_VALUE(\"customer\".\"name\") AS \"any_value\", SUM(\"orders\".\"value\") AS \"sum\" FROM \"customer\" \nINNER JOIN \"orders\" ON \"customer\".\"id\" = \"orders\".\"customer_id\" GROUP BY \"customer\".\"id\"", c[EngineCodes.Snowflake]);
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

        [Fact]
        public void SelectPercentileApproxGeneralSupport()
        {
            var query = new Query()
                .SelectPercentileApprox(0.9, "column")
                .From("table")
            ;

            // The approximate percentile is not supported by all compilers
            Assert.Throws<NotSupportedException>(() => Compile(query));
        }

        [Fact]
        public void SelectPercentileApprox()
        {
            var query = new Query()
                .SelectPercentileApprox(0.9, "column")
                .From("table")
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                            PERCENTILE_CONT(0.9) WITHIN GROUP(ORDER BY [column]) OVER() AS [value_0]
                        FROM
                            [table]
                    )
                SELECT
                    MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]) AS [percentileapprox]
                FROM
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox]
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                SELECT
                    APPROX_PERCENTILE(""column"", 0.9) AS ""percentileapprox""
                FROM
                    ""table""
            ");
        }

        [Fact]
        public void SelectPercentileApproxAlias()
        {
            var query = new Query()
                .SelectPercentileApprox(0.9, "column", "Alias")
                .From("table")
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                            PERCENTILE_CONT(0.9) WITHIN GROUP(ORDER BY [column]) OVER() AS [value_0]
                        FROM
                            [table]
                    )
                SELECT
                    MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]) AS [Alias]
                FROM
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox]
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                SELECT
                    APPROX_PERCENTILE(""column"", 0.9) AS ""Alias""
                FROM
                    ""table""
            ");
        }

        [Fact]
        public void SelectPercentileApproxMultiple()
        {
            // Note: expressly using the same column w/ different percentile.
            // There is a potential performance improvement here for snowflake
            // using APPROX_PERCENTILE_ESTIMATE (not implemented).
            var query = new Query()
                .SelectPercentileApprox(0.1, "c1")
                .SelectPercentileApprox(0.9, "c1")
                .From("table")
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                              PERCENTILE_CONT(0.1) WITHIN GROUP(ORDER BY [c1]) OVER() AS [value_0]
                            , PERCENTILE_CONT(0.9) WITHIN GROUP(ORDER BY [c1]) OVER() AS [value_1]
                        FROM
                            [table]
                    )
                SELECT
                      MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]) AS [percentileapprox]
                    , MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_1]) AS [percentileapprox]
                FROM
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox]
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                SELECT
                      APPROX_PERCENTILE(""c1"", 0.1) AS ""percentileapprox""
                    , APPROX_PERCENTILE(""c1"", 0.9) AS ""percentileapprox""
                FROM
                    ""table""
            ");
        }

        [Fact]
        public void SelectPercentileApproxRegularColumns()
        {
            var query = new Query()
                .Select("c1")
                .SelectAs(("c2", "Alias"))
                .SelectPercentileApprox(0.9, "c3")
                .From("table")
                .GroupBy("c1")
                .GroupBy("c2")
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                              [c1] AS [value_0]
                            , [c2] AS [value_1]
                            , PERCENTILE_CONT(0.9) WITHIN GROUP(ORDER BY [c3]) OVER(PARTITION BY [c1], [c2]) AS [value_2]
                        FROM
                            [table]
                    )
                SELECT
                      [__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0] AS [c1]
                    , [__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_1] AS [Alias]
                    , MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_2]) AS [percentileapprox]
                FROM
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox]
                GROUP BY
                      [__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]
                    , [__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_1]
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                SELECT
                      ""c1""
                    , ""c2"" AS ""Alias""
                    , APPROX_PERCENTILE(""c3"", 0.9) AS ""percentileapprox""
                FROM
                    ""table""
                GROUP BY
                      ""c1""
                    , ""c2""
            ");
        }

        [Fact]
        public void SelectPercentileApproxAggregateColumns()
        {
            var query = new Query()
                .SelectAvg("c1")
                .SelectPercentileApprox(0.9, "c2")
                .From("table")
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                              [c1] AS [value_0]
                            , PERCENTILE_CONT(0.9) WITHIN GROUP(ORDER BY [c2]) OVER() AS [value_1]
                        FROM
                            [table]
                    )
                SELECT
                      AVG([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]) AS [avg]
                    , MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_1]) AS [percentileapprox]
                FROM
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox]
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                SELECT
                      AVG(""c1"") AS ""avg""
                    , APPROX_PERCENTILE(""c2"", 0.9) AS ""percentileapprox""
                FROM
                    ""table""
            ");
        }

        [Fact]
        public void SelectPercentileGroupByOrderBy()
        {
            var query = new Query()
                .Select("c1")
                .SelectPercentileApprox(0.9, "c2")
                .From("table")
                .GroupBy("c1")
                .OrderByDesc("c1")
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                              [c1] AS [value_0]
                            , PERCENTILE_CONT(0.9) WITHIN GROUP(ORDER BY [c2]) OVER(PARTITION BY [c1]) AS [value_1]
                        FROM
                            [table]
                    )
                SELECT
                      [__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0] AS [c1]
                    , MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_1]) AS [percentileapprox]
                FROM
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox]
                GROUP BY
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]
                ORDER BY
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0] DESC
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                SELECT
                      ""c1""
                    , APPROX_PERCENTILE(""c2"", 0.9) AS ""percentileapprox""
                FROM
                    ""table""
                GROUP BY
                    ""c1""
                ORDER BY
                    ""c1"" DESC
            ");
        }

        [Fact]
        public void SelectPercentileApproxInSubquery()
        {
            var query = new Query()
                .With("SubQuery1", q => q
                    .SelectAs(("Column", "Alias"))
                    .From("Table")
                )
                .With("SubQuery2", q => q
                    .SelectPercentileApprox(0.75, "Alias")
                    .From("SubQuery1")
                )
                .Select("percentileapprox")
                .From("SubQuery2")
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                      [SubQuery1] AS (
                        SELECT
                            [Column] AS [Alias]
                        FROM
                            [Table]
                    )
                    , [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                            PERCENTILE_CONT(0.75) WITHIN GROUP(ORDER BY [Alias]) OVER() AS [value_0]
                        FROM
                            [SubQuery1]
                    )
                    , [SubQuery2] AS (
                        SELECT
                            MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]) AS [percentileapprox]
                        FROM
                            [__generated__SqlKata_SqlServerCompiler_percentileapprox]
                    )
                SELECT
                    [percentileapprox]
                FROM
                    [SubQuery2]
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                WITH
                      ""SubQuery1"" AS (
                        SELECT
                            ""Column"" AS ""Alias""
                        FROM
                            ""Table""
                    )
                    , ""SubQuery2"" AS (
                        SELECT
                            APPROX_PERCENTILE(""Alias"", 0.75) AS ""percentileapprox""
                        FROM
                            ""SubQuery1""
                    )
                SELECT
                    ""percentileapprox""
                FROM
                    ""SubQuery2""
            ");
        }

        [Fact]
        public void SelectPercentileApproxComplex()
        {
            var query = new Query()
                .With("filter", q => q
                    .SelectAs(("PurchaseOrderItems.ItemID", "CaseId"))
                    .From("PurchaseOrderItems")
                    .WhereIn("PurchaseOrderItems.Plant", new List<string> { "dummy" })
                )
                .SelectMin("PurchaseOrderItems.ValueOrdered", "MinValue")
                .SelectMax("PurchaseOrderItems.ValueOrdered", "MaxValue")
                .SelectPercentileApprox(0.03, "PurchaseOrderItems.ValueOrdered", "LowPercentile")
                .SelectPercentileApprox(0.97, "PurchaseOrderItems.ValueOrdered", "HighPercentile")
                .From("PurchaseOrderItems")
                .Join("filter", "filter.CaseId", "PurchaseOrderItems.ItemID")
                .Limit(1)
            ;

            CheckCompileResult(query, EngineCodes.SqlServer, @"
                WITH
                      [filter] AS (
                        SELECT
                            [PurchaseOrderItems].[ItemID] AS [CaseId]
                        FROM
                            [PurchaseOrderItems]
                        WHERE
                            [PurchaseOrderItems].[Plant] IN (?)
                    )
                    , [__generated__SqlKata_SqlServerCompiler_percentileapprox] AS (
                        SELECT
                              [PurchaseOrderItems].[ValueOrdered] AS [value_0]
                            , [PurchaseOrderItems].[ValueOrdered] AS [value_1]
                            , PERCENTILE_CONT(0.03) WITHIN GROUP(ORDER BY [PurchaseOrderItems].[ValueOrdered]) OVER() AS [value_2]
                            , PERCENTILE_CONT(0.97) WITHIN GROUP(ORDER BY [PurchaseOrderItems].[ValueOrdered]) OVER() AS [value_3]
                        FROM
                            [PurchaseOrderItems]
                            INNER JOIN [filter] ON [filter].[CaseId] = [PurchaseOrderItems].[ItemID]
                    )
                SELECT TOP (?)
                      MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_0]) AS [MinValue]
                    , MAX([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_1]) AS [MaxValue]
                    , MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_2]) AS [LowPercentile]
                    , MIN([__generated__SqlKata_SqlServerCompiler_percentileapprox].[value_3]) AS [HighPercentile]
                FROM
                    [__generated__SqlKata_SqlServerCompiler_percentileapprox]
            ");

            CheckCompileResult(query, EngineCodes.Snowflake, @"
                WITH
                      ""filter"" AS (
                        SELECT
                            ""PurchaseOrderItems"".""ItemID"" AS ""CaseId""
                        FROM
                            ""PurchaseOrderItems""
                        WHERE
                            ""PurchaseOrderItems"".""Plant"" IN (?)
                    )
                SELECT
                      MIN(""PurchaseOrderItems"".""ValueOrdered"") AS ""MinValue""
                    , MAX(""PurchaseOrderItems"".""ValueOrdered"") AS ""MaxValue""
                    , APPROX_PERCENTILE(""PurchaseOrderItems"".""ValueOrdered"", 0.03) AS ""LowPercentile""
                    , APPROX_PERCENTILE(""PurchaseOrderItems"".""ValueOrdered"", 0.97) AS ""HighPercentile""
                FROM
                    ""PurchaseOrderItems""
                    INNER JOIN ""filter"" ON ""filter"".""CaseId"" = ""PurchaseOrderItems"".""ItemID""
                LIMIT ?
            ");
        }
    }
}
