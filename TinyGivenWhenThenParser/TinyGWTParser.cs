using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TinyGivenWhenThenParser
{
    public class TinyGWTParser
    {
        private readonly IEnumerable<string> _testCaseLines;
        private readonly IEnumerable<string> _gwtLines;
        private string _pattern;

        private TinyGWTParser(string testCase)
        {
            if (!testCase.StartsWith("Given ") && !testCase.StartsWith("When "))
                throw new GwtParserException("Test case must start with 'Given' or 'When'");

            _testCaseLines = testCase
                .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Select(c => c.Trim());
            _gwtLines = ToGwtLinesFrom(_testCaseLines);
        }

        public IList<Property> Properties { get; private set; }

        public ParseResult<IList<string>> ParseSingleLine(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseData(testCase, multiLine: false);

            return new ParseResult<IList<string>>(result.Parsed, result.Data.Any() ? result.Data.First() : new List<string>());
        }

        public ParseResult<T> ParseSingleLine<T>(From testCase = From.TestCaseReplacedAndWithGivenWhenThen) where T : class
        {
            var result = ParseData(testCase, multiLine: false);

            return new ParseResult<T>(result.Parsed,
                result.Data.Any() ? (T)Activator.CreateInstance(typeof(T), result.Data.First()) : default(T));
        }

        public ParseResult<IEnumerable<IList<string>>> ParseMultiLines(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            return ParseData(testCase, multiLine: true);
        }

        public ParseResult<IEnumerable<T>> ParseMultiLines<T>(From testCase = From.TestCaseReplacedAndWithGivenWhenThen) where T : class
        {
            var result = ParseData(testCase, multiLine: true);

            var dataList = new List<T>();

            if (result.Data.Any())
            {
                dataList.AddRange(result.Data.Select(data => (T)Activator.CreateInstance(typeof(T), data)));
            }

            return new ParseResult<IEnumerable<T>>(result.Parsed, dataList);
        }

        private ParseResult<IEnumerable<IList<string>>> ParseData(From testCase, bool multiLine)
        {
            var lines = testCase == From.OriginalTestCase ? _testCaseLines : _gwtLines;
            var parsed = false;
            var results = new List<IList<string>>();

            foreach (var line in lines)
            {
                var matchResult = Regex.Match(line, _pattern);

                if (!matchResult.Success)
                    continue;

                parsed = true;

                var result = new List<string>();
                for (var i = 1; i < matchResult.Groups.Count; i++)
                {
                    result.Add(matchResult.Groups[i].Value);
                }

                results.Add(result);

                if (!multiLine)
                    break;
            }

            return new ParseResult<IEnumerable<IList<string>>>(parsed, results);
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

        private static IEnumerable<string> ToGwtLinesFrom(IEnumerable<string> lines)
        {
            var gwtLines = new List<string>();
            var prefix = string.Empty;
            foreach (var line in lines)
            {
                if (line.StartsWith("Given ") || line.StartsWith("When ") || line.StartsWith("Then "))
                {
                    prefix = line.Substring(0, line.IndexOf(" "));
                    gwtLines.Add(line);
                }
                else if (line.StartsWith("And "))
                {
                    gwtLines.Add(string.Format("!{0}", line).Replace("!And", prefix));
                }
                else
                    throw new GwtParserException("Each line of the test case must start with 'Given', 'When', 'Then' or 'And'");
            }
            return gwtLines;
        }

        public TinyGWTDynamicParser To(params Property[] properties)
        {
            Properties = properties;

            return new TinyGWTDynamicParser();
        }
    }
}