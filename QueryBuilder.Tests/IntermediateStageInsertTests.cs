using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests;

public class IntermediateStageInsertTests : TestSupport
{
    [Fact]
    public void InsertObject()
    {
        CompareWithCompiler(new Query("Table")
            .AsInsert(
                new
                {
                    Name = "The User",
                    Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }));
    }
    [Fact]
    public void InsertFromSubQueryWithCte()
    {
        CompareWithCompiler(new Query("expensive_cars")
            .With("old_cards", new Query("all_cars").Where("year", "<", 2000))
            .AsInsert(
                new[] { "name", "model", "year" },
                new Query("old_cars").Where("price", ">", 100).ForPage(2, 10)));
    }
    [Fact]
    public void InsertMultiRecords()
    {
        CompareWithCompiler(new Query("expensive_cars")
            .AsInsert(
                new[] { "name", "brand", "year" },
                new[]
                {
                    new object?[] { "Chiron", "Bugatti", null },
                    new object?[] { "Huayra", "Pagani", 2012 },
                    new object?[] { "Reventon roadster", "Lamborghini", 2009 }
                }));
    }
    
    [Fact]
    public void InsertWithNullValues()
    {
        CompareWithCompiler(new Query("Books")
            .AsInsert(
                new[] { "Id", "Author", "ISBN", "Date" },
                new object?[] { 1, "Author 1", "123456", null }));
    }
    [Fact]
    public void InsertWithEmptyString()
    {
        CompareWithCompiler(new Query("Books")
            .AsInsert(
                new[] { "Id", "Author", "ISBN", "Description" },
                new object[] { 1, "Author 1", "123456", "" }));
    }
    [Fact]
    public void InsertWithByteArray()
    {
        CompareWithCompiler(new Query("Books")
            .AsInsert(
                new[] { "Id", "CoverImageBytes" },
                new object[]
                {
                    1,
                    new byte[] { 0x1, 0x3, 0x3, 0x7 }
                }));
    }
    [Fact]
    public void InsertFromQueryShouldFail()
    {
        CompareWithCompiler(new Query()
            .From(new Query("InnerTable"))
            .AsInsert(
                new
                {
                    Name = "The User",
                    Age = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }));

    }

}
