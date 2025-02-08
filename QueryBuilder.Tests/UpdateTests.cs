using System.Dynamic;
using SqlKata.Tests.Infrastructure;

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

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'")]
        [InlineData(EngineCodes.Firebird, "UPDATE \"TABLE\" SET \"NAME\" = 'The User', \"AGE\" = '2018-01-01'")]
        public void UpdateObject(string engine, string sqlText)
        {
            var query = new Query("Table").AsUpdate(new
            {
                Name = "The User",
                Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Books] SET [Author] = 'Author 1', [Date] = NULL, [Version] = NULL WHERE [Id] = 1")]
        [InlineData(EngineCodes.Firebird, "UPDATE \"BOOKS\" SET \"AUTHOR\" = 'Author 1', \"DATE\" = NULL, \"VERSION\" = NULL WHERE \"ID\" = 1")]
        public void UpdateWithNullValues(string engine, string sqlText)
        {
            var query = new Query("Books").Where("Id", 1).AsUpdate(
                new[] { "Author", "Date", "Version" },
                new object[] { "Author 1", null, null }
            );

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Books] SET [Author] = 'Author 1', [Description] = '' WHERE [Id] = 1")]
        [InlineData(EngineCodes.Firebird, "UPDATE \"BOOKS\" SET \"AUTHOR\" = 'Author 1', \"DESCRIPTION\" = '' WHERE \"ID\" = 1")]
        public void UpdateWithEmptyString(string engine, string sqlText)
        {
            var query = new Query("Books").Where("Id", 1).AsUpdate(
                new[] { "Author", "Description" },
                new object[] { "Author 1", "" }
            );

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "WITH [OldBooks] AS (SELECT * FROM [Books] WHERE [Date] < '2024-05-24')\nUPDATE [Books] SET [Price] = '150' WHERE [Price] > 100")]
        public void UpdateWithCte(string engine, string sqlText)
        {
            var query = new Query("Books")
                .With("OldBooks", q => q.From("Books").Where("Date", "<", "2024-05-24"))
                .Where("Price", ">", 100)
                .AsUpdate(new Dictionary<string, object>
                {
                    {"Price", "150"}
                });

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Book] SET [Name] = 'SqlKataBook', [Author] = 'Kata', [Price] = 100")]
        [InlineData(EngineCodes.Firebird, "UPDATE \"BOOK\" SET \"NAME\" = 'SqlKataBook', \"AUTHOR\" = 'Kata', \"PRICE\" = 100")]
        public void UpdateWithIgnoreAndColumnProperties(string engine, string sqlText)
        {
            var book = new Book(name: $"SqlKataBook", author: "Kata", color: $"red", price: 100m);
            var query = new Query("Book").AsUpdate(book);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [OrderProductComposite] SET [OrdId] = 'ORD01', [ProductId] = 'PROD02', [Quantity] = 20, [Faa] = 'baz' WHERE [OrdId] = 'ORD01' AND [ProductId] = 'PROD02'")]
        [InlineData(EngineCodes.Firebird, "UPDATE \"ORDERPRODUCTCOMPOSITE\" SET \"ORDID\" = 'ORD01', \"PRODUCTID\" = 'PROD02', \"QUANTITY\" = 20, \"FAA\" = 'baz' WHERE \"ORDID\" = 'ORD01' AND \"PRODUCTID\" = 'PROD02'")]
        public void UpdateWithKeyAttribute(string engine, string sqlText)
        {
            var order = new OrderProductComposite("ORD01", "PROD02", 20);
            var query = new Query("OrderProductComposite").AsUpdate(order);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE Table.With.Dots SET [Name] = 'The User'")]
        public void UpdateFromRaw(string engine, string sqlText)
        {
            var query = new Query().FromRaw("Table.With.Dots").AsUpdate(new
            {
                Name = "The User",
            });

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.Firebird)]
        [InlineData(EngineCodes.MySql)]
        [InlineData(EngineCodes.Oracle)]
        [InlineData(EngineCodes.PostgreSql)]
        [InlineData(EngineCodes.Sqlite)]
        [InlineData(EngineCodes.SqlServer)]
        public void UpdateFromQueryShouldFail(string engine)
        {
            var query = new Query().From(new Query("InnerTable")).AsUpdate(new
            {
                Name = "The User",
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = CompileFor(engine, query);
            });
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [MyTable] SET [Name] = ?, [Address] = @address")]
        public void update_should_compile_literal_without_parameters_holders(string engine, string sqlText)
        {
            var query = new Query("MyTable").AsUpdate(new
            {
                Name = "The User",
                Address = new UnsafeLiteral("@address")
            });

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.RawSql);
            Assert.Single(result.NamedBindings);
            Assert.Equal("The User", result.NamedBindings.First().Value);
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'")]
        public void UpdateUsingKeyValuePairs(string engine, string sqlText)
        {
            var dictionaryUser = new Dictionary<string, object>
                {
                    { "Name", "The User" },
                    { "Age",  new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                }
                .ToArray();

            var query = new Query("Table")
                .AsUpdate(dictionaryUser);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'")]
        public void UpdateUsingExpandoObject(string engine, string sqlText)
        {
            dynamic expandoUser = new ExpandoObject();
            expandoUser.Name = "The User";
            expandoUser.Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var query = new Query("Table")
                .AsUpdate(expandoUser);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Table] SET [Total] = [Total] + 1")]
        public void IncrementUpdate(string engine, string sqlText)
        {
            var query = new Query("Table").AsIncrement("Total");

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Table] SET [Total] = [Total] + 2")]
        public void IncrementUpdateWithValue(string engine, string sqlText)
        {
            var query = new Query("Table").AsIncrement("Total", 2);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Table] SET [Total] = [Total] + 2 WHERE [Name] = 'A'")]
        public void IncrementUpdateWithWheres(string engine, string sqlText)
        {
            var query = new Query("Table").Where("Name", "A").AsIncrement("Total", 2);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "UPDATE [Table] SET [Total] = [Total] - 2 WHERE [Name] = 'A'")]
        public void DecrementUpdate(string engine, string sqlText)
        {
            var query = new Query("Table").Where("Name", "A").AsDecrement("Total", 2);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }
    }
}
