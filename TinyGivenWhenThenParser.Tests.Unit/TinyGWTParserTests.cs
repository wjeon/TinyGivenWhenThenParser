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

            var parseResult = gwtParser.WithPattern(@"^Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)$")
                .ParseSingleLine();

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

            var parseResult = gwtParser.WithPattern(@"When (.*) eats (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseSingleLine();

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

            var parseResult = gwtParser.WithPattern(@"^Then (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseSingleLine();

            var expectedData = string.IsNullOrEmpty(parsedData) ? new string[0] : parsedData.Split(',');

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [Test]
        public void When_test_case_has_multi_lines_ParseSingleLine_method_parses_data_from_the_first_matching_line()
        {
            const string multilineCase = @"not matching line
Given Tom has 3 apples
Given Jerry has 1 orange";
            var expectedData = new[] { "Tom", "3", "apple", "s"};

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) (apple|orange)(s|)")
                .ParseSingleLine();

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [Test]
        public void When_test_case_has_multi_lines_ParseMultiLines_method_parses_data_from_all_matching_lines()
        {
            const string multilineCase = @"not matching line
Given Tom has 3 apples
Given Jerry has 1 orange";
            var expectedData = new[]
                {
                    new[] { "Tom", "3", "apple", "s" },
                    new[] { "Jerry", "1", "orange", "" }
                };

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) (apple|orange)(s|)")
                .ParseMultiLines();

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_Given_and_the_line_is_matched_with_the_pattern_for_Given_if_And_line_is_after_Given_line()
        {
            const string multilineCase = @"Given Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange";
            var expectedData = new[]
                {
                    new[] { "Tom", "2", "s", "3", "s" },
                    new[] { "Jerry", "1", "", "1", "" }
                };

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)")
                .ParseMultiLines(From.TestCaseReplacedAndWithGivenWhenThen);

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_When_and_the_line_is_matched_with_the_pattern_for_When_if_And_line_is_after_When_line()
        {
            const string multilineCase = @"When Tom eats 1 apple and 2 oranges
And Jerry eats 1 orange";
            var expectedData = new[]
                {
                    new[] { "Tom", "1", " apple", "", " and ", "2", " orange", "s" },
                    new[] { "Jerry", "", "", "", "","1", " orange", "" }
                };

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"When (.*) eats (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseMultiLines();

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_Then_and_the_line_is_matched_with_the_pattern_for_Then_if_And_line_is_after_Then_line()
        {
            const string multilineCase = @"Then Tom has 1 apple and 1 orange
And Jerry has 1 apple";
            var expectedData = new[]
                {
                    new[] { "Tom", "1", " apple", "", " and ", "1", " orange", "" },
                    new[] { "Jerry", "1", " apple", "", "", "", "", "" }
                };

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Then (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseMultiLines();

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }

        [TestCase(@"Given Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange", @"Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)", "Tom,2,s,3,s")]
        [TestCase(@"When Tom eats 1 apple and 2 oranges
And Jerry eats 1 orange", @"When (.*) eats (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)", "Tom,1, apple,, and ,2, orange,s")]
        [TestCase(@"Then Tom has 1 apple and 1 orange
And Jerry has 1 apple", @"Then (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)", "Tom,1, apple,, and ,1, orange,")]
        [TestCase(@"Then Tom has 1 apple and 1 orange
And Jerry has 1 apple", @"And (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)", "Jerry,1, apple,,,,,")]
        public void Leading_And_in_the_line_is_not_replaced_and_the_line_is_matched_with_the_pattern_for_And_when_OriginalTestCase_option_is_selected
            (string @case, string pattern, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var singleLineParseResult = gwtParser.WithPattern(pattern)
                .ParseSingleLine(From.OriginalTestCase);

            singleLineParseResult.ShouldAllBeEquivalentTo(parsedData.Split(','));

            var multiLineParseResult = gwtParser.WithPattern(pattern)
                .ParseMultiLines(From.OriginalTestCase);

            multiLineParseResult.ShouldAllBeEquivalentTo(new[] { parsedData.Split(',') });
        }
    }
}
