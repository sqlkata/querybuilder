using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Oracle
{
    public class OracleInsertTests : TestSupport
    {
        protected readonly OracleCompiler Compiler;
        public OracleInsertTests()
        {
            Compiler = Compilers.Get<OracleCompiler>(EngineCodes.Oracle);
        }

        [Fact]
        public void CanCompileValidMultipleInsert()
        {
            var insertColumns = new[]
            {
                "one",
                "two"
            };

            var insertValues = new[]
            {
                new object[] {3348, 1},
                new object[] {3348, 2},
                new object[] {3348, 3}
            };

            var query = new Query("TBL").AsInsert(insertColumns, insertValues);
            var result = Compiler.Compile(query);

            Assert.Equal(
                "INSERT INTO \"TBL\" (\"one\", \"two\") SELECT 3348, 1 FROM dual UNION ALL SELECT 3348, 2 FROM dual UNION ALL SELECT 3348, 3 FROM dual",
                result.ToString());
        }
    }
}