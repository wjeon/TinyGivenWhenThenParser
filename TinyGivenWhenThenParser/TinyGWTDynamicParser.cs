using System;
using System.Collections.Generic;
using System.Linq;
using TinyGivenWhenThenParser.Results;
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

        public ParseResult<ParsedData<dynamic, IEnumerable<IEnumerable<string>>>, dynamic> ParseSingleLine(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = _parser.ParseSingleLine(testCase);

            return new ParseResult<ParsedData<dynamic, IEnumerable<IEnumerable<string>>>, dynamic>(
                result.Parsed,
                new ParsedData<dynamic, IEnumerable<IEnumerable<string>>>(
                    result.ParsedData == null
                    ? null
                    : result.ParsedData.Line != null && result.ParsedData.Line.Any()
                            ? ParseFrom(result.ParsedData.Line, _parser.Properties)
                            : null,
                result.ParsedData.Table));
        }

        public ParseResult<ParsedData<T, IEnumerable<IEnumerable<string>>>, T> ParseSingleLine<T>(Using createObjectUsing, From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseSingleLine(testCase);

            IDictionary<string, object> data = Merge(result.ParsedData.Line, _additionalParameters);

            return new ParseResult<ParsedData<T, IEnumerable<IEnumerable<string>>>, T>(
                result.Parsed,
                new ParsedData<T, IEnumerable<IEnumerable<string>>>(
                    createObjectUsing == Using.Properties
                    ? data.ToCreate<T>()
                    : data.ToCostruct<T>(),
                result.ParsedData.Table));
        }

        public ParseResults<IEnumerable<ParsedData<dynamic, IEnumerable<IEnumerable<string>>>>, dynamic> ParseMultiLines(From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = _parser.ParseMultiLines(testCase);

            return new ParseResults<IEnumerable<ParsedData<dynamic, IEnumerable<IEnumerable<string>>>>, dynamic>(
                result.Parsed,
                result.ParsedData.Any()
                    ? result.ParsedData.Select(d =>
                        new ParsedData<dynamic, IEnumerable<IEnumerable<string>>>(
                            d.Line.Any()
                            ? ParseFrom(d.Line, _parser.Properties) : EmptyDynamicObjectOf(_parser.Properties), d.Table))
                    : Enumerable.Empty<ParsedData<dynamic, IEnumerable<IEnumerable<string>>>>());
        }

        public ParseResults<IEnumerable<ParsedData<T, IEnumerable<IEnumerable<string>>>>, T> ParseMultiLines<T>(Using createObjectUsing, From testCase = From.TestCaseReplacedAndWithGivenWhenThen)
        {
            var result = ParseMultiLines(testCase);

            return new ParseResults<IEnumerable<ParsedData<T, IEnumerable<IEnumerable<string>>>>, T>(
                result.Parsed,
                result.Parsed
                    ? result.ParsedData.Select(d => new ParsedData<T, IEnumerable<IEnumerable<string>>> (
                        (T)Create<T>(Merge(d.Line, _additionalParameters), createObjectUsing), d.Table))
                    : Enumerable.Empty<ParsedData<T, IEnumerable<IEnumerable<string>>>>());
        }

        [Obsolete]
        public TinyGWTDynamicParser With(params KeyValuePair<string, object>[] parameters)
        {
            if (parameters != null && parameters.Any())
                _additionalParameters.AddRange(parameters);

            return this;
        }

        public TinyGWTDynamicParser WithAdditionalValuesOf(params KeyValuePair<string, object>[] parameters)
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

        private static dynamic EmptyDynamicObjectOf(IList<Property> properties)
        {
            dynamic obj = new System.Dynamic.ExpandoObject();

            foreach (var t in properties)
            {
                ((IDictionary<string, object>)obj)
                    .Add(t.Name, t.Type.IsValueType ? Activator.CreateInstance(t.Type) : null);
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