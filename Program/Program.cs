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
using Sqlkata.Compilers;

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

            connection = new SQLiteConnection("Data Source=Demo.db");

            var db = new QueryFactory(connection, new SqliteCompiler());

            // db.Statement("create table accounts(id integer primary key,name text,currency_id text);");

            db.Logger = q => Console.WriteLine(q.ToString());

            var accounts = db.Query("Accounts").OrderByDesc("Id").Offset(10).Get();

            Console.WriteLine(JsonConvert.SerializeObject(accounts));

        }

        private static void log(Compiler compiler, Query query)
        {
            Console.WriteLine(compiler.Compile(query).ToString());
        }

    }
}
