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

        public static T ToCostruct<T>(this IDictionary<string, object> data)
        {
            return (T)data.ToCostruct(typeof(T));
        }

        private static object ToCostruct(this IDictionary<string, object> data, Type type)
        {
            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (data.Count != parameters.Length)
                    continue;

                var pars = data.Where(d => parameters.Select(p => p.Name).Contains(d.Key));

                if (pars.Count() != parameters.Length)
                    continue;

                try
                {
                    return Activator.CreateInstance(type,
                        parameters.Select(par => pars.First(p => p.Key == par.Name).Value).ToArray());
                }
                catch (FormatException)
                {
                    continue;
                }
            }

            throw new NoResolvableConstructorException("No resolvable constructor found");
        }

        public static T ToCreate<T>(this IDictionary<string, object> data)
        {
            return (T)data.ToCreate(typeof(T));
        }

        private static object ToCreate(this IDictionary<string, object> data, Type type)
        {
            var constructors = type.GetConstructors();

            if (constructors.Length > 0 && constructors.All(c => c.GetParameters().Length > 0))
                throw new Exception("No constructor or parameterless constructor required");

            var properties = type.GetProperties();

            var pros = data.Where(d => properties.Select(p => p.Name).Contains(d.Key));

            if (pros.Count() != data.Keys.Count)
                throw new Exception("Not all the properties found");

            var obj = Activator.CreateInstance(type);

            foreach (var property in pros)
            {
                properties.First(p => p.Name == property.Key).SetValue(obj, property.Value);
            }

            return obj;
        }
    }
}