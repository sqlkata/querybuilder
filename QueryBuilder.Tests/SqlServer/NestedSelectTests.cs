using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.SqlServer
{
    public class NestedSelectTests : TestSupport
    {
        private readonly SqlServerCompiler compiler;

        public NestedSelectTests()
        {
            compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
        }

        [Fact]
        public void Compile_RawSql_WithLimit_ReturnsCorrectQuery()
        {
            Query q = new Query().From("Foo as src").Limit(1);

            string actual = compiler.Compile(q).ToString();
            Assert.Contains("SELECT TOP (1) * FROM [Foo]", actual);
        }

        [Fact]
        public void SqlCompile_QueryAadNestedLimit_ReturnsQueryWithTop()
        {
            Query q = new Query().From("Foo as src").Select("MyData");
            Query n = new Query().From("Bar").Limit(1).Select("MyData");
            q.Select(n, "Bar");

            string actual = compiler.Compile(q).ToString();
            Assert.Contains("SELECT TOP (1) [MyData] FROM [Bar]", actual);
            Assert.Contains("SELECT [MyData], (SELECT TOP (1) [MyData] FROM [Bar]) AS [Bar] FROM [Foo] AS [src]",
                actual);
        }

        [Fact]
        public void SqlCompile_QueryLimitAndNestedLimit_ReturnsQueryWithTop()
        {
            Query q = new Query().From("Foo as src").Limit(1).Select("MyData");
            Query n = new Query().From("Bar").Limit(1).Select("MyData");
            q.Select(n, "Bar");


            string actual = compiler.Compile(q).ToString();
            Assert.Contains(
                "SELECT TOP (1) [MyData], (SELECT TOP (1) [MyData] FROM [Bar]) AS [Bar] FROM [Foo] AS [src]", actual);
        }

        [Fact]
        public void SqlCompile_QueryLimitAndNestedLimit_BindingValue()
        {
            Query n = new Query().From("Bar");
            Query q = new Query().From("Foo").Where("x", true).WhereNotExists(n);
            // var q = new Query().From("Foo").Where("C", "c").WhereExists(n).Where("A", "a");

            string actual = compiler.Compile(q).ToString();
            Assert.Contains("SELECT * FROM [Foo] WHERE [x] = true AND NOT EXISTS (SELECT 1 FROM [Bar])",
                actual);
            // Assert.Contains("SELECT * FROM [Foo] WHERE [C] = 'c' AND EXISTS (SELECT TOP (1) 1 FROM [Bar]) AND [A] = 'a'", actual);
        }
    }
}