﻿namespace TinyGivenWhenThenParser.Extensions
{
    public static class StringExtensions
    {
        public static Property As<T>(this string name)
        {
            return new Property(name, typeof(T));
        }
    }
}