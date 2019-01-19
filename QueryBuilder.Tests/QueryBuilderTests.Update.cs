using System;
using System.Collections.Generic;
using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public partial class QueryBuilderTests
    {
        [Fact]
        public void InsertObject()
        {
            var query = new Query("Table").AsInsert(new
            {
                Name = "The User",
                Age = new DateTime(2018, 1, 1),
            });

            var c = Compile(query);

            Assert.Equal("INSERT INTO [Table] ([Name], [Age]) VALUES ('The User', '2018-01-01')", c[EngineCodes.SqlServer]);


            Assert.Equal("INSERT INTO \"TABLE\" (\"NAME\", \"AGE\") VALUES ('The User', '2018-01-01')", c[EngineCodes.Firebird]);
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

            Assert.Equal("UPDATE [Table] SET [Name] = 'The User', [Age] = '2018-01-01'", c[EngineCodes.SqlServer]);


            Assert.Equal("UPDATE \"TABLE\" SET \"NAME\" = 'The User', \"AGE\" = '2018-01-01'", c[EngineCodes.Firebird]);
        }

        [Fact]
        public void UpdateWithNullValues()
        {
            var query = new Query("Books").Where("Id", 1).AsUpdate(
                new[] { "Author", "Date", "Version" },
                new object[] { "Author 1", null, null }
            );

            var c = Compile(query);

            Assert.Equal("UPDATE [Books] SET [Author] = 'Author 1', [Date] = NULL, [Version] = NULL WHERE [Id] = 1",
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
    }
}