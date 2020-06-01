using System.Collections.Generic;

namespace TinyGivenWhenThenParser.NetFramework.Tests.Unit
{
    public class TestData
    {
        public TestData() { }

        public TestData(string name, int apples, int oranges)
        {
            Name = name;
            Fruits = new Dictionary<string, int>
                {
                    { "Apple", apples },
                    { "Orange", oranges }
                };
        }

        public string Name { get; set; }
        public IDictionary<string, int> Fruits { get; set; }
    }
}