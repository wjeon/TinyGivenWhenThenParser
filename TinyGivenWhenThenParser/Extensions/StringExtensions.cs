using System;
using System.Collections.Generic;

namespace TinyGivenWhenThenParser.Extensions
{
    public static class StringExtensions
    {
        public static Property As<T>(this string name)
        {
            return new Property(name, typeof(T));
        }

        public static Property IsA<T>(this string name)
        {
            return new Property(name, typeof(T));
        }

        public static Property Of<T>(this string name)
        {
            return new Property(name, typeof(T));
        }

        [Obsolete]
        public static KeyValuePair<string, object> Value(this string name, object value)
        {
            return new KeyValuePair<string, object>(name, value);
        }

        public static KeyValuePair<string, object> For(this object value, string name)
        {
            return new KeyValuePair<string, object>(name, value);
        }
    }
}
