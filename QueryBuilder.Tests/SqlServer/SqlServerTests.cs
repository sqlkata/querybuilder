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
            Query query = new Query("table").Limit(1);
            SqlResult result = compiler.Compile(query);
            Assert.Equal("SELECT TOP (@p0) * FROM [table]", result.Sql);
        }

        [Fact]
        public void SqlServerTopWithDistinct()
        {
            Query query = new Query("table").Limit(1).Distinct();
            SqlResult result = compiler.Compile(query);
            Assert.Equal("SELECT DISTINCT TOP (@p0) * FROM [table]", result.Sql);
        }


        [Theory()]
        [InlineData(-100)]
        [InlineData(0)]
        public void OffsetSqlServer_Should_Be_Ignored_If_Zero_Or_Negative(int offset)
        {
            Query query = new Query().From("users").Offset(offset);
            SqlResult result = Compilers.CompileFor(EngineCodes.SqlServer, query);

            Assert.Equal("SELECT * FROM [users]", result.ToString());
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
            Query query = new Query().From("users").Offset(offset);
            SqlResult result = Compilers.CompileFor(EngineCodes.SqlServer, query);
            Assert.Equal(
                "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [users]) AS [results_wrapper] WHERE [row_num] >= " +
                (offset + 1), result.ToString());
        }
    }
}