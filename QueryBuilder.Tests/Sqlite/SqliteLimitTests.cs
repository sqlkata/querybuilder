using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Sqlite
{
    public class SqliteLimitTests : TestSupport
    {
        private readonly SqliteCompiler compiler;

        public SqliteLimitTests()
        {
            compiler = Compilers.Get<SqliteCompiler>(EngineCodes.Sqlite);
        }

        [Fact]
        public void WithNoLimitNorOffset()
        {
            Query query = new Query("Table");
            SqlResult context = new SqlResult { Query = query };

            Assert.Null(compiler.CompileLimit(context));
        }

        [Fact]
        public void WithNoOffset()
        {
            Query query = new Query("Table").Limit(10);
            SqlResult context = new SqlResult { Query = query };

            Assert.Equal("LIMIT ?", compiler.CompileLimit(context));
            Assert.Equal(10, context.Bindings[0]);
        }

        [Fact]
        public void WithNoLimit()
        {
            Query query = new Query("Table").Offset(20);
            SqlResult context = new SqlResult { Query = query };

            Assert.Equal("LIMIT -1 OFFSET ?", compiler.CompileLimit(context));
            Assert.Equal(20, context.Bindings[0]);
            Assert.Single(context.Bindings);
        }

        [Fact]
        public void WithLimitAndOffset()
        {
            Query query = new Query("Table").Limit(5).Offset(20);
            SqlResult context = new SqlResult { Query = query };

            Assert.Equal("LIMIT ? OFFSET ?", compiler.CompileLimit(context));
            Assert.Equal(5, context.Bindings[0]);
            Assert.Equal(20, context.Bindings[1]);
            Assert.Equal(2, context.Bindings.Count);
        }
    }
}