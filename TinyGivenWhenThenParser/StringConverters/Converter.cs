using System;
using TinyGivenWhenThenParser.Extensions;

namespace TinyGivenWhenThenParser.StringConverters
{
    internal static class Converter
    {
        public static object ConvertTo(this string value, Type type)
        {
            return type == typeof(string) || type == typeof(object)
                       ? value
                       : type.IsEnum
                             ? Enum.Parse(type, value, true)
                             : type.IsForConversion()
                                   ? Convert.ChangeType(value, type)
                                   : ParseOrCreate(value, type);
        }

        private static object ParseOrCreate(string value, Type type)
        {
            var parse = type.GetMethod("Parse", new[] { typeof(string) });

            return parse != null ? parse.Invoke(null, new object[] { value }) : new[] { value }.ToCostruct(type);
        }
    }
}