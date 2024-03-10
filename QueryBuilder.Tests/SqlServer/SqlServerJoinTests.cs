using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.SqlServer
{
    public class SqlServerJoinTests : TestSupport
    {
        private readonly SqlServerCompiler compiler;

        public SqlServerJoinTests()
        {
            compiler = Compilers.Get<SqlServerCompiler>(EngineCodes.SqlServer);
        }

        [Fact]
        public void Join()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN [TableA] ON [Column1] = [ColumnA]", compiler.CompileJoins(ctx));
        }


        [Fact]
        public void JoinWithIndexHint()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA", indexHint: "index1");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN [TableA] ON [Column1] = [ColumnA]", compiler.CompileJoins(ctx));
        }
    }
}
