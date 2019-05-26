using System;
using System.Collections.Generic;
using System.Linq;
using TinyGivenWhenThenParser.StringConverters;

namespace TinyGivenWhenThenParser.Results
{
    public class ParseResults<TParsedData, TLine> : ParseResult<TParsedData, TLine>
    {
        public ParseResults(bool parsed, TParsedData parsedData) : base(parsed, parsedData)
        {
        }

        public new ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine> WithTableOf<TTable>()
        {
            return ResultsWithCustomTable(r => r.ToList().ToCostruct<TTable>());
        }

        public new ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine> WithTableOf<TParser, TTable>() where TParser : IParser<TTable>
        {
            return ResultsWithCustomTable(r => r.ToList().ToCostruct<TParser>().Value);
        }

        private ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine> ResultsWithCustomTable<TTable>(Func<IEnumerable<string>, TTable> construct)
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

            var parsedDataWithConvertedTable = parsedData.Select(d => ParsedDataWithConvertedTable(d, construct));

            return new ParseResult<IEnumerable<ParsedData<TLine, IEnumerable<TTable>>>, TLine>(Parsed, parsedDataWithConvertedTable);
        }
    }
}