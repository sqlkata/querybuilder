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
    public class UpdateTests : TestSupport
    {
        private class Book
        {
            public Book(string name, string author, decimal price = 1.0m, string color = null)
            {
                this.Name = name ?? throw new ArgumentNullException(nameof(name));
                this.BookPrice = price;
                this.color = color;
                this.BookAuthor = author;
            }

            public string Name { get; set; }

            [Column("Author")]
            public string BookAuthor { get; set; }

            [Column("Price")]
            public decimal BookPrice { get; set; }

            [Ignore]
            public string color { get; set; }
        }

        private class OrderProductComposite
        {
            public OrderProductComposite(string orderid, string productid, int quantity)
            {
                OrderId = orderid;
                ProductId = productid;
                Quantity = quantity;
                Foo = "baz";
            }

            [Key("OrdId")]
            public string OrderId { get; set; }

            [Key]
            public string ProductId { get; set; }

            public int Quantity { get; set; }

            [Column("Faa")]
            public string Foo { get; set; }
        }

        [Fact]
        public void UpdateObject()
        {
            var query = new Query("Table").AsUpdate(new
            {
                Name = "The User",
                Age = new DateTime(2018, 1, 1),
            });

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "UPDATE \"TABLE\" SET \"NAME\" = 'The User', \"AGE\" = '2018-01-01'",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UpdateWithNullValues()
        {
            var query = new Query("Books").Where("Id", 1).AsUpdate(
                new[] { "Author", "Date", "Version" },
                new object[] { "Author 1", null, null }
            );

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [Books] SET [Author] = 'Author 1', [Date] = NULL, [Version] = NULL WHERE [Id] = 1",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "UPDATE \"BOOKS\" SET \"AUTHOR\" = 'Author 1', \"DATE\" = NULL, \"VERSION\" = NULL WHERE \"ID\" = 1",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UpdateWithEmptyString()
        {
            var query = new Query("Books").Where("Id", 1).AsUpdate(
                new[] { "Author", "Description" },
                new object[] { "Author 1", "" }
            );

            var c = Compile(query);

            Assert.Equal("UPDATE [Books] SET [Author] = 'Author 1', [Description] = '' WHERE [Id] = 1", c[EngineCodes.SqlServer]);

            Assert.Equal("UPDATE \"BOOKS\" SET \"AUTHOR\" = 'Author 1', \"DESCRIPTION\" = '' WHERE \"ID\" = 1", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UpdateWithCte()
        {
            var now = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var query = new Query("Books")
                .With("OldBooks", q => q.From("Books").Where("Date", "<", now))
                .Where("Price", ">", 100)
                .AsUpdate(new Dictionary<string, object>
                {
                    {"Price", "150"}
                });

            var c = Compile(query);

            Assert.Equal(
                $"WITH [OldBooks] AS (SELECT * FROM [Books] WHERE [Date] < '{now}')\nUPDATE [Books] SET [Price] = '150' WHERE [Price] > 100",
                c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void UpdateWithIgnoreAndColumnProperties()
        {
            var book = new Book(name: $"SqlKataBook", author: "Kata", color: $"red", price: 100m);
            var query = new Query("Book").AsUpdate(book);

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [Book] SET [Name] = 'SqlKataBook', [Author] = 'Kata', [Price] = 100",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "UPDATE \"BOOK\" SET \"NAME\" = 'SqlKataBook', \"AUTHOR\" = 'Kata', \"PRICE\" = 100",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UpdateWithKeyAttribute()
        {
            var order = new OrderProductComposite("ORD01", "PROD02", 20);
            var query = new Query("OrderProductComposite").AsUpdate(order);

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [OrderProductComposite] SET [OrdId] = 'ORD01', [ProductId] = 'PROD02', [Quantity] = 20, [Faa] = 'baz' WHERE [OrdId] = 'ORD01' AND [ProductId] = 'PROD02'",
                c[EngineCodes.SqlServer]);

            Assert.Equal(
                "UPDATE \"ORDERPRODUCTCOMPOSITE\" SET \"ORDID\" = 'ORD01', \"PRODUCTID\" = 'PROD02', \"QUANTITY\" = 20, \"FAA\" = 'baz' WHERE \"ORDID\" = 'ORD01' AND \"PRODUCTID\" = 'PROD02'",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UpdateFromRaw()
        {
            var query = new Query().FromRaw("Table.With.Dots").AsUpdate(new
            {
                Name = "The User",
            });

            var c = Compile(query);

            Assert.Equal(
                "UPDATE Table.With.Dots SET [Name] = 'The User'",
                c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void UpdateFromQueryShouldFail()
        {
            var query = new Query().From(new Query("InnerTable")).AsUpdate(new
            {
                Name = "The User",
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                Compile(query);
            });
        }

        [Fact]
        public void update_should_compile_literal_without_parameters_holders()
        {
            var query = new Query("MyTable").AsUpdate(new
            {
                Name = "The User",
                Address = new UnsafeLiteral("@address")
            });

            var compiler = new SqlServerCompiler();
            var result = compiler.Compile(query);

            Assert.Equal(
                "UPDATE [MyTable] SET [Name] = ?, [Address] = @address",
                result.RawSql);
        }

        [Fact]
        public void UpdateUsingKeyValuePairs()
        {
            var dictionaryUser = new Dictionary<string, object>
                {
                    { "Name", "The User" },
                    { "Age",  new DateTime(2018, 1, 1) },
                }
                .ToArray();

            var query = new Query("Table")
                .AsUpdate(dictionaryUser);

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'",
                c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void UpdateUsingDictionary()
        {
            var dictionaryUser = new Dictionary<string, object> {
                { "Name", "The User" },
                { "Age",  new DateTime(2018, 1, 1) },
            };

            var query = new Query("Table")
                .AsUpdate(dictionaryUser);

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'",
                c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void UpdateUsingReadOnlyDictionary()
        {
            var dictionaryUser = new ReadOnlyDictionary<string, object>(
                new Dictionary<string, object>
                {
                    { "Name", "The User" },
                    { "Age",  new DateTime(2018, 1, 1) },
                });

            var query = new Query("Table")
                .AsUpdate(dictionaryUser);

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'",
                c[EngineCodes.SqlServer]);
        }

        [Fact]
        public void UpdateUsingExpandoObject()
        {
            dynamic expandoUser = new ExpandoObject();
            expandoUser.Name = "The User";
            expandoUser.Age = new DateTime(2018, 1, 1);

            var query = new Query("Table")
                .AsUpdate(expandoUser);

            var c = Compile(query);

            Assert.Equal(
                "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'",
                c[EngineCodes.SqlServer]);
        }
    }
}