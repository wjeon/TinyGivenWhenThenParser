using System;

namespace TinyGivenWhenThenParser
{
    public class GwtParserException : Exception
    {
        public GwtParserException(string message) : base(message)
        {
        }
    }
}