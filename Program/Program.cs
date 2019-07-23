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

        private class OrderDto
        {
            public Guid OrderId { get; set; }

            public DateTime Date { get; set; }

            public IList<OrderLineDto> OrderLines { get; set; }
        }

        private class OrderLineDto
        {
            public Guid OrderId { get; set; }

            public int Line { get; set; }

            public string ProductCode { get; set; }

            public decimal Price { get; set; }
        }

        static void Main(string[] args)
        {

            var db = SqlLiteQueryFactory();

            var id = db.Query("accounts").InsertGetId<int>(new
            {
                name = "new Account",
                currency_id = "USD",
                created_at = DateTime.UtcNow
            });

            var id2 = db.Select<int>("insert into accounts(name, currency_id, created_at) values ('account 2','usd','2019-01-01 20:00:00');select last_insert_rowid();");

            Console.WriteLine($"last id is: {id}");
            Console.WriteLine($"last id2 is: {id2.First()}");

        }

        private static void log(Compiler compiler, Query query)
        {
            var compiled = compiler.Compile(query);
            Console.WriteLine(compiled.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(compiled.Bindings));
        }

        private static QueryFactory SqlLiteQueryFactory()
        {
            var compiler = new SqliteCompiler();

            var connection = new SQLiteConnection("Data Source=Demo.db");

            var db = new QueryFactory(connection, compiler);

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
            var compiler = new PostgresCompiler();
            var connection = new SqlConnection(
               "Server=tcp:localhost,1433;Initial Catalog=Lite;User ID=sa;Password=P@ssw0rd"
           );

            var db = new QueryFactory(connection, compiler);

            db.Logger = result =>
            {
                Console.WriteLine(result.ToString());
            };

            return db;
        }

    }
}
