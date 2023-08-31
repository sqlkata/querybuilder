using FluentAssertions;

namespace SqlKata.Tests;

public class HelperTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("   ")]
    public void ItShouldKeepItAsIs(string input)
    {
        var output = BindingExtensions.ReplaceAll(input, "any", x => x + "");

        Assert.Equal(input, output);
    }

    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("?hello", "@hello")]
    [InlineData("??hello", "@@hello")]
    [InlineData("?? hello", "@@ hello")]
    [InlineData("? ? hello", "@ @ hello")]
    [InlineData(" ? ? hello", " @ @ hello")]
    public void ReplaceOnTheBeginning(string input, string expected)
    {
        var output = BindingExtensions.ReplaceAll(input, "?", _ => "@");
        Assert.Equal(expected, output);
    }

    [Theory]
    [InlineData("hello?", "hello@")]
    [InlineData("hello? ", "hello@ ")]
    [InlineData("hello??? ", "hello@@@ ")]
    [InlineData("hello ? ?? ? ", "hello @ @@ @ ")]
    public void ReplaceOnTheEnd(string input, string expected)
    {
        var output = BindingExtensions.ReplaceAll(input, "?", _ => "@");
        Assert.Equal(expected, output);
    }

    [Theory]
    [InlineData("hello?", "hello0")]
    [InlineData("hello? ", "hello0 ")]
    [InlineData("hello??? ", "hello012 ")]
    [InlineData("hel?lo ? ?? ? ", "hel0lo 1 23 4 ")]
    [InlineData("????", "0123")]
    public void ReplaceWithPositions(string input, string expected)
    {
        var output = BindingExtensions.ReplaceAll(input, "?", x => x + "");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Flatten_FlatOneLevel()
    {
        // 3 levels
        var objects = new object[] { 1, new object[] { 2, 3, new[] { 4, 5, 6 } } };

        // 2 levels
        objects.FlattenOneLevel().Should().BeEquivalentTo(
            new object[]{1, 2, 3, new[] { 4, 5, 6 }});

        // 3 levels
        objects.FlattenOneLevel().FlattenOneLevel().Should().BeEquivalentTo(
            new []{1, 2, 3, 4, 5, 6 });
    }

    [Theory]
    [InlineData("Users.Id", "Users.Id")]
    [InlineData("Users.{Id", "Users.{Id")]
    [InlineData("Users.{Id}", "Users.Id")]
    [InlineData("Users.{Id,Name}", "Users.Id, Users.Name")]
    [InlineData("Users.{Id,Name, Last_Name }", "Users.Id, Users.Name, Users.Last_Name")]
    public void ExpandExpression(string input, string expected)
    {
        Assert.Equal(expected, string.Join(", ", Helper.ExpandExpression(input)));
    }

    [Fact]
    public void ExpandParameters()
    {
        var expanded = BindingExtensions.ExpandParameters("where id = ? or id in (?) or id in (?)", "?",
            new object[] { 1, new[] { 1, 2 }, new object[] { } });

        Assert.Equal("where id = ? or id in (?,?) or id in ()", expanded);
    }

    [Theory]
    [InlineData(@"\{ text {", @"\", "{", "[", "{ text [")]
    [InlineData(@"{ text {", @"\", "{", "[", "[ text [")]
    public void WrapIdentifiers(string input, string escapeCharacter, string identifier, string newIdentifier,
        string expected)
    {
        var result = input.ReplaceIdentifierUnlessEscaped(escapeCharacter, identifier, newIdentifier);
        Assert.Equal(expected, result);
    }
}
