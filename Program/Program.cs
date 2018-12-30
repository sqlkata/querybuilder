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

            var db = new QueryFactory(connection, new SqlServerCompiler
            {
                UseLegacyPagination = true
            });

            db.Logger = q => Console.WriteLine(q.ToString());

            var accounts = db.Query("Accounts")
                .ForPage(2, 10)
                .WhereRaw("[CurrencyId] in (?)", new object[] { 11 })
                .WhereRaw("[CurrencyId] in (?)", new[] { 1, 2, 3 })
                .WhereRaw("[CurrencyId] in (?)", new[] { "100", "200" })
                .Get();

            Console.WriteLine(JsonConvert.SerializeObject(accounts));

        }

        private static void log(Compiler compiler, Query query)
        {
            Console.WriteLine(compiler.Compile(query).ToString());
        }

    }
}
