using System;

namespace TinyGivenWhenThenParser.NetFramework.Tests.Unit
{
    public class HourOfDay
    {
        private readonly string _hourOfDay;

        public HourOfDay(string value)
        {
            _hourOfDay = value;
        }

        public TimeSpan Value
        {
            get { return DateTime.Parse(_hourOfDay).TimeOfDay; }
        }

        public override string ToString()
        {
            return _hourOfDay;
        }
    }
}