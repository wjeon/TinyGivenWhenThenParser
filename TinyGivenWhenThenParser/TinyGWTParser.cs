using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TinyGivenWhenThenParser
{
    public class TinyGWTParser
    {
        public IList<string> ParseData(string value, string pattern)
        {
            var matchResult = Regex.Match(value, pattern);

            if (!matchResult.Success)
                return new List<string>();

            var result = new List<string>();
            for (var i = 1; i < matchResult.Groups.Count; i++)
            {
                result.Add(matchResult.Groups[i].Value);
            }
            return result;
        }
    }
}