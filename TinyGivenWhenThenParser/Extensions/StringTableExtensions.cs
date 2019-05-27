using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyGivenWhenThenParser.Extensions
{
    public static class StringTableExtensions
    {
        public static IEnumerable<IEnumerable<string>> ToHorizontalRowsTable(this IEnumerable<string> tableSource)
        {
            var table = tableSource.Select(c => c.Replace("||", "|").Trim('|').Split('|').Select(v => v.Trim())).ToList();

            return tableSource.IsVerticalTable()
                ? table.FlipVerticalTableToHorizontal()
                : table;
        }

        private static bool IsVerticalTable(this IEnumerable<string> tableSource)
        {
            return tableSource.Select(row => row.Trim().Split('|'))
                              .All(r => r.Length > 5 && r[0] == string.Empty && r[1] == string.Empty && r[2] != string.Empty && r[3] == string.Empty &&
                                        r.RestBeforeLastAreNotEmpty() && r.Last() == string.Empty);
        }

        private static bool RestBeforeLastAreNotEmpty(this IReadOnlyList<string> you)
        {
            for (var i = 4; i < you.Count - 1; i++)
            {
                if (you[i] == string.Empty)
                    return false;
            }

            return true;
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