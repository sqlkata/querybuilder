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
    private Query query;

    public Compiler compiler;

    [Params(
        EngineCodes.Firebird,
        EngineCodes.MySql,
        EngineCodes.Oracle,
        EngineCodes.PostgreSql,
        EngineCodes.Sqlite,
        EngineCodes.SqlServer)]
    public string EngineCode { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        query = new Query("Products")
            .Select("ProductID", "ProductName", "SupplierID", "CategoryID", "UnitPrice", "UnitsInStock", "UnitsOnOrder",
                "ReorderLevel", "Discontinued")
            .WhereContains("ProductName", "Mascarpone")
            .Where("UnitPrice", ">=", 10)
            .Where("UnitPrice", "<=", 100)
            .Take(10)
            .Skip(20)
            .OrderBy("UnitPrice", "ProductName");
        compiler = TestSupport.CreateCompiler(EngineCode);
    }

    [Benchmark]
    public SqlResult Select()
    {
        return compiler.Compile(query);
    }

    public static void Test()
    {
        var benchmark = new SelectsBenchmark();
        benchmark.EngineCode = EngineCodes.SqlServer;
        benchmark.Setup();
        var result = benchmark.Select().ToString();
        if (result !=
            Regex.Replace("""
            SELECT [ProductID], [ProductName], [SupplierID], [CategoryID], [UnitPrice], [UnitsInStock],
                [UnitsOnOrder], [ReorderLevel], [Discontinued]
            FROM [Products]
            WHERE LOWER([ProductName]) like '%mascarpone%'
                AND [UnitPrice] >= 10
                AND [UnitPrice] <= 100
            ORDER BY [UnitPrice], [ProductName] OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
            """, @"\s+", " "))
        {
            throw new ValidationException($"Invalid result: {result}");
        }
    }
}
