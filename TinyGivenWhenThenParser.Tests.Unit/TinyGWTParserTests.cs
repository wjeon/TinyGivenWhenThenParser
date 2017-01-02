using FluentAssertions;
using NUnit.Framework;

namespace TinyGivenWhenThenParser.Tests.Unit
{
    [TestFixture]
    public class TinyGWTParserTests
    {
        [TestCase(@"Given Tom has 2 apples and 3 oranges", true, "Tom,2,s,3,s")]
        [TestCase(@"Given Jerry has 1 apple and 1 orange", true, "Jerry,1,,1,")]
        [TestCase(@"When Jerry has 1 apple and 1 orange", false, "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_Given_when_the_sentence_starts_with_Given
            (string @case, bool parsed, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"^Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)$")
                .ParseSingleLine();

            var expectedResult = new ParseResult<string[]>(parsed,
                string.IsNullOrEmpty(parsedData) ? new string[0] : parsedData.Split(','));

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [TestCase(@"When Tom eats 1 apple and 2 oranges", true, "Tom,1, apple,, and ,2, orange,s")]
        [TestCase(@"When Jerry eats 1 orange", true, "Jerry,,,,,1, orange,")]
        [TestCase(@"Given Jerry eats 1 orange", false, "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_When_when_the_sentence_starts_with_When
            (string @case, bool parsed, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"When (.*) eats (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseSingleLine();

            var expectedResult = new ParseResult<string[]>(parsed,
                string.IsNullOrEmpty(parsedData) ? new string[0] : parsedData.Split(','));

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [TestCase(@"When ignore this line
Then Tom has 1 apple and 1 orange", true, "Tom,1, apple,, and ,1, orange,")]
        [TestCase(@"When ignore this line
Then Jerry has 1 apple", true, "Jerry,1, apple,,,,,")]
        [TestCase(@"Given Jerry has 1 apple", false, "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_Then_when_the_sentence_starts_with_Then
            (string @case, bool parsed, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"^Then (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseSingleLine();

            var expectedResult = new ParseResult<string[]>(parsed,
                string.IsNullOrEmpty(parsedData) ? new string[0] : parsedData.Split(','));

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void When_test_case_has_multi_lines_ParseSingleLine_method_parses_data_from_the_first_matching_line()
        {
            const string multilineCase = @"Given not matching line
Given Tom has 3 apples
Given Jerry has 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) (apple|orange)(s|)")
                .ParseSingleLine();

            var expectedResult = new ParseResult<string[]>(true, new[] { "Tom", "3", "apple", "s" });

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void When_test_case_has_multi_lines_ParseMultiLines_method_parses_data_from_all_matching_lines()
        {
            const string multilineCase = @"Given not matching line
Given Tom has 3 apples
Given Jerry has 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) (apple|orange)(s|)")
                .ParseMultiLines();

            var expectedResult = new ParseResult<string[][]>(true, new[]
                {
                    new[] { "Tom", "3", "apple", "s" },
                    new[] { "Jerry", "1", "orange", "" }
                });

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_Given_and_the_line_is_matched_with_the_pattern_for_Given_if_And_line_is_after_Given_line()
        {
            const string multilineCase = @"Given Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)")
                .ParseMultiLines(From.TestCaseReplacedAndWithGivenWhenThen);

            var expectedResult = new ParseResult<string[][]>(true, new[]
                {
                    new[] { "Tom", "2", "s", "3", "s" },
                    new[] { "Jerry", "1", "", "1", "" }
                });

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_When_and_the_line_is_matched_with_the_pattern_for_When_if_And_line_is_after_When_line()
        {
            const string multilineCase = @"When Tom eats 1 apple and 2 oranges
And Jerry eats 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"When (.*) eats (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseMultiLines();

            var expectedResult = new ParseResult<string[][]>(true, new[]
                {
                    new[] { "Tom", "1", " apple", "", " and ", "2", " orange", "s" },
                    new[] { "Jerry", "", "", "", "","1", " orange", "" }
                });

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_Then_and_the_line_is_matched_with_the_pattern_for_Then_if_And_line_is_after_Then_line()
        {
            const string multilineCase = @"When ignore this line
Then Tom has 1 apple and 1 orange
And Jerry has 1 apple";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Then (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)")
                .ParseMultiLines();

            var expectedResult = new ParseResult<string[][]>(true, new[]
                {
                    new[] { "Tom", "1", " apple", "", " and ", "1", " orange", "" },
                    new[] { "Jerry", "1", " apple", "", "", "", "", "" }
                });

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [TestCase(@"Given Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange", @"Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)", "Tom,2,s,3,s")]
        [TestCase(@"When Tom eats 1 apple and 2 oranges
And Jerry eats 1 orange", @"When (.*) eats (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)", "Tom,1, apple,, and ,2, orange,s")]
        [TestCase(@"When ignore this line
Then Tom has 1 apple and 1 orange
And Jerry has 1 apple", @"Then (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)", "Tom,1, apple,, and ,1, orange,")]
        [TestCase(@"When ignore this line
Then Tom has 1 apple and 1 orange
And Jerry has 1 apple", @"And (.*) has (|\d+)( apple|)(s|)(| and )(|\d+)( orange|)(s|)", "Jerry,1, apple,,,,,")]
        public void Leading_And_in_the_line_is_not_replaced_and_the_line_is_matched_with_the_pattern_for_And_when_OriginalTestCase_option_is_selected
            (string @case, string pattern, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var singleLineParseResult = gwtParser.WithPattern(pattern)
                .ParseSingleLine(From.OriginalTestCase);

            singleLineParseResult.ShouldBeEquivalentTo(new ParseResult<string[]>(true, parsedData.Split(',')));

            var multiLineParseResult = gwtParser.WithPattern(pattern)
                .ParseMultiLines(From.OriginalTestCase);

            multiLineParseResult.ShouldBeEquivalentTo(new ParseResult<string[][]>(true, new[] {parsedData.Split(',')}));
        }

        [TestCase(@"Case #1: Given Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange")]
        [TestCase(@"Then Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange")]
        [TestCase(@"And Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange")]
        public void When_a_test_case_begins_with_other_than_Given_and_When_it_throws(string @case)
        {
            Assert.Throws<GwtParserException>(() => TinyGWTParser.WithTestCase(@case));
        }

        [Test]
        public void When_a_test_case_contains_line_that_begins_with_other_than_And_Given_When_Then_it_throws()
        {
            var @case = @"Given Tom has 2 apples and 3 oranges
Also Jerry has 1 apple and 1 orange";

            Assert.Throws<GwtParserException>(() => TinyGWTParser.WithTestCase(@case));
        }
    }
}
