using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests.SqlServer
{
    public class SqlServerTests : TestSupport
    {
        private readonly Compiler compiler;

        public SqlServerTests()
        {
            compiler = CreateCompiler(EngineCodes.SqlServer);
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


        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public void OffsetSqlServer_Should_Be_Ignored_If_Zero_Or_Negative(int offset)
        {
            var query = new Query().From("users").Offset(offset);

            var result = compiler.Compile(query);

            Assert.Equal("SELECT * FROM [users]", result.ToString());
        }

        [Fact]
        public void SqlServerSelectWithParameterPlaceHolderEscaped()
        {
            var query = new Query("table").Select("Column\\?");

            var result = compiler.Compile(query);

            Assert.Equal("SELECT [Column?] FROM [table]", result.ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(1000000)]
        public void OffsetSqlServer_Should_Be_Incremented_By_One(int offset)
        {
            var query = new Query().From("users").Offset(offset);

            var c = compiler.Compile(query);

            Assert.Equal(
                "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= " +
                (offset + 1), c.ToString());
        }
    }
}
