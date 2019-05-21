using System.Collections.Generic;
using System.Linq;

namespace TinyGivenWhenThenParser
{
    public class ParseResults<TParsedData, TLine> : ParseResult<TParsedData, TLine>
    {
        public ParseResults(bool parsed, TParsedData parsedData) : base(parsed, parsedData)
        {
        }

        public new ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine> WithTableOf<TTable>()
        {
            if (typeof(TParsedData) == typeof(IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>))
            {
                PrintTableIsAlreadyRequestedType();
                return ParsedData as ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine>;
            }

            if (typeof(TTable) == typeof(IEnumerable<IEnumerable<string>>))
            {
                PrintConversionToStringListNotNeeded();
                return ParsedData as ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine>;
            }

            var parsedData = (IEnumerable<ParsedData<TLine, IEnumerable<IEnumerable<string>>>>)ParsedData;

            var parsedDataWithConvertedTable = parsedData.Select(ParsedDataWithConvertedTable<TTable>);

            return new ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine>(Parsed, parsedDataWithConvertedTable);
        }
    }
}