using System;

namespace TinyGivenWhenThenParser.Exceptions
{
    [Serializable]
    public class GwtParserException : Exception
    {
        public GwtParserException(string message) : base(message)
        {
        }
    }
}