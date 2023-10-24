using SqlKata.Compilers;
using Xunit;
using SqlKata.Execution;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using static SqlKata.Expressions;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SqlKata.Tests
{
    public class MySqlExecutionTest
    {
        [Fact]
        public void EmptySelect()
        {
            var db = DB().Create("Cars", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Brand TEXT NOT NULL",
                "Year INT NOT NULL",
                "Color TEXT NULL",
            });

            var rows = db.Query("Cars").Get();

            Assert.Empty(rows);

            db.Drop("Cars");
        }

        [Fact]
        public void SelectWithLimit()
        {
            var db = DB().Create("Cars", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Brand TEXT NOT NULL",
                "Year INT NOT NULL",
                "Color TEXT NULL",
            });

            db.Statement("INSERT INTO `Cars`(Brand, Year) VALUES ('Honda', 2020)");

            var rows = db.Query("Cars").Get().ToList();

            Assert.Single(rows);

            db.Drop("Cars");
        }

        [Fact]
        public void Count()
        {
            var db = DB().Create("Cars", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Brand TEXT NOT NULL",
                "Year INT NOT NULL",
                "Color TEXT NULL",
            });

            db.Statement("INSERT INTO `Cars`(Brand, Year) VALUES ('Honda', 2020)");
            var count = db.Query("Cars").Count<int>();
            Assert.Equal(1, count);

            db.Statement("INSERT INTO `Cars`(Brand, Year) VALUES ('Toyota', 2021)");
            count = db.Query("Cars").Count<int>();
            Assert.Equal(2, count);

            int affected = db.Query("Cars").Delete();
            Assert.Equal(2, affected);

            count = db.Query("Cars").Count<int>();
            Assert.Equal(0, count);

            db.Drop("Cars");
        }

        [Fact]
        public void CloneThenCount()
        {
            var db = DB().Create("Cars", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Brand TEXT NOT NULL",
                "Year INT NOT NULL",
                "Color TEXT NULL",
            });

            for (int i = 0; i < 10; i++)
            {
                db.Query("Cars").Insert(new
                {
                    Brand = "Brand " + i,
                    Year = "2020",
                });
            }

            var query = db.Query("Cars").Where("Id", "<", 5);
            var count = query.Count<int>();
            var cloneCount = query.Clone().Count<int>();

            Assert.Equal(4, count);
            Assert.Equal(4, cloneCount);

            db.Drop("Cars");
        }

        [Fact]
        public void QueryWithVariable()
        {
            var db = DB().Create("Cars", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Brand TEXT NOT NULL",
                "Year INT NOT NULL",
                "Color TEXT NULL",
            });

            for (int i = 0; i < 10; i++)
            {
                db.Query("Cars").Insert(new
                {
                    Brand = "Brand " + i,
                    Year = "2020",
                });
            }


            var count = db.Query("Cars")
                .Define("Threshold", 5)
                .Where("Id", "<", SqlKata.Expressions.Variable("Threshold"))
                .Count<int>();

            Assert.Equal(4, count);

            db.Drop("Cars");
        }

        [Fact]
        public void InlineTable()
        {
            var db = DB().Create("Transaction", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Amount int NOT NULL",
                "Date DATE NOT NULL",
            });

            db.Query("Transaction").Insert(new
            {
                Date = "2022-01-01",
                Amount = 10
            });


            var rows = db.Query("Transaction")
                .With("Rates", new[] { "Date", "Rate" }, new object[][]
                {
                    new object[] { "2022-01-01", 0.5 },
                })
                .Join("Rates", "Rates.Date", "Transaction.Date")
                .SelectRaw("Transaction.Amount * Rates.Rate as AmountConverted")
                .Get();

            Assert.Single(rows);
            Assert.Equal(5, rows.First().AmountConverted);

            db.Drop("Transaction");
        }

        [Fact]
        public void ExistsShouldReturnFalseForEmptyTable()
        {
            var db = DB().Create("Transaction", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Amount int NOT NULL",
                "Date DATE NOT NULL",
            });

            var exists = db.Query("Transaction").Exists();
            Assert.False(exists);

            db.Drop("Transaction");
        }

        [Fact]
        public void ExistsShouldReturnTrueForNonEmptyTable()
        {
            var db = DB().Create("Transaction", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Amount int NOT NULL",
                "Date DATE NOT NULL",
            });

            db.Query("Transaction").Insert(new
            {
                Date = "2022-01-01",
                Amount = 10
            });

            var exists = db.Query("Transaction").Exists();
            Assert.True(exists);

            db.Drop("Transaction");
        }

        [Fact]
        public void BasicSelectFilter()
        {
            var db = DB().Create("Transaction", new[]
            {
                "Id INT PRIMARY KEY AUTO_INCREMENT",
                "Date DATE NOT NULL",
                "Amount int NOT NULL",
            });

            var data = new Dictionary<string, int>
            {
                // 2020
                { "2020-01-01", 10 },
                { "2020-05-01", 20 },

                // 2021
                { "2021-01-01", 40 },
                { "2021-02-01", 10 },
                { "2021-04-01", -10 },

                // 2022
                { "2022-01-01", 80 },
                { "2022-02-01", -30 },
                { "2022-05-01", 50 },
            };

            foreach (var row in data)
            {
                db.Query("Transaction").Insert(new
                {
                    Date = row.Key,
                    Amount = row.Value
                });
            }

            var query = db.Query("Transaction")
                    .SelectSum("Amount as Total_2020", q => q.WhereDatePart("year", "date", 2020))
                    .SelectSum("Amount as Total_2021", q => q.WhereDatePart("year", "date", 2021))
                    .SelectSum("Amount as Total_2022", q => q.WhereDatePart("year", "date", 2022))
                ;

            var results = query.Get().ToList();
            Assert.Single(results);
            Assert.Equal(30, results[0].Total_2020);
            Assert.Equal(40, results[0].Total_2021);
            Assert.Equal(100, results[0].Total_2022);

            db.Drop("Transaction");
        }

        QueryFactory DB()
        {
            var connection = GetConnectionFromEnv() ?? GetConnectionFromConfig();
            var db = new QueryFactory(connection, new MySqlCompiler());
            return db;
        }

        MySqlConnection GetConnectionFromEnv()
        {
            var host = System.Environment.GetEnvironmentVariable("SQLKATA_MYSQL_HOST");
            var user = System.Environment.GetEnvironmentVariable("SQLKATA_MYSQL_USER");
            var dbName = System.Environment.GetEnvironmentVariable("SQLKATA_MYSQL_DB");

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(dbName))
                return null;

            var cs = $"server={host};user={user};database={dbName}";

            return new MySqlConnection(cs);
        }

        MySqlConnection GetConnectionFromConfig()
        {
            var settings = File.ReadAllText("config.json");
            var deserializedSettings = JsonConvert.DeserializeObject<dynamic>(settings);

            return new MySqlConnection(deserializedSettings.ConnectionStrings.MySql.ToString());
        }
    }
}
