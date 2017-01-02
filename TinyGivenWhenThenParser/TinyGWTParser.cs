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
            _testCaseLines = testCase
                .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Select(c => c.Trim());
            _gwtLines = ToGwtLinesFrom(_testCaseLines);
        }

        public IList<string> ParseSingleLine()
        {
            var result = ParseData(multiLine: false);

            return result.Any() ? result.First() : new List<string>();
        }

        public IEnumerable<IList<string>> ParseMultiLines()
        {
            return ParseData(multiLine: true);
        }

        private IEnumerable<IList<string>> ParseData(bool multiLine)
        {
            foreach (var line in _gwtLines)
            {
                var matchResult = Regex.Match(line, _pattern);

                if (!matchResult.Success)
                    continue;

                var result = new List<string>();
                for (var i = 1; i < matchResult.Groups.Count; i++)
                {
                    result.Add(matchResult.Groups[i].Value);
                }

                yield return result;

                if (!multiLine)
                    break;
            }
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

        private IEnumerable<string> ToGwtLinesFrom(IEnumerable<string> lines)
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
            }
            return gwtLines;
        }
    }
}