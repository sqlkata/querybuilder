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
            sb.AppendLine();
            sb.Append(sqlResult);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("----------- RAW -------------");
            sb.AppendLine();
            sb.Append(sqlResult.RawSql);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("--------PARAMETRIZED --------");
            sb.AppendLine();
            sb.Append(sqlResult.Sql);

            return Verifier.Verify(sb.ToString(), "sql")
                .UseTextForParameters(compiler.GetType().Name)
                .UseDirectory("../Output");
        }
    }
    public class AllCompilers : List<object[]>
    {
        public AllCompilers()
        {
            Add(new object[] { new FirebirdCompiler() });
            Add(new object[] { new Compiler() });
            Add(new object[] { new MySqlCompiler() });
            Add(new object[] { new OracleCompiler() });
            Add(new object[] { new PostgresCompiler() });
            Add(new object[] { new SqliteCompiler() });
            Add(new object[] { new SqlServerCompiler() });
        }
    }
}
