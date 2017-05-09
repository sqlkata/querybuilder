using System;
using Xunit;

namespace SqlKata.Tests
{
    public class HelperTest
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("   ")]
        public void ItShouldKeepItAsIs(string input)
        {
            var output = Helper.ReplaceAll(input, "any", x => x + "");

            Assert.Equal(input, output);
        }

        [Theory]
        [InlineData("hello", "hello")]
        [InlineData("?hello", "@hello")]
        [InlineData("??hello", "@@hello")]
        [InlineData("?? hello", "@@ hello")]
        [InlineData("? ? hello", "@ @ hello")]
        [InlineData(" ? ? hello", " @ @ hello")]
        public void ReplaceOnTheBegining(string input, string expected)
        {
            var output = Helper.ReplaceAll(input, "?", x => "@");
            Assert.Equal(expected, output);
        }

        [Theory]
        [InlineData("hello?", "hello@")]
        [InlineData("hello? ", "hello@ ")]
        [InlineData("hello??? ", "hello@@@ ")]
        [InlineData("hello ? ?? ? ", "hello @ @@ @ ")]
        public void ReplaceOnTheEnd(string input, string expected)
        {
            var output = Helper.ReplaceAll(input, "?", x => "@");
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
            var output = Helper.ReplaceAll(input, "?", x => x + "");
            Assert.Equal(expected, output);
        }
    }
}
