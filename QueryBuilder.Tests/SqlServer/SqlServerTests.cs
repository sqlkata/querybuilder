using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.SqlServer
{
    public class SqlServerTests : TestSupport
    {
        private readonly SqlServerCompiler compiler;

        public SqlServerTests()
        {
            compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
        }


        [Fact]
        public void SqlServerTop()
        {
            var query = new Query("table").Limit(1);
            var result = compiler.Compile(query);
            Assert.Equal("SELECT TOP (@p0) * FROM [table]", result.Sql);
        }


        [Fact]
        public void SqlServerSelectWithParameterPlaceHolder()
        {
            var query = new Query("table").Select("Column\\?");
            var result = compiler.Compile(query);
            Assert.Equal("SELECT [Column\\?] FROM [table]", result.Sql);
        }

        [Fact]
        public void SqlServerTopWithDistinct()
        {
            var query = new Query("table").Limit(1).Distinct();
            var result = compiler.Compile(query);
            Assert.Equal("SELECT DISTINCT TOP (@p0) * FROM [table]", result.Sql);
        }


        [Theory()]
        [InlineData(-100)]
        [InlineData(0)]
        public void OffsetSqlServer_Should_Be_Ignored_If_Zero_Or_Negative(int offset)
        {
            var q = new Query().From("users").Offset(offset);
            var c = Compilers.CompileFor(EngineCodes.SqlServer, q);

            Assert.Equal("SELECT * FROM [users]", c.ToString());
        }

        [Fact]
        public void SqlServerSelectWithParameterPlaceHolderEscaped()
        {
            var query = new Query("table").Select("Column\\?");
            var result = compiler.Compile(query);
            Assert.Equal("SELECT [Column?] FROM [table]", result.ToString());
        }

        [Theory()]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(1000000)]
        public void OffsetSqlServer_Should_Be_Incremented_By_One(int offset)
        {
            var q = new Query().From("users").Offset(offset);
            var c = Compilers.CompileFor(EngineCodes.SqlServer, q);
            Assert.Equal(
                "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= " +
                (offset + 1), c.ToString());
        }

        [Fact]
        public void SqlServerOrderByTimePart()
        {
            var query = new Query("table").OrderByDatePart("year","field");
            var result = compiler.Compile(query);
            Assert.Equal("SELECT * FROM [table] ORDER BY DATEPART(YEAR, [field])", result.Sql);
        }
        [Fact]
        public void SqlServerGroupByTimePart()
        {
            var query = new Query("table").GroupByDatePart("year", "field").SelectDatePart("year","field","meow");
            var result = compiler.Compile(query);
            Assert.Equal("SELECT DATEPART(YEAR, [field]) AS [meow] FROM [table] GROUP BY DATEPART(YEAR, [field])", result.Sql);
        }
    }
}
