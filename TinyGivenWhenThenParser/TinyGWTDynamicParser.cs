using System.Collections.Generic;
using System.Linq;
using TinyGivenWhenThenParser.StringConverters;

namespace TinyGivenWhenThenParser
{
    public class TinyGWTDynamicParser
    {
        private readonly TinyGWTParser _parser;
        private readonly List<KeyValuePair<string, object>> _additionalParameters;


        public TinyGWTDynamicParser(TinyGWTParser parser)
        {
            _parser = parser;
            _additionalParameters = new List<KeyValuePair<string, object>>();
        }

        public ParseResult<dynamic> ParseSingleLine(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = _parser.ParseSingleLine(testCase);

            return new ParseResult<dynamic>(result.Parsed,
                                            result.Data.Any() ? ParseFrom(result.Data, _parser.Properties) : null);
        }

        public ParseResult<T> ParseSingleLine<T>(Using createObjectUsing, From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseSingleLine(testCase);

            IDictionary<string, object> data = Merge(result.Data, _additionalParameters);

            return new ParseResult<T>(result.Parsed,
                                      createObjectUsing == Using.Properties
                                          ? data.ToCreate<T>()
                                          : data.ToCostruct<T>());
        }

        public ParseResult<IEnumerable<dynamic>> ParseMultiLines(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = _parser.ParseMultiLines(testCase);

            return new ParseResult<IEnumerable<dynamic>>(result.Parsed,
                                            result.Data.Any()
                                            ? result.Data.Select(d => d.Any() ? ParseFrom(d, _parser.Properties) : null)
                                            : new List<dynamic>());
        }

        public ParseResult<IEnumerable<T>> ParseMultiLines<T>(Using createObjectUsing, From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseMultiLines(testCase);

            return new ParseResult<IEnumerable<T>>(result.Parsed,
                                            result.Parsed
                                            ? result.Data.Select(d => (T)Create<T>(Merge(d, _additionalParameters), createObjectUsing))
                                            : new List<T>());
        }

        public TinyGWTDynamicParser With(params KeyValuePair<string, object>[] parameters)
        {
            if (parameters != null && parameters.Any())
                _additionalParameters.AddRange(parameters);

            return this;
        }

        private static IDictionary<string, object> Merge(IDictionary<string, object> data, IEnumerable<KeyValuePair<string, object>> additionalParams)
        {
            return data.Concat(additionalParams).ToDictionary(p => p.Key, pair => pair.Value);
        }

        private static T Create<T>(IDictionary<string, object> data, Using createObjectUsing)
        {
            return createObjectUsing == Using.Properties
                ? data.ToCreate<T>()
                : data.ToCostruct<T>();
        }

        private static dynamic ParseFrom(IList<string> data, IList<Property> properties)
        {
            dynamic obj = new System.Dynamic.ExpandoObject();

            for (var i = 0; i < properties.Count; i++)
            {
                ((IDictionary<string, object>)obj)
                    .Add(properties[i].Name, data[i].ConvertTo(properties[i].Type));
            }

            return obj;
        }
    }

    public enum Using
    {
        Constructor,
        Properties
    }
}