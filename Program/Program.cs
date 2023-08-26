using System;
using SqlKata.Compilers;
using SqlKata.Execution;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.IO;

namespace Program
{
    class Program
    {
      
        static void Main()
        {
            using (var db = SqlLiteQueryFactory())
            {
                var query = db.Query("accounts")
                    .Where("balance", ">", 0)
                    .GroupBy("balance")
                .Limit(10);

                var accounts = query.Clone().Get();
                Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));

                var exists = query.Clone().Exists();
                Console.WriteLine(exists);
            }
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


    }
}
