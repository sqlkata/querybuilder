using System.Collections;
using SqlKata;
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

        [Fact]
        public void AllIndexesOf_ReturnIndexes_IfValueIsContainedInAString()
        {
            // Given
            var input = "hello";

            // When
            var result = Helper.AllIndexesOf(input, "l");

            // Then
            Assert.Equal(new[] {2, 3}, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AllIndexesOf_ReturnEmptyCollection_IfValueIsEmptyOrNull(string value)
        {
            // Given
            var input = "hello";

            // When
            var result = Helper.AllIndexesOf(input, value);

            // Then
            Assert.Empty(result);
        }

        [Fact]
        public void AllIndexesOf_ReturnEmptyCollection_IfValueIsNotContainedInAString()
        {
            // Given
            var input = "hello";

            // When
            var result = Helper.AllIndexesOf(input, "F");

            // Then
            Assert.Empty(result);
        }

        [Fact]
        public void Flatten_ReturnFlatttenCollecitonRecursivity_IfArrayIsNested()
        {
            // Given
            var objects = new object[]
            {
                1,
                0.1,
                'A',
                new object[]
                {
                    'A',
                    "B",
                    new object[]
                    {
                        "C",
                        'D'
                    }
                }
            };

            // When
            var flatten = Helper.Flatten(objects);

            // Then
            Assert.Equal(new object[] {1, 0.1, 'A', 'A', "B", "C", 'D'}, flatten);
        }

        [Fact]
        public void IsArray_ReturnFalse_IfValueIsNull()
        {
            // Given
            IEnumerable test = null;

            // When
            var isArray = Helper.IsArray(test);

            // Then
            Assert.False(isArray);
        }

        [Fact]
        public void IsArray_ReturnFlase_IfTypeOfValueIsString()
        {
            // Given
            var value = "string";

            // When
            var isArray = Helper.IsArray(value);

            // Then
            Assert.False(isArray);
        }

        [Fact]
        public void IsArray_ReturnTrue_IfValueIsExactlyIEnuerable()
        {
            // Given
            var value = new object[] {1, 'B', "C"};

            // When
            var isArray = Helper.IsArray(value);

            // Then
            Assert.True(isArray);
        }
    }
}