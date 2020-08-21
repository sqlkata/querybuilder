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
using SqlKata.SqlExpressions;
using System.Linq.Expressions;
using static SqlKata.SqlExpressions.Functions;
using static SqlKata.SqlExpressions.Conditions;
using static SqlKata.SqlExpressions.Common;

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

            Expression amountIsBig = Expression.Condition(
                        Expression.GreaterThan(
                            Expression.Parameter(typeof(int), "Amount"),
                            Expression.Constant(5)
                        ),
                        Expression.Constant("Big"),
                        Expression.Constant("Small")
                    );


            var query = new Query("Users").Where(
                GreaterThan("Name", "5")
            ).Where(
                LessThan(Length("Amount"), Literal(10))
            ).Select(
                Case(Length("Name")).When(
                    Literal(10),
                    StringValue("Big")
                ).Otherwise(
                    StringValue("Small")
                ),
                "NameIsBig"
            );

            log(new MySqlCompiler(), query);
            log(new SqlServerCompiler(), query);

            // query = new Query("Users").Select(
            //     new Cast("Age", CastType.Bool), "BooleanAge"
            // );

            // log(new MySqlCompiler(), query);
            // log(new SqlServerCompiler(), query);

            // query = new Query("Users").Select(
            //     new Function("GetExchangeRate", new Concat("Name", "LastName"), new StringValue("Anything")), "UpperName"
            // );

            // log(new MySqlCompiler(), query);
            // log(new SqlServerCompiler(), query);


            /*
            using (var db = SqlLiteQueryFactory())
            {
                var query = db.Query("accounts")
                    .Where("balance", ">", 0)
                    .Select(new Concat("name", "'+'", "balance"))
                    .GroupBy("balance")
                .Limit(10);

                var result = db.Query("accounts").InsertGetId<int>(new
                {
                    name = "New Account",
                    currency_id = "GBP",
                    balance = UnsafeLiteral("100 + 1"),
                    created_at = DateTime.UtcNow
                });

                Console.WriteLine(result);

                var accounts = query.Clone().Get();
                Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));

                var exists = query.Clone().Exists();
                Console.WriteLine(exists);
            }
            */
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
                Console.WriteLine(result.Sql);
            };

            if (!File.Exists("Demo.db"))
            {
                Console.WriteLine("db not exists creating db");

                SQLiteConnection.CreateFile("Demo.db");

                db.Statement("create table accounts(id integer primary key autoincrement, name varchar, currency_id varchar, balance decimal, created_at datetime);");
                for (var i = 0; i < 10; i++)
                {
                    db.Statement(@"insert into accounts(
                        name,
                        currency_id,
                        balance,
                        created_at
                    ) values(@name, @currency, @balance, @date)", new
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

            db.Logger = result =>
            {
                Console.WriteLine(result.ToString());
            };

            return db;
        }

    }

    public class MyOracleCompiler : OracleCompiler
    {
        MyOracleCompiler()
        {
            OpeningIdentifier = "";
            ClosingIdentifier = "";
        }
    }
}
