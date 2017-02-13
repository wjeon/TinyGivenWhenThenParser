﻿using System.Collections.Generic;
using System.Linq;
using TinyGivenWhenThenParser.StringConverters;

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
                    .Add(properties[i].Name, data[i].ConvertTo(properties[i].Type));
            }

            return obj;
        }
    }
}