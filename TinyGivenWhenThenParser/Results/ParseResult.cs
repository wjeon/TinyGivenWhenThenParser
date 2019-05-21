using System;
using System.Collections.Generic;
using System.Linq;
using TinyGivenWhenThenParser.StringConverters;

namespace TinyGivenWhenThenParser.Results
{
    public class ParseResult<TParsedData, TLine>
    {
        public ParseResult(bool parsed, TParsedData parsedData)
        {
            Parsed = parsed;
            ParsedData = parsedData;
        }

        public bool Parsed { get; private set; }
        public TParsedData ParsedData { get; private set; }

        public ParseResult<ParsedData<TLine, IEnumerable<TTable>>, TLine> WithTableOf<TTable>()
        {
            if (typeof(TParsedData) == typeof(ParsedData<TLine, IEnumerable<TTable>>))
            {
                PrintTableIsAlreadyRequestedType();
                return ParsedData as ParseResult<ParsedData<TLine, IEnumerable<TTable>>, TLine>;
            }

            if (typeof(TTable) == typeof(IEnumerable<IEnumerable<string>>))
            {
                PrintConversionToStringListNotNeeded();
                return ParsedData as ParseResult<ParsedData<TLine, IEnumerable<TTable>>, TLine>;
            }

            var parsedData = ParsedData as ParsedData<TLine, IEnumerable<IEnumerable<string>>>;

            var parsedDataWithConvertedTable = ParsedDataWithConvertedTable<TTable>(parsedData);

            return new ParseResult<ParsedData<TLine, IEnumerable<TTable>>, TLine>(Parsed, parsedDataWithConvertedTable);
        }

        protected static void PrintTableIsAlreadyRequestedType()
        {
            Console.WriteLine("The table is already converted to the requested type");
        }

        protected static void PrintConversionToStringListNotNeeded()
        {
            Console.WriteLine("The table shouldn't need to be converted to string list rows");
        }

        protected static ParsedData<TLine, IEnumerable<TTable>> ParsedDataWithConvertedTable<TTable>(ParsedData<TLine, IEnumerable<IEnumerable<string>>> parsedData)
        {
            return new ParsedData<TLine, IEnumerable<TTable>>(
                parsedData.Line,
                parsedData.Table.Select(row => row.ToList().ToCostruct<TTable>()));
        }
    }
}