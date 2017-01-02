using FluentAssertions;
using NUnit.Framework;

namespace TinyGivenWhenThenParser.Tests.Unit
{
    [TestFixture]
    public class TinyGWTParserTests
    {
        [TestCase(@"Given Tom has 2 apples and 3 oranges", "Tom,2,s,3,s")]
        [TestCase(@"Given Jerry has 1 apple and 1 orange", "Jerry,1,,1,")]
        [TestCase(@"When Jerry has 1 apple and 1 orange", "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_Given_when_the_sentence_starts_with_Given
            (string @case, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            const string pattern = @"^Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)$";

            var parseResult = gwtParser.WithPattern(pattern).ParseData();

            var expectedData = string.IsNullOrEmpty(parsedData) ? new string[0] : parsedData.Split(',');

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [TestCase(@"When Tom eats 1 apple and 2 oranges", "Tom,1, apple,, and ,2, orange,s")]
        [TestCase(@"When Jerry eats 1 orange", "Jerry,,,,,1, orange,")]
        [TestCase(@"Given Jerry eats 1 orange", "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_When_when_the_sentence_starts_with_When
            (string @case, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            const string pattern = @"^When (.*) eats (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)$";

            var parseResult = gwtParser.WithPattern(pattern).ParseData();

            var expectedData = string.IsNullOrEmpty(parsedData) ? new string[0] : parsedData.Split(',');

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [TestCase(@"Then Tom has 1 apple and 1 orange", "Tom,1, apple,, and ,1, orange,")]
        [TestCase(@"Then Jerry has 1 apple", "Jerry,1, apple,,,,,")]
        [TestCase(@"Given Jerry has 1 apple", "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_Then_when_the_sentence_starts_with_Then
            (string @case, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            const string pattern = @"^Then (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)$";

            var parseResult = gwtParser.WithPattern(pattern).ParseData();

            var expectedData = string.IsNullOrEmpty(parsedData) ? new string[0] : parsedData.Split(',');

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }
    }
}
