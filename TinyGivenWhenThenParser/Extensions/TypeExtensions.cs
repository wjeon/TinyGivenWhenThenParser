using System;

namespace TinyGivenWhenThenParser.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsForConversion(this Type type)
        {
            return type == typeof(bool) ||
                   type == typeof(char) ||
                   type == typeof(sbyte) ||
                   type == typeof(byte) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(string) ||
                   type == typeof(object);
        }
    }
}