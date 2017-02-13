using System;

namespace TinyGivenWhenThenParser
{
    public class Property
    {
        public Property(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }
    }
}