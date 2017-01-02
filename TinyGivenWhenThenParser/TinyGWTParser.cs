using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TinyGivenWhenThenParser
{
    public class TinyGWTParser
    {
        private string _testCase;
        private string _pattern;

        private TinyGWTParser(string testCase)
        {
            _testCase = testCase;
        }

        public IList<string> ParseData()
        {
            var matchResult = Regex.Match(_testCase, _pattern);

            if (!matchResult.Success)
                return new List<string>();

            var result = new List<string>();
            for (var i = 1; i < matchResult.Groups.Count; i++)
            {
                result.Add(matchResult.Groups[i].Value);
            }
            return result;
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