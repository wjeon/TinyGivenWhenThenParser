﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using TinyGivenWhenThenParser.Exceptions;
using TinyGivenWhenThenParser.Extensions;
using TinyGivenWhenThenParser.Results;

namespace TinyGivenWhenThenParser.NetCore.Tests.Unit
{
    [TestFixture]
    public class TinyGWTParserNetCoreTests
    {
        [TestCase(@"Given Tom has 2 apples and 3 oranges", true, "Tom,2,3")]
        [TestCase(@"Given Jerry has 1 apple and 1 orange", true, "Jerry,1,1")]
        [TestCase(@"When Jerry has 1 apple and 1 orange", false, "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_Given_when_the_sentence_starts_with_Given
            (string @case, bool parsed, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"^Given (.*) has (\d+) apple(?:s|) and (\d+) orange(?:s|)$")
                .ParseSingleLine();

            var expectedResult = new ParseResult<ParsedData<string[], IEnumerable<IEnumerable<string>>>, string[]>(
                parsed,
                new ParsedData<string[], IEnumerable<IEnumerable<string>>>(
                    string.IsNullOrEmpty(parsedData) ? new string[] { } : parsedData.Split(','),
                    Enumerable.Empty<IEnumerable<string>>()
                ));

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        [TestCase(@"When Tom eats 1 apple and 2 oranges", true, "Tom,1, apple,2, orange")]
        [TestCase(@"When Jerry eats 1 orange", true, "Jerry,,,1, orange")]
        [TestCase(@"Given Jerry eats 1 orange", false, "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_When_when_the_sentence_starts_with_When
            (string @case, bool parsed, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"When (.*) eats (|\d+)( apple|)(?:s|)(?:| and )(|\d+)( orange|)(?:s|)")
                .ParseSingleLine();

            var expectedResult = new ParseResult<ParsedData<string[], IEnumerable<IEnumerable<string>>>, string[]>(
                parsed,
                new ParsedData<string[], IEnumerable<IEnumerable<string>>>(
                    string.IsNullOrEmpty(parsedData) ? new string[] { } : parsedData.Split(','),
                    Enumerable.Empty<IEnumerable<string>>()
                ));

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        [TestCase(@"When ignore this line
                    Then Tom has 1 apple and 1 orange",
            true, "Tom,1, apple,1, orange")]
        [TestCase(@"When ignore this line
                    Then Jerry has 1 apple",
            true, "Jerry,1, apple,,")]
        [TestCase(@"Given Jerry has 1 apple",
            false, "")]
        public void Parse_data_from_a_sentence_correctly_with_the_pattern_for_Then_when_the_sentence_starts_with_Then
            (string @case, bool parsed, string parsedData)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"^Then (.*) has (|\d+)( apple|)(?:s|)(?:| and )(|\d+)( orange|)(?:s|)")
                .ParseSingleLine();

            var expectedResult = new ParseResult<ParsedData<string[], IEnumerable<IEnumerable<string>>>, string[]>(
                parsed,
                new ParsedData<string[], IEnumerable<IEnumerable<string>>>(
                    string.IsNullOrEmpty(parsedData) ? new string[] { } : parsedData.Split(','),
                    Enumerable.Empty<IEnumerable<string>>()
                ));

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void When_test_case_has_multi_lines_ParseSingleLine_method_parses_data_from_the_first_matching_line()
        {
            const string multilineCase = @"Given not matching line
                                           Given Tom has 3 apples
                                           Given Jerry has 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) (apple|orange)(?:s|)")
                .ParseSingleLine();

            var expectedResult = new ParseResult<ParsedData<string[], IEnumerable<IEnumerable<string>>>, string[]>(
                true, new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Tom", "3", "apple" }, Enumerable.Empty<IEnumerable<string>>()));

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void When_test_case_has_multi_lines_ParseMultiLines_method_parses_data_from_all_matching_lines()
        {
            const string multilineCase = @"Given not matching line
                                           Given Tom has 3 apples
                                           Given Jerry has 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) (apple|orange)(?:s|)")
                .ParseMultiLines();

            var expectedResult = new ParseResult<IEnumerable<ParsedData<string[], IEnumerable<IEnumerable<string>>>>, string[][]>(
                true,
                new List<ParsedData<string[], IEnumerable<IEnumerable<string>>>>
                {
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Tom", "3", "apple" }, Enumerable.Empty<IEnumerable<string>>()),
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Jerry", "1", "orange" }, Enumerable.Empty<IEnumerable<string>>())
                });

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_Given_and_the_line_is_matched_with_the_pattern_for_Given_if_And_line_is_after_Given_line()
        {
            const string multilineCase = @"Given Tom has 2 apples and 3 oranges
                                           And Jerry has 1 apple and 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Given (.*) has (\d+) apple(?:s|) and (\d+) orange(?:s|)")
                .ParseMultiLines(From.TestCaseReplacedAndWithGivenWhenThen);

            var expectedResult = new ParseResult<IEnumerable<ParsedData<string[], IEnumerable<IEnumerable<string>>>>, string[][]>(
                true,
                new List<ParsedData<string[], IEnumerable<IEnumerable<string>>>>
                {
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Tom", "2", "3" }, Enumerable.Empty<IEnumerable<string>>()),
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Jerry", "1", "1" }, Enumerable.Empty<IEnumerable<string>>())
                });

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_When_and_the_line_is_matched_with_the_pattern_for_When_if_And_line_is_after_When_line()
        {
            const string multilineCase = @"When Tom eats 1 apple and 2 oranges
                                           And Jerry eats 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"When (.*) eats (|\d+)( apple|)(?:s|)(?:| and )(|\d+)( orange|)(?:s|)")
                .ParseMultiLines();

            var expectedResult = new ParseResult<IEnumerable<ParsedData<string[], IEnumerable<IEnumerable<string>>>>, string[][]>(
                true,
                new List<ParsedData<string[], IEnumerable<IEnumerable<string>>>>
                {
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Tom", "1", " apple", "2", " orange" }, Enumerable.Empty<IEnumerable<string>>()),
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Jerry", "", "","1", " orange" }, Enumerable.Empty<IEnumerable<string>>())
                });

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Leading_And_in_the_line_is_replaced_with_Then_and_the_line_is_matched_with_the_pattern_for_Then_if_And_line_is_after_Then_line()
        {
            const string multilineCase = @"When ignore this line
                                           Then Tom has 1 apple and 1 orange
                                           And Jerry has 1 apple";

            var gwtParser = TinyGWTParser.WithTestCase(multilineCase);

            var parseResult = gwtParser.WithPattern(@"Then (.*) has (|\d+)( apple|)(?:s|)(?:| and )(|\d+)( orange|)(?:s|)")
                .ParseMultiLines();

            var expectedResult = new ParseResult<IEnumerable<ParsedData<string[], IEnumerable<IEnumerable<string>>>>, string[][]>(
                true,
                new List<ParsedData<string[], IEnumerable<IEnumerable<string>>>>
                {
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Tom", "1", " apple", "1", " orange" }, Enumerable.Empty<IEnumerable<string>>()),
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(new[] { "Jerry", "1", " apple", "", "" }, Enumerable.Empty<IEnumerable<string>>())
                });

            parseResult.Should().BeEquivalentTo(expectedResult);
        }

        private static string OriginalTestCase_option_test_case_source = @"[
          {
            'case': 'Given Tom has 2 apples and 3 oranges
                     And Jerry has 1 apple and 1 orange',
            'pattern': 'Given (.*) has (\\d+) apple(?:s|) and (\\d+) orange(?:s|)',
            'parsedData': 'Tom,2,3',
            'description': 'Parse leading Given only -> Tom,2,3'
          },
          {
            'case': 'When Tom eats 1 apple and 2 oranges
                     And Jerry eats 1 orange',
            'pattern': 'When (.*) eats (|\\d+)( apple|)(?:s|)(?:| and )(|\\d+)( orange|)(?:s|)',
            'parsedData': 'Tom,1, apple,2, orange',
            'description': 'Parse leading When only -> Tom,1, apple,2, orange'
          },
          {
            'case': 'When ignore this line
                     Then Tom has 1 apple and 1 orange
                     And Jerry has 1 apple',
            'pattern': 'Then (.*) has (|\\d+)( apple|)(?:s|)(?:| and )(|\\d+)( orange|)(?:s|)',
            'parsedData': 'Tom,1, apple,1, orange',
            'description': 'Parse leading Then only -> Tom,1, apple,1, orange'
          },
          {
            'case': 'When ignore this line
                     Then Tom has 1 apple and 1 orange
                     And Jerry has 1 apple',
            'pattern': 'And (.*) has (|\\d+)( apple|)(?:s|)(?:| and )(|\\d+)( orange|)(?:s|)',
            'parsedData': 'Jerry,1, apple,,',
            'description': 'Parse leading And only -> Jerry,1, apple,,'
          }
        ]";

        private static object[] OriginalTestCase_option_case_ids =
            GetCaseIdsFrom(CasesFrom(OriginalTestCase_option_test_case_source), descriptionFieldName: "description");

        [Test, TestCaseSource("OriginalTestCase_option_case_ids")]
        public void Leading_And_in_the_line_is_not_replaced_and_the_line_is_matched_with_the_pattern_for_And_when_OriginalTestCase_option_is_selected
            (int caseId, string description)
        {
            var cases = CasesFrom(OriginalTestCase_option_test_case_source);

            var @case = cases[caseId]["case"];
            var pattern = cases[caseId]["pattern"];
            var parsedData = cases[caseId]["parsedData"];

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var singleLineParseResult = gwtParser.WithPattern(pattern)
                .ParseSingleLine(From.OriginalTestCase);

            var expectedSingleLineResult = new ParseResult<ParsedData<string[], IEnumerable<IEnumerable<string>>>, string[]>(
                true,
                new ParsedData<string[], IEnumerable<IEnumerable<string>>>(parsedData.Split(','), Enumerable.Empty<IEnumerable<string>>())
            );

            singleLineParseResult.Should().BeEquivalentTo(expectedSingleLineResult);

            var multiLineParseResult = gwtParser.WithPattern(pattern)
                .ParseMultiLines(From.OriginalTestCase);

            var expectedMultiLineResult = new ParseResult<IEnumerable<ParsedData<string[], IEnumerable<IEnumerable<string>>>>, string[][]>(
                true,
                new List<ParsedData<string[], IEnumerable<IEnumerable<string>>>>
                {
                    new ParsedData<string[], IEnumerable<IEnumerable<string>>>(parsedData.Split(','), Enumerable.Empty<IEnumerable<string>>())
                });

            multiLineParseResult.Should().BeEquivalentTo(expectedMultiLineResult);
        }

        private static string Throw_when_begins_with_other_than_Given_and_When_test_case_source = @"[
          {
            'case': 'Case #1: Given Tom has 2 apples and 3 oranges
                     And Jerry has 1 apple and 1 orange',
            'description': 'Begins with ""Case"" -> throw'
          },
          {
            'case': 'Then Tom has 2 apples and 3 oranges
                     And Jerry has 1 apple and 1 orange',
            'description': 'Begins with ""Then"" -> throw'
          },
          {
            'case': 'And Tom has 2 apples and 3 oranges
                     And Jerry has 1 apple and 1 orange',
            'description': 'Begins with ""And"" -> throw'
          }
        ]";

        private static object[] Throw_when_begins_with_other_than_Given_and_When_case_ids =
            GetCaseIdsFrom(CasesFrom(Throw_when_begins_with_other_than_Given_and_When_test_case_source), descriptionFieldName: "description");

        [Test, TestCaseSource("Throw_when_begins_with_other_than_Given_and_When_case_ids")]
        public void When_a_test_case_begins_with_other_than_Given_and_When_it_throws(int caseId, string description)
        {
            var cases = CasesFrom(Throw_when_begins_with_other_than_Given_and_When_test_case_source);

            var @case = cases[caseId]["case"];

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

            var parseResult = gwtParser.WithPattern(@"^Given (.*) has (\d+) apple(?:s|) and (\d+) orange(?:s|)$")
                .ParseSingleLine<TestData>();

            var expectedResult = new TestData
            {
                Name = "Tom",
                Fruits = new Dictionary<string, int>
                {
                    { "Apple", 2 },
                    { "Orange", 3 }
                }
            };

            parseResult.ParsedData.Line.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void ParseMultiLines_parses_with_generic_type()
        {
            const string @case = @"Given Tom has 2 apples and 3 oranges
                                   And Jerry has 1 apple and 1 orange";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"^Given (.*) has (\d+) apple(?:s|) and (\d+) orange(?:s|)$")
                .ParseMultiLines<TestData>();

            var expectedResult = new List<TestData> {
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
                }
            };

            parseResult.ParsedData.Select(d => d.Line).Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Parses_with_custom_parser_class()
        {
            const string @case = "Given the meeting is at 10:30 on 15th of August";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parseResult = gwtParser.WithPattern(@"^Given the meeting is at (.*) on (\d+)(?:st|nd|rd|th) of (.*)$")
                                       .ParseSingleLine<DateTimeParser, DateTime>();

            parseResult.ParsedData.Line.Should().Be(new DateTime(2018, 8, 15, 10, 30, 0));
        }

        [Test]
        public void Parses_table_with_custom_type_in_single_line()
        {
            const string @case = @"Given following attendees
                                   +-------+------+----------+
                                   | name  | team | title    |
                                   +-------+------+----------+
                                   | Tom   | 6    | TechLead |
                                   | Jerry | 2    | Manager  |
                                   +-------+------+----------+";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parseResult = gwtParser.WithPattern(@"^Given following attendees$")
                                       .ParseSingleLine()
                                       .WithTableOf<Attendee>();

            parseResult.ParsedData.Should().Match<ParsedData<IList<string>, IEnumerable<Attendee>>>(d =>
                d.Line.Count == 0 &&
                d.Table.Count() == 2 &&
                d.Table.ToList()[0].Name == "Tom" &&
                d.Table.ToList()[0].Team == 6 &&
                d.Table.ToList()[0].Title == Title.TechLead &&
                d.Table.ToList()[1].Name == "Jerry" &&
                d.Table.ToList()[1].Team == 2 &&
                d.Table.ToList()[1].Title == Title.Manager);
        }

        [Test]
        public void Flips_and_parses_table_when_the_table_is_vertical()
        {
            const string @case = @"Given following attendees
                                   ++-------++----------+---------+
                                   || name  || Tom      | Jerry   |
                                   || team  || 6        | 2       |
                                   || title || TechLead | Manager |
                                   ++-------++----------+---------+";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parseResult = gwtParser.WithPattern(@"^Given following attendees$")
                                       .ParseSingleLine()
                                       .WithTableOf<Attendee>();

            parseResult.ParsedData.Should().Match<ParsedData<IList<string>, IEnumerable<Attendee>>>(d =>
                d.Line.Count == 0 &&
                d.Table.Count() == 2 &&
                d.Table.ToList()[0].Name == "Tom" &&
                d.Table.ToList()[0].Team == 6 &&
                d.Table.ToList()[0].Title == Title.TechLead &&
                d.Table.ToList()[1].Name == "Jerry" &&
                d.Table.ToList()[1].Team == 2 &&
                d.Table.ToList()[1].Title == Title.Manager);
        }

        [Test]
        public void Parses_table_with_custom_parser_class_in_single_line()
        {
            const string @case = @"Given following schedule for Tom's meetings
                                   +-------+-----+--------+
                                   | time  | day | month  |
                                   +-------+-----+--------+
                                   | 10:30 | 15  | May    |
                                   | 9:00  | 3   | August |
                                   +-------+-----+--------+";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parseResult = gwtParser.WithPattern(@"^Given following schedule for (?:Tom|Jerry)'s meetings$")
                                       .ParseSingleLine()
                                       .WithTableOf<DateTimeParser, DateTime>();

            parseResult.ParsedData.Should().Match<ParsedData<IList<string>, IEnumerable<DateTime>>>(d =>
                d.Line.Count == 0 &&
                d.Table.Count() == 2 &&
                d.Table.ToList()[0] == new DateTime(2018, 5, 15, 10, 30, 0) &&
                d.Table.ToList()[1] == new DateTime(2018, 8, 3, 9, 0, 0));
        }

        [Test]
        public void Parsing_table_with_no_custom_type_includes_headers()
        {
            const string @case = @"Given following attendees
                                   +-------+------+----------+
                                   | name  | team | title    |
                                   +-------+------+----------+
                                   | Tom   | 6    | TechLead |
                                   | Jerry | 2    | Manager  |
                                   +-------+------+----------+";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parseResult = gwtParser.WithPattern(@"^Given following attendees$")
                                       .ParseSingleLine();

            parseResult.ParsedData.Should().Match<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>>(d =>
                d.Line.Count == 0 &&
                d.Table.Count() == 3 &&
                d.Table.ToList()[0].ToList()[0] == "name" &&
                d.Table.ToList()[0].ToList()[1] == "team" &&
                d.Table.ToList()[0].ToList()[2] == "title" &&
                d.Table.ToList()[1].ToList()[0] == "Tom" &&
                d.Table.ToList()[1].ToList()[1] == "6" &&
                d.Table.ToList()[1].ToList()[2] == "TechLead" &&
                d.Table.ToList()[2].ToList()[0] == "Jerry" &&
                d.Table.ToList()[2].ToList()[1] == "2" &&
                d.Table.ToList()[2].ToList()[2] == "Manager");
        }

        [Test]
        public void Parses_table_with_custom_type_in_multi_lines()
        {
            const string @case = @"Given the meeting is at 11:30 on 15th of August with following attendees
                                   +-------+------+----------+
                                   | name  | team | title    |
                                   +-------+------+----------+
                                   | Tom   | 6    | TechLead |
                                   | Jerry | 2    | Manager  |
                                   +-------+------+----------+
                                   And another meeting is at 9:00 on 16th of August with following attendees
                                   +--------+------+-----------+
                                   | name   | team | title     |
                                   +--------+------+-----------+
                                   | Tom    | 6    | TechLead  |
                                   | Jerry  | 2    | Manager   |
                                   | Cuckoo | 7    | Developer |
                                   +--------+------+-----------+";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parseResult = gwtParser.WithPattern(@"^Given (?:the|another) meeting is at (.*) on (\d+)(?:st|nd|rd|th) of (.*) with following attendees$")
                                       .ParseMultiLines<DateTimeParser, DateTime>()
                                       .WithTableOf<Attendee>();

            parseResult.ParsedData.Should().Match<IEnumerable<ParsedData<DateTime, IEnumerable<Attendee>>>>(d =>
                d.Count() == 2 &&
                d.ToList()[0].Line == new DateTime(2018, 8, 15, 11, 30, 0) &&
                d.ToList()[0].Table.Count() == 2 &&
                d.ToList()[0].Table.ToList()[0].Name == "Tom" &&
                d.ToList()[0].Table.ToList()[0].Team == 6 &&
                d.ToList()[0].Table.ToList()[0].Title == Title.TechLead &&
                d.ToList()[0].Table.ToList()[1].Name == "Jerry" &&
                d.ToList()[0].Table.ToList()[1].Team == 2 &&
                d.ToList()[0].Table.ToList()[1].Title == Title.Manager &&
                d.ToList()[1].Line == new DateTime(2018, 8, 16, 9, 0, 0) &&
                d.ToList()[1].Table.Count() == 3 &&
                d.ToList()[1].Table.ToList()[0].Name == "Tom" &&
                d.ToList()[1].Table.ToList()[0].Team == 6 &&
                d.ToList()[1].Table.ToList()[0].Title == Title.TechLead &&
                d.ToList()[1].Table.ToList()[1].Name == "Jerry" &&
                d.ToList()[1].Table.ToList()[1].Team == 2 &&
                d.ToList()[1].Table.ToList()[1].Title == Title.Manager &&
                d.ToList()[1].Table.ToList()[2].Name == "Cuckoo" &&
                d.ToList()[1].Table.ToList()[2].Team == 7 &&
                d.ToList()[1].Table.ToList()[2].Title == Title.Developer);
        }

        [Test]
        public void Parses_table_with_custom_parser_class_in_multi_lines()
        {
            const string @case = @"Given following schedule for Tom's meetings
                                   +-------+-----+--------+
                                   | time  | day | month  |
                                   +-------+-----+--------+
                                   | 10:30 | 15  | May    |
                                   | 9:00  | 3   | August |
                                   +-------+-----+--------+
                                   And following schedule for Jerry's meetings
                                   +-------+-----+-------+
                                   | time  | day | month |
                                   +-------+-----+-------+
                                   | 10:30 | 15  | May   |
                                   | 8:30  | 25  | June  |
                                   +-------+-----+-------+";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parseResult = gwtParser.WithPattern(@"^Given following schedule for (?:Tom|Jerry)'s meetings$")
                                       .ParseMultiLines()
                                       .WithTableOf<DateTimeParser, DateTime>();

            parseResult.ParsedData.Should().Match<IEnumerable<ParsedData<IList<string>, IEnumerable<DateTime>>>>(d =>
                d.Count() == 2 &&
                d.ToList()[0].Table.Count() == 2 &&
                d.ToList()[0].Table.ToList()[0] == new DateTime(2018, 5, 15, 10, 30, 0) &&
                d.ToList()[0].Table.ToList()[1] == new DateTime(2018, 8, 3, 9, 0, 0) &&
                d.ToList()[1].Table.Count() == 2 &&
                d.ToList()[1].Table.ToList()[0] == new DateTime(2018, 5, 15, 10, 30, 0) &&
                d.ToList()[1].Table.ToList()[1] == new DateTime(2018, 6, 25, 8, 30, 0));
        }

        private class Attendee
        {
            public Attendee(string name, int team, Title title)
            {
                Name = name;
                Team = team;
                Title = title;
            }

            public string Name { get; private set; }
            public int Team { get; private set; }
            public Title Title { get; private set; }
        }

        private enum Title
        {
            Developer,
            TechLead,
            Manager
        }

        private class DateTimeParser : IParser<DateTime>
        {
            public DateTimeParser(TimeSpan time, int day, string monthName)
            {
                var month = Convert.ToDateTime(monthName + " 01, 1900").Month;

                Value = new DateTime(2018, month, day).Add(time);
            }

            public DateTime Value { get; private set; }
        }

        [Test]
        public void Strings_in_single_line_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                .To("Name".As<string>(), "Quantity".As<int>(), "FavoriteFruit".As<Fruit>(), "Date".As<DateTime>(), "Time".As<TimeSpan>(), "DateTimeOffset".As<DateTimeOffset>(), "Id".As<Guid>(), "HourOfDay".As<HourOfDay>())
                .ParseSingleLine();

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

            ((object)parseResult.ParsedData.Line).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Strings_in_multi_lines_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm
                                   And test data - Name: Jerry, Age: 1, Favorite Fruit: orange, Date: 2017/2/1, Time: 05:42:02, DateTimeOffset: 2017-02-01T06:51:16-08:00, ID: B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F, Hour of day: 11am";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                .To("Name".IsA<string>(), "Quantity".IsA<int>(), "FavoriteFruit".IsA<Fruit>(), "Date".IsA<DateTime>(), "Time".Of<TimeSpan>(), "DateTimeOffset".Of<DateTimeOffset>(), "Id".Of<Guid>(), "HourOfDay".Of<HourOfDay>())
                .ParseMultiLines();

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

            ((object)parseResult.ParsedData.Select(d => d.Line)).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Strings_in_single_line_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method_and_construct_return_object()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                                       .To("name".As<string>(), "quantity".As<int>(), "favoriteFruit".As<Fruit>(), "date".As<DateTime>(), "time".As<TimeSpan>(), "dateTimeOffset".As<DateTimeOffset>(), "id".As<Guid>(), "hourOfDay".As<HourOfDay>())
                                       .ParseSingleLine<ObjectWithConstructor>(Using.Constructor);

            var expected = new ObjectWithConstructor(
                Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A"), new HourOfDay("3pm"), TimeSpan.Parse("10:35:17"), DateTimeOffset.Parse("2017-01-10T17:30:21-08:00"), Fruit.Apple, DateTime.Parse("2017/1/10"), "Tom", 2);

            parseResult.ParsedData.Line.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Strings_in_multi_lines_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method_and_construct_return_object()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm
                                   And test data - Name: Jerry, Age: 1, Favorite Fruit: orange, Date: 2017/2/1, Time: 05:42:02, DateTimeOffset: 2017-02-01T06:51:16-08:00, ID: B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F, Hour of day: 11am";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                                       .To("name".As<string>(), "quantity".As<int>(), "favoriteFruit".As<Fruit>(), "date".As<DateTime>(), "time".As<TimeSpan>(), "dateTimeOffset".As<DateTimeOffset>(), "id".As<Guid>(), "hourOfDay".As<HourOfDay>())
                                       .ParseMultiLines<ObjectWithConstructor>(Using.Constructor);

            var expected = new List<ObjectWithConstructor> {
                new ObjectWithConstructor(
                    Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A"), new HourOfDay("3pm"), TimeSpan.Parse("10:35:17"), DateTimeOffset.Parse("2017-01-10T17:30:21-08:00"), Fruit.Apple, DateTime.Parse("2017/1/10"), "Tom", 2),
                new ObjectWithConstructor(
                    Guid.Parse("B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F"), new HourOfDay("11am"), TimeSpan.Parse("05:42:02"), DateTimeOffset.Parse("2017-02-01T06:51:16-08:00"), Fruit.Orange, DateTime.Parse("2017/2/1"), "Jerry", 1)
            };

            parseResult.ParsedData.Select(d => d.Line).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Additional_parameter_values_that_passed_to_with_method_are_added_to_the_parsed_values_for_constructor_parameters()
        {
            const string @case = @"Given test data - Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm
                                   And test data - Time: 05:42:02, DateTimeOffset: 2017-02-01T06:51:16-08:00, ID: B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F, Hour of day: 11am";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                                       .To("time".As<TimeSpan>(), "dateTimeOffset".As<DateTimeOffset>(), "id".As<Guid>(), "hourOfDay".As<HourOfDay>())
                                       .WithAdditionalValuesOf("Tom".For("name"), 2.For("quantity"), Fruit.Apple.For("favoriteFruit"), DateTime.Parse("2017/1/10").For("date"))
                                       .ParseMultiLines<ObjectWithConstructor>(Using.Constructor);

            var expected = new List<ObjectWithConstructor> {
                new ObjectWithConstructor(
                    Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A"), new HourOfDay("3pm"), TimeSpan.Parse("10:35:17"), DateTimeOffset.Parse("2017-01-10T17:30:21-08:00"), Fruit.Apple, DateTime.Parse("2017/1/10"), "Tom", 2),
                new ObjectWithConstructor(
                    Guid.Parse("B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F"), new HourOfDay("11am"), TimeSpan.Parse("05:42:02"), DateTimeOffset.Parse("2017-02-01T06:51:16-08:00"), Fruit.Apple, DateTime.Parse("2017/1/10"), "Tom", 2)
            };

            parseResult.ParsedData.Select(d => d.Line).Should().BeEquivalentTo(expected);
        }

        private class ObjectWithConstructor
        {
            public ObjectWithConstructor(Guid id, HourOfDay hourOfDay, TimeSpan time, DateTimeOffset dateTimeOffset, Fruit favoriteFruit, DateTime date, string name, int quantity)
            {
                Id = id;
                HourOfDay = hourOfDay;
                Time = time;
                DateTimeOffset = dateTimeOffset;
                FavoriteFruit = favoriteFruit;
                Date = date;
                Name = name;
                Quantity = quantity;
            }

            public Guid Id { get; private set; }
            public HourOfDay HourOfDay { get; private set; }
            public TimeSpan Time { get; private set; }
            public DateTimeOffset DateTimeOffset { get; private set; }
            public Fruit FavoriteFruit { get; private set; }
            public DateTime Date { get; private set; }
            public string Name { get; private set; }
            public int Quantity { get; private set; }
        }

        [Test]
        public void Strings_in_single_line_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method_and_create_return_object()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                                       .To("Name".As<string>(), "Quantity".As<int>(), "FavoriteFruit".As<Fruit>(), "Date".As<DateTime>(), "Time".As<TimeSpan>(), "DateTimeOffset".As<DateTimeOffset>(), "Id".As<Guid>(), "HourOfDay".As<HourOfDay>())
                                       .ParseSingleLine<ObjectWithProperties>(Using.Properties);

            var expected = new ObjectWithProperties
            {
                Id = Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A"),
                HourOfDay = new HourOfDay("3pm"),
                Time = TimeSpan.Parse("10:35:17"),
                DateTimeOffset = DateTimeOffset.Parse("2017-01-10T17:30:21-08:00"),
                FavoriteFruit = Fruit.Apple,
                Date = DateTime.Parse("2017/1/10"),
                Name = "Tom",
                Quantity = 2
            };

            parseResult.ParsedData.Line.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Strings_in_multi_lines_that_matched_with_the_pattern_are_parsed_correctly_to_the_types_configured_with_to_method_and_create_return_object()
        {
            const string @case = @"Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, Date: 2017/1/10, Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm
                                   And test data - Name: Jerry, Age: 1, Favorite Fruit: orange, Date: 2017/2/1, Time: 05:42:02, DateTimeOffset: 2017-02-01T06:51:16-08:00, ID: B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F, Hour of day: 11am";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                                       .To("Name".As<string>(), "Quantity".As<int>(), "FavoriteFruit".As<Fruit>(), "Date".As<DateTime>(), "Time".As<TimeSpan>(), "DateTimeOffset".As<DateTimeOffset>(), "Id".As<Guid>(), "HourOfDay".As<HourOfDay>())
                                       .ParseMultiLines<ObjectWithProperties>(Using.Properties);

            var expected = new List<ObjectWithProperties>
            {
                new ObjectWithProperties
                {
                    Id = Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A"),
                    HourOfDay = new HourOfDay("3pm"),
                    Time = TimeSpan.Parse("10:35:17"),
                    DateTimeOffset = DateTimeOffset.Parse("2017-01-10T17:30:21-08:00"),
                    FavoriteFruit = Fruit.Apple,
                    Date = DateTime.Parse("2017/1/10"),
                    Name = "Tom",
                    Quantity = 2
                },
                new ObjectWithProperties
                {
                    Id = Guid.Parse("B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F"),
                    HourOfDay = new HourOfDay("11am"),
                    Time = TimeSpan.Parse("05:42:02"),
                    DateTimeOffset = DateTimeOffset.Parse("2017-02-01T06:51:16-08:00"),
                    FavoriteFruit = Fruit.Orange,
                    Date = DateTime.Parse("2017/2/1"),
                    Name = "Jerry",
                    Quantity = 1
                }
            };

            parseResult.ParsedData.Select(d => d.Line).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Additional_parameter_values_that_passed_to_with_method_are_added_to_the_parsed_values_for_properties()
        {
            const string @case = @"Given test data - Time: 10:35:17, DateTimeOffset: 2017-01-10T17:30:21-08:00, ID: 6579328A-B45A-48EB-BC1C-68018157F47A, Hour of day: 3pm
                                   And test data - Time: 05:42:02, DateTimeOffset: 2017-02-01T06:51:16-08:00, ID: B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F, Hour of day: 11am";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                                       .To("Time".As<TimeSpan>(), "DateTimeOffset".As<DateTimeOffset>(), "Id".As<Guid>(), "HourOfDay".As<HourOfDay>())
                                       .WithAdditionalValuesOf("Tom".For("Name"), 2.For("Quantity"), Fruit.Apple.For("FavoriteFruit"), DateTime.Parse("2017/1/10").For("Date"))
                                       .ParseMultiLines<ObjectWithProperties>(Using.Properties);

            var expected = new List<ObjectWithProperties>
            {
                new ObjectWithProperties
                {
                    Id = Guid.Parse("6579328A-B45A-48EB-BC1C-68018157F47A"),
                    HourOfDay = new HourOfDay("3pm"),
                    Time = TimeSpan.Parse("10:35:17"),
                    DateTimeOffset = DateTimeOffset.Parse("2017-01-10T17:30:21-08:00"),
                    FavoriteFruit = Fruit.Apple,
                    Date = DateTime.Parse("2017/1/10"),
                    Name = "Tom",
                    Quantity = 2
                },
                new ObjectWithProperties
                {
                    Id = Guid.Parse("B045F0D5-F7FB-4DA8-91A4-8D8D365B0B0F"),
                    HourOfDay = new HourOfDay("11am"),
                    Time = TimeSpan.Parse("05:42:02"),
                    DateTimeOffset = DateTimeOffset.Parse("2017-02-01T06:51:16-08:00"),
                    FavoriteFruit = Fruit.Apple,
                    Date = DateTime.Parse("2017/1/10"),
                    Name = "Tom",
                    Quantity = 2
                }
            };

            parseResult.ParsedData.Select(d => d.Line).Should().BeEquivalentTo(expected);
        }

        private class ObjectWithProperties
        {
            public Guid Id { get; set; }
            public HourOfDay HourOfDay { get; set; }
            public TimeSpan Time { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public Fruit FavoriteFruit { get; set; }
            public DateTime Date { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
        }

        [TestCase("Given nullable integer: .", "")]
        [TestCase("Given nullable integer: 3.", "3")]
        public void ParseSingleLine_parses_the_match_to_nullable_type_correctly(string @case, string result)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given nullable integer: (|\d+).")
                .ParseSingleLine<int?>();

            var expected = string.IsNullOrEmpty(result) ? (int?)null : int.Parse(result);

            parseResult.ParsedData.Line.Should().Be(expected);
        }

        [Test]
        public void ParseMultiLines_parses_the_matches_to_nullable_type_correctly()
        {
            const string @case = @"Given nullable integer: .
                                   And nullable integer: 3.";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given nullable integer: (|\d+).")
                .ParseMultiLines<int?>();

            var expected = new List<int?> { null, 3 };

            parseResult.ParsedData.Select(d => d.Line).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Matches_are_parsed_to_nullable_types_and_returned_via_properties_in_dynamic_object()
        {
            const string @case = @"Given nullable TimeSpan: 00:17:00, nullable integer: .
                                   And nullable TimeSpan: , nullable integer: 3.";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given nullable TimeSpan: (|.*), nullable integer: (|\d+).")
                .To("NullableTimeSpan".As<TimeSpan?>(), "NullableInteger".As<int?>())
                .ParseMultiLines();

            var expected = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "NullableTimeSpan", TimeSpan.FromMinutes(17) },
                    { "NullableInteger", null }
                },
                new Dictionary<string, object>
                {
                    { "NullableTimeSpan", null },
                    { "NullableInteger", 3 }
                }
            };

            ((object)parseResult.ParsedData.Select(d => d.Line)).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Matches_are_parsed_to_nullable_types_via_constructor_parameters()
        {
            const string @case = @"Given nullable integer: .
                                   And nullable integer: 3.";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given nullable integer: (|\d+).")
                .ParseMultiLines<NullableInteger>();

            var expected = new List<int?> { null, 3 };

            parseResult.ParsedData.Select(d => d.Line).Select(d => (int?)d).Should().BeEquivalentTo(expected);
        }

        private class NullableInteger
        {
            private readonly int? _value;

            public NullableInteger(int? value)
            {
                _value = value;
            }

            public static explicit operator int?(NullableInteger nullableInteger)
            {
                return nullableInteger._value;
            }
        }

        [Test]
        public void Dynamic_parser_sets_dynamic_properties_with_default_value_when_there_are_no_matching_lines()
        {
            const string @case = @"Given something..";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parseResult = gwtParser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), Date: (.*), Time: (.*), DateTimeOffset: (.*), ID: (.*), Hour of day: ((?:[1][0-2]|[1-9])(?:am|pm))")
                                       .To("Name".As<string>(), "Quantity".As<int>(), "FavoriteFruit".As<Fruit>(), "Date".As<DateTime>(), "Time".As<TimeSpan>(), "DateTimeOffset".As<DateTimeOffset>(), "Id".As<Guid>(), "HourOfDay".As<HourOfDay>())
                                       .ParseSingleLine();

            var expected = new Dictionary<string, object>
            {
                { "Name", default(string) },
                { "Quantity", default(int) },
                { "FavoriteFruit", default(Fruit) },
                { "Date", default(DateTime) },
                { "Time", default(TimeSpan) },
                { "DateTimeOffset", default(DateTimeOffset) },
                { "Id", default(Guid) },
                {"HourOfDay", default(HourOfDay) }
            };

            ((object)parseResult.ParsedData.Line).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Dynamic_parser_does_not_throw_when_there_are_no_matching_lines()
        {
            const string @case = "Given something..";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parser = gwtParser.WithPattern(@"^Given there are (\d+) cats$")
                                  .To("Cats".As<int>())
                                  .WithAdditionalValuesOf(Fruit.Apple.For("favoriteFruit"));

            Assert.DoesNotThrow(() =>
            {
                var singleLine = parser.ParseSingleLine().ParsedData.Line.Cats;
                var singleTable = parser.ParseSingleLine().ParsedData.Table.ToList();
                var multiLines = parser.ParseMultiLines().ParsedData.Select(d => d.Line.Cats).ToList();
                var multiTables = parser.ParseMultiLines().ParsedData.Select(d => d.Table.ToList()).ToList();
            });
        }

        [Test]
        public void Parsing_with_custom_parser_class_does_not_throw_when_there_are_no_matching_lines()
        {
            const string @case = "Given something..";

            var gwtParser = TinyGWTParser.WithTestCase(@case);

            var parser = gwtParser.WithPattern(@"^Given the meeting is at (.*) on (\d+)(?:st|nd|rd|th) of (.*)$");

            Assert.DoesNotThrow(() =>
            {
                var singleLine = parser.ParseSingleLine<DateTimeParser, DateTime>().ParsedData.Line.ToUniversalTime();
                var singleTable = parser.ParseSingleLine<DateTimeParser, DateTime>().ParsedData.Table.Select(t => t.ToList()).ToList();
                var multiLines = parser.ParseMultiLines<DateTimeParser, DateTime>().ParsedData.Select(d => d.Line.ToUniversalTime()).ToList();
                var multiTables = parser.ParseMultiLines<DateTimeParser, DateTime>().ParsedData.Select(d => d.Table.Select(t => t.ToList()).ToList()).ToList();
            });
        }

        [Test]
        public void Parsing_with_custom_table_class_does_not_throw_when_there_are_no_matching_lines()
        {
            const string @case = @"Given something..";

            var gwtParser = TinyGWTParser.WithTestCase(@case);


            var parser = gwtParser.WithPattern(@"^Given following attendees$");

            Assert.DoesNotThrow(() =>
            {
                var singleLine = parser.ParseSingleLine().WithTableOf<Attendee>().ParsedData.Line.Select(l => l).ToList();
                var singleTable = parser.ParseSingleLine().WithTableOf<Attendee>().ParsedData.Table.Select(t => t.Name).ToList();
                var multiLines = parser.ParseMultiLines().WithTableOf<Attendee>().ParsedData.Select(d => d.Line.Select(l => l).ToList()).ToList();
                var multiTables = parser.ParseMultiLines().WithTableOf<Attendee>().ParsedData.Select(d => d.Table.Select(t => t.Name).ToList()).ToList();
            });
        }

        private enum Fruit
        {
            Apple,
            Orange
        }

        private static Dictionary<string, string>[] CasesFrom(string caseSource)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>[]>(caseSource);
        }

        private static object[] GetCaseIdsFrom(Dictionary<string, string>[] cases, string descriptionFieldName)
        {
            var caseIds = new List<object>();

            for (var i = 0; i < cases.Length; i++)
            {
                var description = cases[i][descriptionFieldName];
                caseIds.Add(new object[] { i, description });
            }

            return caseIds.ToArray();
        }
    }
}
