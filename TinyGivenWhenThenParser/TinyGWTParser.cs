using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TinyGivenWhenThenParser
{
    public class TinyGWTParser
    {
        private readonly IEnumerable<string> _testCaseLines;
        private string _pattern;

        private TinyGWTParser(string testCase)
        {
            _testCaseLines = testCase.Split('\r').Select(c => c.Trim());
        }

        public IList<string> ParseData()
        {
            foreach (var line in _testCaseLines)
            {
                var matchResult = Regex.Match(line, _pattern);

                if (!matchResult.Success)
                    continue;

                var result = new List<string>();
                for (var i = 1; i < matchResult.Groups.Count; i++)
                {
                    result.Add(matchResult.Groups[i].Value);
                }
                return result;
            }
            return new List<string>();
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
    }
}