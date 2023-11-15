using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Sqlite
{
    public class SqlliteJoinTests : TestSupport
    {
        private readonly SqliteCompiler compiler;

        public SqlliteJoinTests()
        {
            compiler = Compilers.Get<SqliteCompiler>(EngineCodes.Sqlite);
        }

        [Fact]
        public void Join()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN \"TableA\" ON \"Column1\" = \"ColumnA\"", compiler.CompileJoins(ctx));
        }


        [Fact]
        public void JoinWithIndexHint()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA", indexHint: "index1");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN \"TableA\" ON \"Column1\" = \"ColumnA\"", compiler.CompileJoins(ctx));
        }
    }
}
