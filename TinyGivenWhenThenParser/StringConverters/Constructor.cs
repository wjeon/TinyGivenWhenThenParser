using System;
using System.Collections.Generic;
using System.Linq;
using TinyGivenWhenThenParser.Exceptions;

namespace TinyGivenWhenThenParser.StringConverters
{
    internal static class Constructor
    {
        public static T ToCostruct<T>(this IList<string> data)
        {
            return (T)data.ToCostruct(typeof(T));
        }

        public static object ToCostruct(this IList<string> data, Type type)
        {
            if (type.FullName.StartsWith("System.Nullable`") &&
                data.Count == 1 && string.IsNullOrEmpty(data.First()))
                return null;

            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (data.Count != parameters.Length)
                    continue;

                try
                {
                    return Activator.CreateInstance(type,
                        data.Select((t, i) => Converter.ConvertTo(t, parameters[i].ParameterType)).ToArray());
                }
                catch (FormatException)
                {
                    continue;
                }
            }

            throw new NoResolvableConstructorException("No resolvable constructor found");
        }
    }
}