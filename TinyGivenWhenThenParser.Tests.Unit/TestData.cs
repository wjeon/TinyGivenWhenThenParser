using System.Collections.Generic;

namespace TinyGivenWhenThenParser.Tests.Unit
{
    public class TestData
    {
        public TestData() { }

        public TestData(IList<string> data)
        {
            Name = data[0];
            Fruits = new Dictionary<string, int>
                {
                    { "Apple", int.Parse(data[1]) },
                    { "Orange", int.Parse(data[3]) }
                };
        }

        public string Name { get; set; }
        public IDictionary<string, int> Fruits { get; set; }
    }
}