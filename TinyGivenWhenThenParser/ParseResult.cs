namespace TinyGivenWhenThenParser
{
    public class ParseResult<T> where T : class
    {
        public ParseResult() {}

        public ParseResult(bool parsed, T data)
        {
            Parsed = parsed;
            Data = data;
        }

        public bool Parsed { get; private set; }
        public T Data { get; private set; }
    }
}