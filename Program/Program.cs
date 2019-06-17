using System;
using System.Collections.Generic;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;
using System.Data;
using Dapper;
using System.Data.SQLite;
using static SqlKata.Query;

namespace Program
{
    class Program
    {
        private class Loan
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<Installment> Installments { get; set; } = new List<Installment>();
        }

        private class Installment
        {
            public string Id { get; set; }
            public string LoanId { get; set; }
            public int DaysCount { get; set; }
        }

        static void Main(string[] args)
        {

            IDbConnection connection = new SqlConnection(
                "Server=tcp:localhost,1433;Initial Catalog=Lite;User ID=sa;Password=P@ssw0rd"
            );

            // SQLiteConnection.CreateFile("Demo.db");

            // connection = new SQLiteConnection("Data Source=Demo.db");

            var db = new QueryFactory(connection, new SqlServerCompiler());

            db.Logger = result =>
            {
                Console.WriteLine(result.ToString());
            };

            var query = new Query("Users")
            .Define("Age", 17)
            .Where(q => q.Where("Age", ">", Variable("Age")).OrWhere("Age", "<", Variable("Age")));

            log(new SqlServerCompiler(), query);

        }

        private static void log(Compiler compiler, Query query)
        {
            var compiled = compiler.Compile(query);
            Console.WriteLine(compiled.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(compiled.Bindings));
        }

    }
}
