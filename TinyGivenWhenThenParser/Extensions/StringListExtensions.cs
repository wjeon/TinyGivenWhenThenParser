using System.Collections.Generic;

namespace TinyGivenWhenThenParser.Extensions
{
    public static class StringListExtensions
    {
        public static bool HaveTableSource(this List<string> caseLineSegments)
        {
            return caseLineSegments.Count > 1;
        }

        public static IEnumerable<string> GetTableSource(this List<string> caseLineSegments)
        {
            return caseLineSegments.GetRange(1, caseLineSegments.Count - 1);
        }
    }
}