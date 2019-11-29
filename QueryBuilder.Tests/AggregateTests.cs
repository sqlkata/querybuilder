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
            Query query = new Query("A").AsCount();

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM [A]", c[EngineCodes.SqlServer]);
            Assert.Equal("SELECT COUNT(*) AS `count` FROM `A`", c[EngineCodes.MySql]);
            Assert.Equal("SELECT COUNT(*) AS \"count\" FROM \"A\"", c[EngineCodes.PostgreSql]);
            Assert.Equal("SELECT COUNT(*) AS \"COUNT\" FROM \"A\"", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void CountMultipleColumns()
        {
            Query query = new Query("A").AsCount("ColumnA", "ColumnB");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT 1 FROM [A] WHERE [ColumnA] IS NOT NULL AND [ColumnB] IS NOT NULL) AS [countQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCount()
        {
            Query query = new Query("A").Distinct().AsCount();

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT * FROM [A]) AS [countQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void DistinctCountMultipleColumns()
        {
            Query query = new Query("A").Distinct().AsCount("ColumnA", "ColumnB");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT COUNT(*) AS [count] FROM (SELECT DISTINCT [ColumnA], [ColumnB] FROM [A]) AS [countQuery]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Average()
        {
            Query query = new Query("A").AsAverage("TTL");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT AVG([TTL]) AS [avg] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Sum()
        {
            Query query = new Query("A").AsSum("PacketsDropped");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT SUM([PacketsDropped]) AS [sum] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Max()
        {
            Query query = new Query("A").AsMax("LatencyMs");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT MAX([LatencyMs]) AS [max] FROM [A]", c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void Min()
        {
            Query query = new Query("A").AsMin("LatencyMs");

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("SELECT MIN([LatencyMs]) AS [min] FROM [A]", c[EngineCodes.SqlServer]);
        }
    }
}
