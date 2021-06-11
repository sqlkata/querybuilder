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

        static async Task Main(string[] args)
        {
            using (var db = SqlServerQueryFactory())
            {

                //var readZoneStorages = db.Query("ReadZoneStorages").Join("ReadZones", "ReadZones.Id", "ReadZoneStorages.ReadZoneId")
                //                       .Select("ReadZones.FacilityId", "ReadZoneStorages.ReadZoneId", "ReadZones.Name as ReadZoneName", "ReadZones.Status", "ReadZones.DisplayOnDashboard", "ReadZoneStorages.GTINItemRef", "ReadZoneStorages.GRAIAssetType", "ReadZoneStorages.GIAIIndAssetRef", "ReadZoneStorages.SSCCSerialRef");
                //var facilityReadzones = db.Query("Facilities").Join(
                //            readZoneStorages.As("ReadZoneStorages"),
                //            j => j.On("ReadZoneStorages.FacilityId", "Facilities.Id")
                //            );
                //var gtinItemsInReadZones =  db.Query("Companies").Join(
                //                                facilityReadzones.As("FacilityReadzones"),
                //                                j => j.On("FacilityReadzones.CompanyId", "Companies.Id")
                //                                ).Select("ReadZoneName", "FacilityReadzones.Name as FacilityName", "Companies.Name as CompanyName", "ReadZoneId", "FacilityId", "Companies.Id as CompanyId").SelectRaw("count(1) as [Count]").GroupBy("Companies.Id", "FacilityId", "ReadZoneId", "ReadZoneName", "FacilityName", "CompanyName")
                //                                .WhereNotNull("FacilityReadzones.GTINItemRef")
                //                                .Where("FacilityReadzones.Status", "A")
                //                                .Where("FacilityReadzones.DisplayOnDashboard", "A")
                //                                 .Where("Companies.Id", 1);

                //var query = db.Query("accounts")
                //    .Where("balance", ">", 0)
                //    .GroupBy("balance")
                //.Limit(10);
                var companies = db.Query("Companies").Select("Id", "Name");
                var facilities = db.Query("Facilities").Select("CompanyId", "Name");
                var result = companies
                                .IncludeMany("Facilities", facilities, "CompanyId", "Id");

                var accounts = result.Clone().Get();
                Console.WriteLine(JsonConvert.SerializeObject(accounts, Formatting.Indented));

              //  var exists = gtinItemsInReadZones.Clone().Exists();
               // Console.WriteLine(exists);
            }
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
            var connection = new NpgsqlConnection(
               "Server=localhost;Port=5432;Database=ItemsScanLocalDb;User Id=chrisgate;Password=Dink01@secure!;"
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
