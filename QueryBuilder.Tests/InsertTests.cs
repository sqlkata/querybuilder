using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public class InsertTests : TestSupport
    {
        private class Account
        {
            public Account(string name, string currency = null, string created_at = null, string color = null)
            {
                this.name = name ?? throw new ArgumentNullException(nameof(name));
                this.Currency = currency;
                this.color = color;
            }

            public string name { get; set; }

            [Column("currency_id")]
            public string Currency { get; set; }

            [Ignore]
            public string color { get; set; }
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')")]
        [InlineData(EngineCodes.Firebird, "INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01')")]
        public void InsertObject(string engine, string sqlText)
        {
            var query = new Query("Table")
                .AsInsert(
                    new
                    {
                        Name = "The User",
                        Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    });

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "WITH [old_cards] AS (SELECT * FROM [all_cars] WHERE [year] < 2000)\nINSERT INTO [expensive_cars] ([name], [model], [year]) SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS [row_num] FROM [old_cars] WHERE [price] > 100) AS [results_wrapper] WHERE [row_num] BETWEEN 11 AND 20")]
        [InlineData(EngineCodes.MySql, "WITH `old_cards` AS (SELECT * FROM `all_cars` WHERE `year` < 2000)\nINSERT INTO `expensive_cars` (`name`, `model`, `year`) SELECT * FROM `old_cars` WHERE `price` > 100 LIMIT 10 OFFSET 10")]
        [InlineData(EngineCodes.PostgreSql, "WITH \"old_cards\" AS (SELECT * FROM \"all_cars\" WHERE \"year\" < 2000)\nINSERT INTO \"expensive_cars\" (\"name\", \"model\", \"year\") SELECT * FROM \"old_cars\" WHERE \"price\" > 100 LIMIT 10 OFFSET 10")]
        public void InsertFromSubQueryWithCte(string engine, string sqlText)
        {
            var query = new Query("expensive_cars")
                .With("old_cards", new Query("all_cars").Where("year", "<", 2000))
                .AsInsert(
                    new[] { "name", "model", "year" },
                    new Query("old_cars").Where("price", ">", 100).ForPage(2, 10));

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "INSERT INTO [expensive_cars] ([name], [brand], [year]) VALUES ('Chiron', 'Bugatti', NULL), ('Huayra', 'Pagani', 2012), ('Reventon roadster', 'Lamborghini', 2009)")]
        [InlineData(EngineCodes.Firebird, "INSERT INTO \"EXPENSIVE_CARS\" (\"NAME\", \"BRAND\", \"YEAR\") SELECT 'Chiron', 'Bugatti', NULL FROM RDB$DATABASE UNION ALL SELECT 'Huayra', 'Pagani', 2012 FROM RDB$DATABASE UNION ALL SELECT 'Reventon roadster', 'Lamborghini', 2009 FROM RDB$DATABASE")]
        public void InsertMultiRecords(string engine, string sqlText)
        {
            var query = new Query("expensive_cars")
                .AsInsert(
                    new[] { "name", "brand", "year" },
                    new[]
                    {
                        new object[] { "Chiron", "Bugatti", null },
                        new object[] { "Huayra", "Pagani", 2012 },
                        new object[] { "Reventon roadster", "Lamborghini", 2009 }
                    });

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "INSERT INTO [Books] ([Id], [Author], [ISBN], [Date]) VALUES (1, 'Author 1', '123456', NULL)")]
        [InlineData(EngineCodes.Firebird, "INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DATE\") VALUES (1, 'Author 1', '123456', NULL)")]
        public void InsertWithNullValues(string engine, string sqlText)
        {
            var query = new Query("Books")
                .AsInsert(
                    new[] { "Id", "Author", "ISBN", "Date" },
                    new object[] { 1, "Author 1", "123456", null });

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "INSERT INTO [Books] ([Id], [Author], [ISBN], [Description]) VALUES (1, 'Author 1', '123456', '')")]
        [InlineData(EngineCodes.Firebird, "INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DESCRIPTION\") VALUES (1, 'Author 1', '123456', '')")]
        public void InsertWithEmptyString(string engine, string sqlText)
        {
            var query = new Query("Books")
                .AsInsert(
                    new[] { "Id", "Author", "ISBN", "Description" },
                    new object[] { 1, "Author 1", "123456", "" });

            var c = CompileFor(engine, query);

            Assert.Equal(sqlText, c.ToString());
        }

        [Fact]
        public void InsertWithByteArray()
        {
            var fauxImagebytes = new byte[] { 0x1, 0x3, 0x3, 0x7 };
            var query = new Query("Books")
                .AsInsert(
                    new[] { "Id", "CoverImageBytes" },
                    new object[]
                    {
                        1,
                        fauxImagebytes
                    });

            var c = CompileFor(EngineCodes.SqlServer, query);

            Assert.Equal(2, c.NamedBindings.Count);
            Assert.Equal("INSERT INTO [Books] ([Id], [CoverImageBytes]) VALUES (?, ?)", c.RawSql);
            Assert.Equal("INSERT INTO [Books] ([Id], [CoverImageBytes]) VALUES (@p0, @p1)", c.Sql);
            Assert.Equal(1, c.NamedBindings["@p0"]);
            Assert.Equal(fauxImagebytes, c.NamedBindings["@p1"]);
        }

        [Fact]
        public void InsertWithIgnoreAndColumnProperties()
        {
            var account = new Account(name: $"popular", color: $"blue", currency: "US");
            var query = new Query("Account").AsInsert(account);

            var c = Compile(query);

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
            var query = new Query()
                .FromRaw("Table.With.Dots")
                .AsInsert(
                    new
                    {
                        Name = "The User",
                        Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    });

            var c = Compile(query);

            Assert.Equal(
                "INSERT INTO Table.With.Dots ([Name], [Age]) VALUES ('The User', '2018-01-01')",
                c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void InsertFromQueryShouldFail()
        {
            var query = new Query()
                .From(new Query("InnerTable"))
                .AsInsert(
                    new
                    {
                        Name = "The User",
                        Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    });

            Assert.Throws<InvalidOperationException>(() =>
            {
                Compile(query);
            });
        }

        [Fact]
        public void InsertKeyValuePairs()
        {
            var dictionaryUser = new Dictionary<string, object>
                {
                    { "Name", "The User" },
                    { "Age",  new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                }
                .ToArray();

            var query = new Query("Table")
                .AsInsert(dictionaryUser);

            var c = Compile(query);

            Assert.Equal(
                "INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01')",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertDictionary()
        {
            var dictionaryUser = new Dictionary<string, object> {
                { "Name", "The User" },
                { "Age",  new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            };

            var query = new Query("Table")
                .AsInsert(dictionaryUser);

            var c = Compile(query);

            Assert.Equal(
                "INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01')",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertReadOnlyDictionary()
        {
            var dictionaryUser = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object>
                {
                    { "Name", "The User" },
                    { "Age",  new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                });

            var query = new Query("Table")
                .AsInsert(dictionaryUser);

            var c = Compile(query);

            Assert.Equal(
                "INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01')",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertExpandoObject()
        {
            dynamic expandoUser = new ExpandoObject();
            expandoUser.Name = "The User";
            expandoUser.Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var query = new Query("Table")
                .AsInsert(expandoUser);

            var c = Compile(query);

            Assert.Equal(
                "INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01')",
                c[EngineCodes.Firebird]);
        }
    }
}
