using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SqlKata.Tests
{
    public partial class QueryBuilderTest
    {
        [Fact]
        public void Count()
        {
            var query = new Query("A").AsCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [A]", c[0]);
            Assert.Equal("SELECT COUNT(*) AS `count` FROM `A`", c[1]);
            Assert.Equal("SELECT COUNT(*) AS \"count\" FROM \"A\"", c[2]);
            Assert.Equal("SELECT COUNT(*) AS \"COUNT\" FROM \"A\"", c[3]);
        }

        [Fact]
        public void CountMultipleColumns()
        {
            var query = new Query("A").AsCount("ColumnA", "ColumnB");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [countQuery]", c[0]);
        }

        [Fact]
        public void DistinctCount()
        {
            var query = new Query("A").Distinct().AsCount();

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT * FROM [A]) AS [countQuery]", c[0]);
        }

        [Fact]
        public void DistinctCountMultipleColumns()
        {
            var query = new Query("A").Distinct().AsCount("ColumnA", "ColumnB");

            var c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [ColumnA], [ColumnB] FROM [A]) AS [countQuery]", c[0]);
        }

        [Fact]
        public void Average()
        {
            var query = new Query("A").AsAverage("TTL");

            var c = Compile(query);

            Assert.Equal("SELECT AVG([TTL]) AS [avg] FROM [A]", c[0]);
        }

        [Fact]
        public void Sum()
        {
            var query = new Query("A").AsSum("PacketsDropped");

            var c = Compile(query);

            Assert.Equal("SELECT SUM([PacketsDropped]) AS [sum] FROM [A]", c[0]);
        }

        [Fact]
        public void Max()
        {
            var query = new Query("A").AsMax("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MAX([LatencyMs]) AS [max] FROM [A]", c[0]);
        }

        [Fact]
        public void Min()
        {
            var query = new Query("A").AsMin("LatencyMs");

            var c = Compile(query);

            Assert.Equal("SELECT MIN([LatencyMs]) AS [min] FROM [A]", c[0]);
        }
    }
}
