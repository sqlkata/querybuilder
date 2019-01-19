using SqlKata.Compilers;
using Xunit;

namespace SqlKata.Tests
{
    public partial class QueryBuilderTests
    {
        [Fact]
        public void InsertFromSubQueryWithCte()
        {
            var query = new Query("expensive_cars")
                .With("old_cards", new Query("all_cars").Where("year", "<", 2000))
                .AsInsert(
                    new[] { "name", "model", "year" },
                    new Query("old_cars").Where("price", ">", 100).ForPage(2, 10)
                );

            var c = Compile(query);

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
            var query = new Query("expensive_cars")
                .AsInsert(
                    new[] { "name", "brand", "year" },
                    new[]
                    {
                        new object[] {"Chiron", "Bugatti", null},
                        new object[] {"Huayra", "Pagani", 2012},
                        new object[] {"Reventon roadster", "Lamborghini", 2009}
                    }
                );

            var c = Compile(query);

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
            var query = new Query("Books").AsInsert(
                new[] { "Id", "Author", "ISBN", "Date" },
                new object[] { 1, "Author 1", "123456", null }
            );

            var c = Compile(query);

            Assert.Equal("INSERT INTO [Books] ([Id], [Author], [ISBN], [Date]) VALUES (1, 'Author 1', '123456', NULL)",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DATE\") VALUES (1, 'Author 1', '123456', NULL)",
                c[EngineCodes.Firebird]);
        }

        [Fact]
        public void InsertWithEmptyString()
        {
            var query = new Query("Books").AsInsert(
                new[] { "Id", "Author", "ISBN", "Description" },
                new object[] { 1, "Author 1", "123456", "" }
            );

            var c = Compile(query);

            Assert.Equal(
                "INSERT INTO [Books] ([Id], [Author], [ISBN], [Description]) VALUES (1, 'Author 1', '123456', '')",
                c[EngineCodes.SqlServer]);


            Assert.Equal(
                "INSERT INTO \"BOOKS\" (\"ID\", \"AUTHOR\", \"ISBN\", \"DESCRIPTION\") VALUES (1, 'Author 1', '123456', '')",
                c[EngineCodes.Firebird]);
        }
    }
}