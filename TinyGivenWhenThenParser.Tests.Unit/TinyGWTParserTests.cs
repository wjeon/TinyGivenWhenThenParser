using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using TinyGivenWhenThenParser.Exceptions;
using TinyGivenWhenThenParser.Extensions;

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

        [Test]
        public void ParseSingleLine_parses_with_generic_type()
        {
            const string @case = "Given Tom has 2 apples and 3 oranges";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"^Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)$")
                .ParseSingleLine<TestData>();

            var expectedResult = new ParseResult<TestData>(true,
                new TestData
                    {
                        Name = "Tom",
                        Fruits = new Dictionary<string, int>
                        {
                            { "Apple", 2 },
                            { "Orange", 3 }
                        }
                    });

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void ParseMultiLines_parses_with_generic_type()
        {
            const string @case = @"Given Tom has 2 apples and 3 oranges
And Jerry has 1 apple and 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"^Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)$")
                .ParseMultiLines<TestData>();

            var expectedResult = new ParseResult<List<TestData>>(true, new List<TestData> {
                new TestData
                {
                    Name = "Tom",
                    Fruits = new Dictionary<string, int>
                        {
                            { "Apple", 2 },
                            { "Orange", 3 }
                        }
                },
                new TestData
                {
                    Name = "Jerry",
                    Fruits = new Dictionary<string, int>
                        {
                            { "Apple", 1 },
                            { "Orange", 1 }
                        }
                }});

            parseResult.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void Strings_in_single_line_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                .To("Name".As<string>(), "Quantity".As<int>(), "FavoriteFruit".As<Fruit>(), "Date".As<DateTime>(), "Time".As<TimeSpan>(), "DateTimeOffset".As<DateTimeOffset>(), "Id".As<Guid>(), "HourOfDay".As<HourOfDay>())
                .ParseSingleLine();
            var data = parseResult.Data;

            var expected = new Dictionary<string, object>
            {
                { "Name", "Tom" },
                { "Quantity", 2 },
                { "FavoriteFruit", Fruit.Apple },
                { "Date", DateTime.Parse("2017/1/10") },
                { "Time", TimeSpan.Parse("10:35:17") },
                { "DateTimeOffset", DateTimeOffset.Parse("2017-01-10T17:30:21-08:00") },
                { "Id", Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A") },
                {"HourOfDay", new HourOfDay("3pm") }
            };

            ((object)data).ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void Strings_in_multi_lines_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm
And test data - Name: Jerry, Age: 1, Favorite Fruit: orange, Date: 2017/2/1, Time: 05:42:02, DateTimeOffset: 2017-02-01T06:51:16-08:00, ID: B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F, Hour of day: 11am";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                .To("Name".As<string>(), "Quantity".As<int>(), "FavoriteFruit".As<Fruit>(), "Date".As<DateTime>(), "Time".As<TimeSpan>(), "DateTimeOffset".As<DateTimeOffset>(), "Id".As<Guid>(), "HourOfDay".As<HourOfDay>())
                .ParseMultiLines();
            var data = parseResult.Data.ToArray();

            var expected = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Name", "Tom" },
                    { "Quantity", 2 },
                    { "FavoriteFruit", Fruit.Apple },
                    { "Date", DateTime.Parse("2017/1/10") },
                    { "Time", TimeSpan.Parse("10:35:17") },
                    { "DateTimeOffset", DateTimeOffset.Parse("2017-01-10T17:30:21-08:00") },
                    { "Id", Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A") },
                    { "HourOfDay", new HourOfDay("3pm") }
                },
                new Dictionary<string, object>
                {
                    { "Name", "Jerry" },
                    { "Quantity", 1 },
                    { "FavoriteFruit", Fruit.Orange },
                    { "Date", DateTime.Parse("2017/2/1") },
                    { "Time", TimeSpan.Parse("05:42:02") },
                    { "DateTimeOffset", DateTimeOffset.Parse("2017-02-01T06:51:16-08:00") },
                    { "Id", Guid.Parse("B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F") },
                    { "HourOfDay", new HourOfDay("11am") }
                }
            };

            ((object)parseResult.Data).ShouldBeEquivalentTo(expected);
        }

        private enum Fruit
        {
            Apple,
            Orange
        }
    }
}
