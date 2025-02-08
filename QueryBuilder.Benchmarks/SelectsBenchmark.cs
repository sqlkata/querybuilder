using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using QueryBuilder.Benchmarks.Infrastructure;
using SqlKata;
using SqlKata.Compilers;

namespace QueryBuilder.Benchmarks;

[MemoryDiagnoser]
public class SelectsBenchmark
{
    private Query selectSimple;
    private Query selectGroupBy;
    private Query selectWith;

    public Compiler compiler;

    [Params(
        EngineCodes.SqlServer)]
    public string EngineCode { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        selectSimple = new Query("Products")
            .Select("ProductID", "ProductName", "SupplierID", "CategoryID", "UnitPrice", "UnitsInStock", "UnitsOnOrder",
                "ReorderLevel", "Discontinued")
            .WhereIn("CategoryID", [1, 2, 3])
            .Where("SupplierID", 5)
            .Where("UnitPrice", ">=", 10)
            .Where("UnitPrice", "<=", 100)
            .Take(10)
            .Skip(20)
            .OrderBy("UnitPrice", "ProductName");


        selectGroupBy = new Query("Products")
            .Select("SupplierID", "CategoryID")
            .SelectAvg("UnitPrice")
            .SelectMin("UnitPrice")
            .SelectMax("UnitPrice")
            .Where("CategoryID", 123)
            .GroupBy("SupplierID", "CategoryID")
            .HavingRaw("MIN(UnitPrice) >= ?", 10)
            .Take(10)
            .Skip(20)
            .OrderBy("SupplierID", "CategoryID");

        var activePosts = new Query("Comments")
            .Select("PostId")
            .SelectRaw("count(1) as Count")
            .GroupBy("PostId")
            .HavingRaw("count(1) > 100");

        selectWith = new Query("Posts")
            .With("ActivePosts", activePosts)
            .Join("ActivePosts", "ActivePosts.PostId", "Posts.Id")
            .Select("Posts.*", "ActivePosts.Count");

        compiler = TestSupport.CreateCompiler(EngineCode);
    }

    [Benchmark]
    public SqlResult SelectSimple()
    {
        return compiler.Compile(selectSimple);
    }

    [Benchmark]
    public SqlResult SelectGroupBy()
    {
        return compiler.Compile(selectGroupBy);
    }

    [Benchmark]
    public SqlResult SelectWith()
    {
        return compiler.Compile(selectWith);
    }

}
