using System.Linq;
using System.Text.RegularExpressions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests;

public class BindingsTests : TestSupport
{
    private static readonly Regex _parameterExtractor = new(@"(@\w+\b)", RegexOptions.Compiled);

    [Fact]
    public void BindingsAreAppliedFromWhere()
    {
        var q = new Query("Table")
            .Where("Id", 1)
            .Where("Name", "John");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            1,
            "John");
    }

    [Fact]
    public void BindingsAreAppliedFromTake()
    {
        var q = new Query("Table")
            .Take(10);

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            10);
    }

    [Fact]
    public void BindingsAreAppliedFromSkip()
    {
        var q = new Query("Table")
            .Skip(10);

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            10L);
    }

    [Fact]
    public void BindingsAreAppliedFromTakeAndSkip()
    {
        var q = new Query("Table")
            .Take(10)
            .Skip(20);

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            10,
            20L);
    }

    [Fact]
    public void BindingsAreAppliedFromOrderByRaw()
    {
        var q = new Query("Table")
            .OrderByRaw("?, ?", 1, "John");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            1,
            "John");
    }

    [Fact]
    public void BindingsAreAppliedFromHaving()
    {
        var q = new Query("Table")
            .Having("Id", 1)
            .Having("Name", "John");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            1,
            "John");
    }

    [Fact]
    public void BindingsAreAppliedFromWith()
    {
        var q = new Query("Table")
            .With("WithAlias", ["Id", "Name"], [[1, "First"], [2, "Second"]]);

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            1,
            "First",
            2,
            "Second");
    }

    [Fact]
    public void BindingsAreAppliedFromCombineRaw()
    {
        var q = new Query("Table")
            .CombineRaw("UNION SELECT * FROM Table2 Id = ? AND Name = ?", 1, "John");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            1,
            "John");
    }

    [Fact]
    public void BindingsAreAppliedFromHavingBetween()
    {
        var q = new Query("Table")
            .HavingBetween("Id", 10, 20);

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            10,
            20);
    }

    [Fact]
    public void BindingsAreAppliedFromHavingContains()
    {
        // Contains is object but it works only with string.
        var q = new Query("Table")
            .HavingContains("Name", "John");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            "%john%");
    }

    [Fact]
    public void BindingsAreAppliedFromHavingEnds()
    {
        // Contains is object but it works only with string.
        var q = new Query("Table")
            .HavingEnds("Name", "John");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            "%john");
    }

    [Fact]
    public void BindingsAreAppliedFromHavingStarts()
    {
        var q = new Query("Table")
            .HavingStarts("Name", "John");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            "john%");
    }

    [Fact]
    public void BindingsAreAppliedFromHavingLike()
    {
        var q = new Query("Table")
            .HavingLike("Name", "John%Wick");

        var result = CompileForGeneric(q);

        AssertBindingsAndNamedBindings(result,
            "john%wick");
    }

    private static void AssertBindingsAndNamedBindings(SqlResult result, params object[] parameters)
    {
        Assert.Equal(parameters.Length, result.Bindings.Count);
        foreach (var values in parameters.Zip(result.Bindings))
        {
            Assert.Equal(values.First, values.Second);
        }

        Assert.Equal(parameters.Length, result.NamedBindings.Count);
        var parameterNames = _parameterExtractor
            .Matches(result.Sql)
            .Select(e => e.Value)
            .ToArray();
        Assert.Equal(parameters.Length, parameterNames.Length);
        foreach (var parameter in parameterNames.Zip(parameters))
        {
            Assert.Equal(parameter.Second, result.NamedBindings[parameter.First]);
        }
    }
}
