using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using SqlKata;
using SqlKata.Compilers;

namespace QueryBuilder.Benchmarks;

public static partial class SelectsBenchmarkTests
{
    public static void TestAll()
    {
        TestSelectSimple();
        TestSelectGroupBy();
        TestSelectWith();
    }

    public static void TestSelectSimple()
    {
        var benchmark = CreateBenchmark();

        var result = benchmark.SelectSimple();

        // language=SQL
        ValidateResult(
            """
            SELECT [ProductID], [ProductName], [SupplierID], [CategoryID], [UnitPrice],
                [UnitsInStock], [UnitsOnOrder], [ReorderLevel], [Discontinued]
            FROM [Products]
            WHERE [CategoryID] IN (1, 2, 3)
              AND [SupplierID] = 5
              AND [UnitPrice] >= 10
              AND [UnitPrice] <= 100
            ORDER BY [UnitPrice], [ProductName]
            OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
            """, result);
    }

    public static void TestSelectGroupBy()
    {
        var benchmark = CreateBenchmark();

        var result = benchmark.SelectGroupBy();

        // language=SQL
        ValidateResult(
            """
            SELECT [SupplierID], [CategoryID],
                AVG([UnitPrice]), MIN([UnitPrice]), MAX([UnitPrice])
            FROM [Products]
            WHERE [CategoryID] = 123
            GROUP BY [SupplierID], [CategoryID]
            HAVING MIN(UnitPrice) >= 10
            ORDER BY [SupplierID], [CategoryID]
            OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
            """, result);
    }

    public static void TestSelectWith()
    {
        var benchmark = CreateBenchmark();

        var result = benchmark.SelectWith();

        // language=SQL
        ValidateResult(
            """
            WITH [ActivePosts] AS (SELECT [PostId], count(1) as Count FROM [Comments] GROUP BY [PostId] HAVING count(1) > 100)
            SELECT [Posts].*, [ActivePosts].[Count]
            FROM [Posts]
            INNER JOIN [ActivePosts] ON [ActivePosts].[PostId] = [Posts].[Id]
            """, result);
    }

    private static SelectsBenchmark CreateBenchmark()
    {
        var benchmark = new SelectsBenchmark
        {
            EngineCode = EngineCodes.SqlServer
        };
        benchmark.Setup();
        return benchmark;
    }

    private static void ValidateResult(string expected, SqlResult result)
    {
        var actual = result.ToString();
        if (WhiteSpaces().Replace(actual, " ") != WhiteSpaces().Replace(expected, " "))
        {
            throw new ValidationException($"Invalid result: {actual}");
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaces();
}
