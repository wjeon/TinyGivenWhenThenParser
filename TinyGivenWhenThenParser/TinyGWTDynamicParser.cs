using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyGivenWhenThenParser
{
    public class TinyGWTDynamicParser
    {
        private readonly TinyGWTParser _parser;

        public TinyGWTDynamicParser(TinyGWTParser parser)
        {
            _parser = parser;
        }

        public ParseResult<dynamic> ParseSingleLine(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = _parser.ParseSingleLine(testCase);

            return new ParseResult<dynamic>(result.Parsed,
                                            result.Data.Any() ? ParseFrom(result.Data, _parser.Properties) : null);
        }

        private static dynamic ParseFrom(IList<string> data, IList<Property> properties)
        {
            dynamic obj = new System.Dynamic.ExpandoObject();

            for (var i = 0; i < properties.Count; i++)
            {
                ((IDictionary<string, object>)obj)
                    .Add(properties[i].Name, ConvertTo(data[i], properties[i].Type));
            }

            return obj;
        }

        private static object ConvertTo(string value, Type type)
        {
            return type == typeof(string) || type == typeof(object)
                       ? value
                       : type.IsEnum
                             ? Enum.Parse(type, value, true)
                             : IsForConversion(type)
                                   ? Convert.ChangeType(value, type)
                                   : ParseOrCreate(value, type);
        }

        private static object ParseOrCreate(string value, Type type)
        {
            var parse = type.GetMethod("Parse", new[] { typeof(string) });

            return parse != null ? parse.Invoke(null, new object[] { value }) : ToCostruct(new[] { value }, type);
        }

        private static object ToCostruct(IList<string> data, Type type)
        {
            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (data.Count != parameters.Length)
                    continue;

                try
                {
                    return Activator.CreateInstance(type,
                        data.Select((t, i) => ConvertTo(t, parameters[i].ParameterType)).ToArray());
                }
                catch (FormatException)
                {
                    continue;
                }
            }

            throw new Exception("No resolvable constructor found");
        }

        private static bool IsForConversion(Type type)
        {
            return type == typeof(bool) ||
                   type == typeof(char) ||
                   type == typeof(sbyte) ||
                   type == typeof(byte) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(string) ||
                   type == typeof(object);
        }
    }
}