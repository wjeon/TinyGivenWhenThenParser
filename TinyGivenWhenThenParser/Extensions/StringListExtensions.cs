using System.Collections.Generic;
using System.Linq;
using TinyGivenWhenThenParser.Exceptions;

namespace TinyGivenWhenThenParser.Extensions
{
    public static class StringListExtensions
    {
        public static IEnumerable<string> ToGwtLines(this IEnumerable<string> lines)
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

        public static bool HaveTableSource(this List<string> caseLineSegments)
        {
            return caseLineSegments.Count > 1;
        }

        public static IEnumerable<string> GetTableSource(this List<string> caseLineSegments)
        {
            return caseLineSegments.GetRange(1, caseLineSegments.Count - 1).Where(l => l.Contains('|'));
        }
    }
}