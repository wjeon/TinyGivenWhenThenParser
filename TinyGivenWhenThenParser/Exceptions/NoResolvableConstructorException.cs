using System;

namespace TinyGivenWhenThenParser.Exceptions
{
    [Serializable]
    internal class NoResolvableConstructorException : Exception
    {
        public NoResolvableConstructorException(string message) : base(message)
        {
        }
    }
}