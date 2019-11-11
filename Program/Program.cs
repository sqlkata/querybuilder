using System;
using System.Collections.Generic;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;
using System.Data.SQLite;
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

            QueryFactory db = SqlLiteQueryFactory();

            int id = db.Query("accounts").InsertGetId<int>(new
            {
                name = "new Account",
                currency_id = "USD",
                created_at = DateTime.UtcNow
            });

            IEnumerable<int> id2 = db.Select<int>("insert into accounts(name, currency_id, created_at) values ('account 2','usd','2019-01-01 20:00:00');select last_insert_rowid();");

            Console.WriteLine($"last id is: {id}");
            Console.WriteLine($"last id2 is: {id2.First()}");

        }

        private static void log(Compiler compiler, Query query)
        {
            SqlResult compiled = compiler.Compile(query);
            Console.WriteLine(compiled.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(compiled.Bindings));
        }

        private static QueryFactory SqlLiteQueryFactory()
        {
            SqliteCompiler compiler = new SqliteCompiler();

            SQLiteConnection connection = new SQLiteConnection("Data Source=Demo.db");

            QueryFactory db = new QueryFactory(connection, compiler);

            db.Logger = result =>
            {
                Console.WriteLine(result.ToString());
            };

            if (!File.Exists("Demo.db"))
            {
                Console.WriteLine("db not exists creating db");

                SQLiteConnection.CreateFile("Demo.db");

                db.Statement("create table accounts(id integer primary key autoincrement, name varchar, currency_id varchar, created_at datetime);");

            }

            return db;
        }

        private static QueryFactory SqlServerQueryFactory()
        {
            PostgresCompiler compiler = new PostgresCompiler();
            SqlConnection connection = new SqlConnection(
               "Server=tcp:localhost,1433;Initial Catalog=Lite;User ID=sa;Password=P@ssw0rd"
           );

            QueryFactory db = new QueryFactory(connection, compiler);

            db.Logger = result =>
            {
                Console.WriteLine(result.ToString());
            };

            return db;
        }

    }
}
