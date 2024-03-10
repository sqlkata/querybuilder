using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Firebird
{
    public class FirebirdJoinTests : TestSupport
    {
        private readonly FirebirdCompiler compiler;

        public FirebirdJoinTests()
        {
            compiler = Compilers.Get<FirebirdCompiler>(EngineCodes.Firebird);
        }

        [Fact]
        public void Join()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN \"TABLEA\" ON \"COLUMN1\" = \"COLUMNA\"", compiler.CompileJoins(ctx));
        }


        [Fact]
        public void JoinWithIndexHint()
        {
            var query = new Query("Table").Join("TableA", "Column1", "ColumnA", indexHint: "index1");
            var ctx = new SqlResult { Query = query };

            Assert.Equal("\nINNER JOIN \"TABLEA\" ON \"COLUMN1\" = \"COLUMNA\"", compiler.CompileJoins(ctx));
        }
    }
}
