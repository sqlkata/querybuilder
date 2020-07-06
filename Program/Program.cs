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

            /*
            var accounts = db.Query("Accounts")
                .WhereNotNull("BankId")
                .Include("bank",
                    db.Query("Banks").Include("Country", db.Query("Countries"))
                        .Select("Id", "Name", "CountryId")
                )
                .Select("Id", "Name", "BankId")
                .OrderByDesc("Id").Limit(10).Get();
            */

            var includedAccountsQuery = db.Query("Accounts").Limit(2)
                .IncludeMany("Transactions", db.Query("Transactions"))
                .Include("Company", db.Query("Companies"));

            var bank = db.Query("Banks as Icon")
                .IncludeMany("Accounts", includedAccountsQuery, "BankId")
                .WhereExists(q => q.From("Accounts").WhereColumns("Accounts.BankId", "=", "Icon.Id"))
                .Limit(1)
                .Get();

            Console.WriteLine(JsonConvert.SerializeObject(bank, Formatting.Indented));

        }

        private static void log(Compiler compiler, Query query)
        {
            Console.WriteLine(compiler.Compile(query).ToString());
        }

    }
}
