using System;
using System.Collections.Generic;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data.SQLite;
using static SqlKata.Expressions;
using System.IO;

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
            using (var db = SqlLiteQueryFactory())
            {
                var query = db.Query("accounts")
                    .Where("balance", ">", 0)
                    .Where("currency_id", "=", Variable("@CurrencyID"))
                    .GroupBy("balance")
                    .Limit(10)
                    .Define("@CurrencyID", "USD");

                var accounts = query.Clone().Get();
                Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));

                var exists = query.Clone().Exists();
                Console.WriteLine(exists);
            }
        }

        private static void log(SqlResult result)
        {
            Console.WriteLine("Query:\t\t\t" + result.Sql);
            Console.WriteLine("Bindings By Name:\t" + JsonConvert.SerializeObject(result.NamedBindings));
            Console.WriteLine("Bindings By Position:\t" + JsonConvert.SerializeObject(result.Bindings));
        }

        private static QueryFactory SqlLiteQueryFactory()
        {
            var compiler = new SqliteCompiler() { UseCustomNamedParameters = true };

            var connection = new SQLiteConnection("Data Source=Demo.db");

            var db = new QueryFactory(connection, compiler);

            db.Logger = log;

            if (!File.Exists("Demo.db"))
            {
                Console.WriteLine("db not exists creating db");

                SQLiteConnection.CreateFile("Demo.db");

                db.Statement("create table accounts(id integer primary key autoincrement, name varchar, currency_id varchar, balance decimal, created_at datetime);");
                for (var i = 0; i < 10; i++)
                {
                    db.Statement("insert into accounts(name, currency_id, balance, created_at) values(@name, @currency, @balance, @date)", new
                    {
                        name = $"Account {i}",
                        currency = "USD",
                        balance = 100 * i * 1.1,
                        date = DateTime.UtcNow,
                    });
                }

            }

            return db;

        }

        private static QueryFactory SqlServerQueryFactory()
        {
            var compiler = new PostgresCompiler();
            var connection = new SqlConnection(
               "Server=tcp:localhost,1433;Initial Catalog=Lite;User ID=sa;Password=P@ssw0rd"
           );

            var db = new QueryFactory(connection, compiler);

            db.Logger = log;

            return db;
        }
    }
}
