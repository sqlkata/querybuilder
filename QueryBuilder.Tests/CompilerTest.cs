using SqlKata;
using SqlKata.Compilers;
using Xunit;

namespace QueryBuilder.Tests
{
    public class CompilerTest
    {
        private readonly MySqlCompiler mysql;
        private readonly Compiler pgsql;
        private SqlServerCompiler mssql { get; }

        public CompilerTest()
        {
            mssql = new SqlServerCompiler();
            mysql = new MySqlCompiler();
            pgsql = new PostgresCompiler();
        }

        private string[] Compile(Query q)
        {
            return new[]
            {
                mssql.Compile(q.Clone()).ToString(),
                mysql.Compile(q.Clone()).ToString(),
                pgsql.Compile(q.Clone()).ToString()
            };
        }

        [Fact]
        public void Should_clear_query_parameters_after_compilation()
        {
            var laptops = new Query("Laptops").Where("Price", ">", 1000).ForPage(3);
            var laptops2 = new Query("Laptops").Where("Price", ">", 1000).ForPage(4);

            Compile(laptops);
            Compile(laptops2);

            Assert.Empty(pgsql.bindings);
            Assert.Empty(mysql.bindings);
            Assert.Empty(mssql.bindings);
        }
    }
}