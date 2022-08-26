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

        public QueryFactory SetupDb()
        {
            Console.WriteLine($"Using cs: {MySqlInitialization.DbTestContainer.ConnectionString}");
            var connection = new MySqlConnection(MySqlInitialization.DbTestContainer.ConnectionString);
            var db = new QueryFactory(connection, new MySqlCompiler());
            db.Statement("DROP TABLE IF EXISTS `Cars`");
            return db;
        }
    }
}
