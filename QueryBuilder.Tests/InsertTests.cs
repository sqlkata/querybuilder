using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public class InsertTests : TestSupport
    {
        [Fact]
        public void InsertObject()
        {
            Query query = new Query("Table").AsInsert(new
            {
                Name = "The User",
                Age = new DateTime(2018, 1, 1),
            });

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')", c[EngineCodes.SqlServer]);


            Assert.Equal("INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01')", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertFromSubQueryWithCte()
        {
            Query query = new Query("expensive_cars")
                .With("old_cards", new Query("all_cars").Where("year", "<", 2000))
                .AsInsert(
                    new[] { "name", "model", "year" },
                    new Query("old_cars").Where("price", ">", 100).ForPage(2, 10)
                );

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal(
                "WITH [old_cards] AS (SELECT * FROM [all_cars] WHERE [year] < 2000)\nINSERT INTO [expensive_cars] ([name], [model], [year]) SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [old_cars] WHERE [price] > 100) AS [results_wrapper] WHERE [row_num] BETWEEN 11 AND 20",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "WITH `old_cards` AS (SELECT * FROM `all_cars` WHERE `year` < 2000)\nINSERT INTO `expensive_cars` (`name`, `model`, `year`) SELECT * FROM `old_cars` WHERE `price` > 100 LIMIT 10 OFFSET 10",
                c[EngineCodes.MySql]);

            Assert.Equal(
                "WITH \"old_cards\" AS (SELECT * FROM \"all_cars\" WHERE \"year\" < 2000)\nINSERT INTO \"expensive_cars\" (\"name\", \"model\", \"year\") SELECT * FROM \"old_cars\" WHERE \"price\" > 100 LIMIT 10 OFFSET 10",
                c[EngineCodes.PostgreSql]);
        }

        [Fact]
        public void InsertMultiRecords()
        {
            Query query = new Query("expensive_cars")
                .AsInsert(
                    new[] { "name", "brand", "year" },
                    new[]
                    {
                        new object[] {"Chiron", "Bugatti", null},
                        new object[] {"Huayra", "Pagani", 2012},
                        new object[] {"Reventon roadster", "Lamborghini", 2009}
                    }
                );

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal(
                "INSERT INTO [expensive_cars] ([name], [brand], [year]) VALUES ('Chiron', 'Bugatti', NULL), ('Huayra', 'Pagani', 2012), ('Reventon roadster', 'Lamborghini', 2009)",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "INSERT INTO \"EXPENSIVE_CARS\" (\"NAME\", \"BRAND\", \"YEAR\") SELECT 'Chiron', 'Bugatti', NULL FROM RDB$DATABASE UNION ALL SELECT 'Huayra', 'Pagani', 2012 FROM RDB$DATABASE UNION ALL SELECT 'Reventon roadster', 'Lamborghini', 2009 FROM RDB$DATABASE",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertWithNullValues()
        {
            Query query = new Query("Books").AsInsert(
                new[] { "Id", "Author", "ISBN", "Date" },
                new object[] { 1, "Author 1", "123456", null }
            );

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal("INSERT INTO [Books] ([Id], [Author], [ISBN], [Date]) VALUES (1, 'Author 1', '123456', NULL)",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DATE\") VALUES (1, 'Author 1', '123456', NULL)",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertWithEmptyString()
        {
            Query query = new Query("Books").AsInsert(
                new[] { "Id", "Author", "ISBN", "Description" },
                new object[] { 1, "Author 1", "123456", "" }
            );

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal(
                "INSERT INTO [Books] ([Id], [Author], [ISBN], [Description]) VALUES (1, 'Author 1', '123456', '')",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DESCRIPTION\") VALUES (1, 'Author 1', '123456', '')",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertWithByteArray()
        {
            byte[] fauxImagebytes = new byte[] { 0x1, 0x3, 0x3, 0x7 };
            Query query = new Query("Books")
                .AsInsert(new[] { "Id", "CoverImageBytes" },
                    new object[]
                    {
                        1,
                        fauxImagebytes
                    });

            TestSqlResultContainer c = Compilers.Compile(query);
            Assert.All(c.Values, a => Assert.Equal(2, a.NamedBindings.Count));

            SqlResult exemplar = c[EngineCodes.SqlServer];
            Assert.Equal("INSERT INTO [Books] ([Id], [CoverImageBytes]) VALUES (?, ?)", exemplar.RawSql);
            Assert.Equal("INSERT INTO [Books] ([Id], [CoverImageBytes]) VALUES (@p0, @p1)", exemplar.Sql);
        }


        private class Account
        {
            public Account(string name, string currency = null, string created_at = null, string color = null)
            {
                this.name = name ?? throw new ArgumentNullException("name must be provided");
                this.Currency = currency;
                this.color = color;
            }

            public string name { get; set; }
            [Column("currency_id")]
            public string Currency { get; set; }
            [Ignore]
            public string color { get; set; }
        }

        [Fact]
        public void InsertWithIgnoreAndColumnProperties()
        {
            Account account = new Account(name: $"popular", color: $"blue", currency: "US");
            Query query = new Query("Account").AsInsert(account);

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal(
                "INSERT INTO [Account] ([name], [currency_id]) VALUES ('popular', 'US')",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "INSERT INTO \"ACCOUNT\" (\"NAME\", \"CURRENCY_ID\") VALUES ('popular', 'US')",
                 c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertFromRaw()
        {
            Query query = new Query().FromRaw("Table.With.Dots").AsInsert(new
            {
                Name = "The User",
                Age = new DateTime(2018, 1, 1),
            });

            IReadOnlyDictionary<string, string> c = Compile(query);

            Assert.Equal(
                "INSERT INTO Table.With.Dots ([Name], [Age]) VALUES ('The User', '2018-01-01')",
                c[EngineCodes.SqlServer]
            );

        }


        [Fact]
        public void InsertFromQueryShouldFail()
        {
            Query query = new Query().From(new Query("InnerTable")).AsInsert(new
            {
                Name = "The User",
                Age = new DateTime(2018, 1, 1),
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                Compile(query);
            });

        }

    }
}