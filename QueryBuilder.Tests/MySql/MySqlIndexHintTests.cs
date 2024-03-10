using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.MySql
{
    public class MySqlIndexHintTests : TestSupport
    {
        private readonly MySqlCompiler compiler;

        public MySqlIndexHintTests()
        {
            compiler = Compilers.Get<MySqlCompiler>(EngineCodes.MySql);
        }

        [Fact]
        public void WithNoIndexHint()
        {
            var query = new Query("Table");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("FROM `Table`", compiler.CompileFrom(ctx));
        }

        [Fact]
        public void WithIndexHint()
        {
            var query = new Query("Table", indexHint: "index1");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("FROM `Table` USE INDEX(index1)", compiler.CompileFrom(ctx));
        }

        [Fact]
        public void JoinWithNoIndexHint()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN `TableA` ON `Column1` = `ColumnA`", compiler.CompileJoins(ctx));
        }


        [Fact]
        public void JoinWithIndexHint()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA", indexHint: "index1");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN `TableA` USE INDEX(index1) ON `Column1` = `ColumnA`", compiler.CompileJoins(ctx));
        }
    }
}
