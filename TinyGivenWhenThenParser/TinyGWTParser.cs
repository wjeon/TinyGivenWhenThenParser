using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TinyGivenWhenThenParser.Exceptions;
using TinyGivenWhenThenParser.Extensions;
using TinyGivenWhenThenParser.StringConverters;

namespace TinyGivenWhenThenParser
{
    public class TinyGWTParser
    {
        private readonly IEnumerable<string> _testCaseLines;
        private readonly IEnumerable<string> _gwtLines;
        private string _pattern;
        private const string TableLineBreak = "\v\t\a\r\b\f";

        private TinyGWTParser(string testCase)
        {
            var regex = new Regex("\r\n[ ]*\\|");
            var replacedCase = regex.Replace(testCase, $"{TableLineBreak}|");

            if (!replacedCase.StartsWith("Given ") && !replacedCase.StartsWith("When "))
                throw new GwtParserException("Test case must start with 'Given' or 'When'");

            _testCaseLines = replacedCase
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(c => c.Trim());

            _gwtLines = _testCaseLines.ToGwtLines();
        }

        public IList<Property> Properties { get; private set; }

        public ParseResult<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>, IList<string>> ParseSingleLine(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseData(testCase, multiLine: false);

            return new ParseResult<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>, IList<string>>(
                result.Parsed,
                result.ParsedData != null && result.ParsedData.Any()
                    ? result.ParsedData.First()
                    : default(ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>));
        }

        public ParseResults<IEnumerable<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>>, IList<string>> ParseMultiLines(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseData(testCase, multiLine: true);
            return new ParseResults<IEnumerable<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>>, IList<string>>(result.Parsed, result.ParsedData);
        }

        public ParseResult<ParsedData<T, IEnumerable<IEnumerable<string>>>, T> ParseSingleLine<T>(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseData(testCase, multiLine: false);

            return new ParseResult<ParsedData<T, IEnumerable<IEnumerable<string>>>, T>(
                result.Parsed,
                result.ParsedData != null && result.ParsedData.Any()
                    ? new ParsedData<T, IEnumerable<IEnumerable<string>>>(
                        result.ParsedData.First().Line != null && result.ParsedData.First().Line.Any()
                            ? result.ParsedData.First().Line.ToCostruct<T>() : default(T),
                            result.ParsedData.First().Table)
                    : default(ParsedData<T, IEnumerable<IEnumerable<string>>>));
        }

        public ParseResults<IEnumerable<ParsedData<T, IEnumerable<IEnumerable<string>>>>, T> ParseMultiLines<T>(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseData(testCase, multiLine: true);

            var dataListWithTable = new List<ParsedData<T, IEnumerable<IEnumerable<string>>>>();

            if (result.ParsedData != null && result.ParsedData.Any())
            {
                dataListWithTable.AddRange(
                        result.ParsedData.Select(
                            d => new ParsedData<T, IEnumerable<IEnumerable<string>>>(
                                d.Line != null && d.Line.Any() ? d.Line.ToCostruct<T>() : default(T),
                                d.Table
                            )
                        )
                    );
            }

            return new ParseResults<IEnumerable<ParsedData<T, IEnumerable<IEnumerable<string>>>>, T>(result.Parsed, dataListWithTable);
        }

        public ParseResult<ParsedData<TReturn, IEnumerable<IEnumerable<string>>>, TReturn> ParseSingleLine<TParser, TReturn>(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
            where TParser : IParser<TReturn>
        {
            var result = ParseSingleLine<TParser>(testCase);

            return new ParseResult<ParsedData<TReturn, IEnumerable<IEnumerable<string>>>, TReturn>(
                result.Parsed,
                result.ParsedData == null
                    ? null
                    : new ParsedData<TReturn, IEnumerable<IEnumerable<string>>>(result.ParsedData.Line.Value, result.ParsedData.Table));
        }

        public ParseResults<IEnumerable<ParsedData<TReturn, IEnumerable<IEnumerable<string>>>>, TReturn> ParseMultiLines<TParser, TReturn>(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
            where TParser : IParser<TReturn>
        {
            var parsers = ParseMultiLines<TParser>(testCase);

            return new ParseResults<IEnumerable<ParsedData<TReturn, IEnumerable<IEnumerable<string>>>>, TReturn>(
                parsers.Parsed,
                parsers.ParsedData == null
                    ? EmptyParsedDataList<TReturn>()
                    : parsers.ParsedData.Select(d =>
                        new ParsedData<TReturn, IEnumerable<IEnumerable<string>>>(d.Line == null ? default(TReturn) : d.Line.Value, d.Table)));
        }

        private ParseResult<IEnumerable<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>>, IList<string>> ParseData(From testCase, bool multiLine)
        {
            var caseLines = testCase == From.OriginalTestCase ? _testCaseLines : _gwtLines;
            var parsed = false;
            var parsedData = new List<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>>();

            foreach (var caseLine in caseLines)
            {
                var caseLineSegments = caseLine.Split(new [] { TableLineBreak }, StringSplitOptions.None).ToList();

                var matchResult = Regex.Match(caseLineSegments.First(), _pattern);

                if (!matchResult.Success)
                    continue;

                parsed = true;

                var line = new List<string>();
                for (var i = 1; i < matchResult.Groups.Count; i++)
                {
                    line.Add(matchResult.Groups[i].Value);
                }

                if (caseLineSegments.HaveTableSource())
                {
                    var tableSource = caseLineSegments.GetTableSource();

                    var table = tableSource.ToHorizontalRowsTable();

                    parsedData.Add(new ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>(line, table));
                }
                else
                {
                    parsedData.Add(new ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>(line, EmptyTable()));
                }

                if (!multiLine)
                    break;
            }

            return new ParseResult<IEnumerable<ParsedData<IList<string>, IEnumerable<IEnumerable<string>>>>, IList<string>>(parsed, parsedData);
        }

        public static TinyGWTParser WithTestCase(string testCase)
        {
            return new TinyGWTParser(testCase);
        }

        public TinyGWTParser WithPattern(string pattern)
        {
            _pattern = string.Format("^{0}$", pattern.TrimStart('^').TrimEnd('$'));
            return this;
        }

        public TinyGWTDynamicParser To(params Property[] properties)
        {
            Properties = properties;

            return new TinyGWTDynamicParser(this);
        }

        private static IEnumerable<IEnumerable<string>> EmptyTable()
        {
            return Enumerable.Empty<IEnumerable<string>>();
        }

        private static IEnumerable<ParsedData<TReturn, IEnumerable<IEnumerable<string>>>> EmptyParsedDataList<TReturn>()
        {
            return Enumerable.Empty<ParsedData<TReturn, IEnumerable<IEnumerable<string>>>>();
        }
    }
}