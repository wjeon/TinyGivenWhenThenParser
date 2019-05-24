using System.Collections.Generic;
using System.Linq;

namespace TinyGivenWhenThenParser.Extensions
{
    public static class StringTableExtensions
    {
        public static IEnumerable<IEnumerable<string>> ToHorizontalRowsTable(this IEnumerable<string> tableSource)
        {
            var table = tableSource.Select(c => c.Replace("||", "|").Trim('|').Split('|').Select(v => v.Trim())).ToList();

            return tableSource.First().Trim().EndsWith("||")
                ? table
                : table.FlipVerticalTableToHorizontal();
        }

        private static IEnumerable<IEnumerable<string>> FlipVerticalTableToHorizontal(this IEnumerable<IEnumerable<string>> table)
        {
            var flippedTable = new List<List<string>>();

            foreach (var row in table)
            {
                for (var i = 0; i < row.Count(); i++)
                {
                    if (flippedTable.Count <= i)
                    {
                        flippedTable.Add(new List<string> { row.ToArray()[i] });
                    }
                    else
                    {
                        flippedTable[i].Add(row.ToArray()[i]);
                    }
                }
            }

            return flippedTable;
        }

        public static IEnumerable<IEnumerable<string>> GetRowsOnlyWithNoHeaders(this IEnumerable<IEnumerable<string>> table)
        {
            return table.Any() ? table.ToList().GetRange(1, table.Count() - 1) : table;
        }
    }
}