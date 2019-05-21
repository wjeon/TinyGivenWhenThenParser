namespace TinyGivenWhenThenParser
{
    public class ParsedData<TLine, TTable>
    {

        public ParsedData(TLine line, TTable table)
        {
            Line = line;
            Table = table;
        }

        public TLine Line { get; private set; }
        public TTable Table { get; private set; }
    }
}