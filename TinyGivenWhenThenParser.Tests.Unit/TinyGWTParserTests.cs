using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace TinyGivenWhenThenParser.Tests.Unit
{
    [TestFixture]
    public class TinyGWTParserTests
    {
        [TestCase(@"Given Tom has 2 apples and 3 oranges", "Tom", 2, 3, "s")]
        [TestCase(@"Given Jerry has 1 apple and 1 orange", "Jerry", 1, 1, "")]
        public void Parse_data_from_a_sentence_correctly(string @case, string name, int numberOfApples, int numberOfOranges, string ignore)
        {
            var gwtParser = TinyGWTParser.WithTestCase(@case);

            const string pattern = @"^Given (.*) has (\d+) apple(s|) and (\d+) orange(s|)$";

            var parseResult = gwtParser.WithPattern(pattern).ParseData();

            var expectedData = new List<string> {name, numberOfApples.ToString(), ignore, numberOfOranges.ToString(), ignore};

            parseResult.ShouldAllBeEquivalentTo(expectedData);
        }
    }
}
