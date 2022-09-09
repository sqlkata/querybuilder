using SqlKata.Compilers;
using Xunit;
using SqlKata.Execution;
using MySql.Data.MySqlClient;
using System;
using System.Linq;

namespace SqlKata.Tests
{
    public class MySqlExecutionTest
    {
        [Fact]
        public void EmptySelect()
        {

            var db = SetupDb();
            var sql = @"
                CREATE TABLE Cars(
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    Brand TEXT NOT NULL,
                    Year INT NOT NULL,
                    Color TEXT NULL
                )
            ";
            db.Statement(sql);

            var rows = db.Query("Cars").Get();

            Assert.Empty(rows);

            db.Statement("DROP TABLE IF EXISTS `Cars`");
        }

        [Fact]
        public void SelectWithLimit()
        {

            var db = SetupDb();
            var sql = @"
                CREATE TABLE Cars(
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    Brand TEXT NOT NULL,
                    Year INT NOT NULL,
                    Color TEXT NULL
                )
            ";
            db.Statement(sql);

            db.Statement("INSERT INTO `Cars`(Brand, Year) VALUES ('Honda', 2020)");

            var rows = db.Query("Cars").Get().ToList();

            Assert.Equal(1, rows.Count());

            db.Statement("DROP TABLE IF EXISTS `Cars`");
        }

        [Fact]
        public void Count()
        {
            var db = SetupDb();
            var sql = @"
                CREATE TABLE Cars(
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    Brand TEXT NOT NULL,
                    Year INT NOT NULL,
                    Color TEXT NULL
                )
            ";
            db.Statement(sql);

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

            db.Statement("DROP TABLE IF EXISTS `Cars`");
        }

        [Fact]
        public void CloneThenCount()
        {
            var db = SetupDb();
            var sql = @"
                CREATE TABLE Cars(
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    Brand TEXT NOT NULL,
                    Year INT NOT NULL,
                    Color TEXT NULL
                )
            ";
            db.Statement(sql);

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

            db.Statement("DROP TABLE IF EXISTS `Cars`");
        }

        public QueryFactory SetupDb()
        {
            var host = System.Environment.GetEnvironmentVariable("SQLKATA_MYSQL_HOST");
            var user = System.Environment.GetEnvironmentVariable("SQLKATA_MYSQL_USER");
            var dbName = System.Environment.GetEnvironmentVariable("SQLKATA_MYSQL_DB");
            var cs = $"server={host};user={user};database={dbName}";
            Console.WriteLine($"Using cs: {cs}");

            var connection = new MySqlConnection(cs);

            var db = new QueryFactory(connection, new MySqlCompiler());

            db.Statement("DROP TABLE IF EXISTS `Cars`");

            return db;
        }
    }
}