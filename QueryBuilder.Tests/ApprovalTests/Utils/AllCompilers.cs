using System.Text;
using SqlKata.Compilers;

namespace SqlKata.Tests.ApprovalTests.Utils
{
    public static class MyVerify
    {
        public static Task Verify(this Query query,
            Compiler compiler)
        {
            var sqlResult = compiler.Compile(query);
            var sb = new StringBuilder();
            sb.AppendLine("-------- ORIGINAL -----------");
            sb.Append(sqlResult);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("----------- RAW -------------");
            sb.Append(sqlResult.RawSql);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("--------PARAMETRIZED --------");
            sb.Append(sqlResult.Sql);

            var compilerName = compiler.GetType().Name;
            if (compiler is SqlServerCompiler { UseLegacyPagination: true })
                compilerName += " with LegacyPagination";
            if (!compiler.OmitSelectInsideExists )
                compilerName += " with SelectInsideExists";
            return Verifier.Verify(sb.ToString(), "sql")
                .UseTextForParameters(compilerName)
                .UseDirectory("../Output");
        }
    }
    public class AllCompilers : List<object[]>
    {
        public AllCompilers()
        {
            Add(new object[] { new FirebirdCompiler() });
            Add(new object[] { new Compiler() });
            Add(new object[] { new Compiler {OmitSelectInsideExists = false}});
            Add(new object[] { new MySqlCompiler() });
            Add(new object[] { new OracleCompiler() });
            Add(new object[] { new PostgresCompiler() });
            Add(new object[] { new SqliteCompiler() });
            Add(new object[] { new SqlServerCompiler() });
            Add(new object[] { new SqlServerCompiler {UseLegacyPagination = true} });
        }
    }
}
