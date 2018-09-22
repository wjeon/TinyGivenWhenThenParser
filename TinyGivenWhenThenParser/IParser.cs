namespace TinyGivenWhenThenParser
{
    public interface IParser<out TReturn>
    {
        TReturn Value { get; }
    }
}