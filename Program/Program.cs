using System;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var query = new Query("table")
            .WhereLike("name", "a")
            .Limit(10).Offset(5)
            .ForPage(3, 4);

            // var sql = compiler.Compile(query);
             // Console.WriteLine(sql);
            // Console.WriteLine(string.Join(", ", compiler.GetBindings()));



            var compiler = new SqlServerCompiler();
            var connection = new SqlConnection(
                "Server=tcp:localhost,1433;Initial Catalog=Lite;User ID=sa;Password=P@ssw0rd"
            );

            var db = new QueryFactory(connection, compiler);

            var r = db.StatementAsync("UPDATE Recurring set Description = concat(Id, 2)", null).GetAwaiter().GetResult();
            Console.WriteLine("result: " + r);

        }
    }
}
