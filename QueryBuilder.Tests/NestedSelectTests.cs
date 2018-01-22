using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public class NestedSelectTests
    {

        
        [Fact]
        public static void Compile_RawSql_WithLimit_ReturnsCorrectQuery()
        {
            var q = new Query().From("Foo as src");
            var n = new Query();
            n.From("Bar").Limit(1);
            n.Select("MyData");
            q.Select(n, "Bar");

            var target = new SqlServerCompiler();

            var actual = target.Compile(q).RawSql;
            Assert.Contains(actual, "(SELECT TOP(1) [MyData] FROM BAR)");
            Assert.DoesNotContain(actual,"(SELECT , [MyData] FROM [Bar])");
        }
    }
}