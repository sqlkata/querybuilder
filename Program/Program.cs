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

            //var orders = new Dictionary<Guid, OrderDto>();
            //var data = db.Query("Orders as o")
            //             .Join("OrderLines as ol", "ol.OrderId", "o.OrderId")
            //             .SelectRaw("o.*, '' split, ol.*")
            //             .Where("o.OrderId", "=", "c65a3f10-475a-410b-ad07-b67922388a00")
            //             .GetAsync<OrderDto, OrderLineDto, OrderDto>((o, ol) =>
            //             {
            //                 if (!orders.TryGetValue(o.OrderId, out OrderDto order))
            //                 {
            //                     orders.Add(o.OrderId, order = o);
            //                 }

            //                 if (order.OrderLines == null) order.OrderLines = new List<OrderLineDto>();

            //                 order.OrderLines.Add(ol);

            //                 return order;
            //             },
            //             splitOn: "split").GetAwaiter().GetResult();

        }

        private static void log(Compiler compiler, Query query)
        {
            Console.WriteLine(compiler.Compile(query).ToString());
        }

    }
}
