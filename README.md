# TinyGivenWhenThenParser

[![Build status](https://ci.appveyor.com/api/projects/status/x1lc46up8vrkmwd5?svg=true)](https://ci.appveyor.com/project/wjeon/tinygivenwhenthenparser)



***Summary***

TinyGivenWhenThenParser is a lightweight, fluent Given-When-Then test parser library for .NET. It allows you to build clean, human-readable Behavior-Driven Development (BDD) style scenarios directly within your existing unit tests without the complex setup, heavy installations, or external dependencies required by full frameworks like SpecFlow.

***Why Use TinyGivenWhenThenParser?***

When writing tests, full-scale BDD frameworks can introduce unwanted overhead, heavy extensions, and rigid tool setups. TinyGivenWhenThenParser provides a simple, regex-pattern-based alternative to map user story strings directly to executable test methods.

Use TinyGivenWhenThenParser when you want to:

- Parse structured BDD data from strings or multi-line test cases using Regex patterns.
- Map parsed string values into dynamic properties or construct custom strongly-typed objects on the fly.
- Extract tabular GWT row data effortlessly into list collections or strongly-typed vertical/horizontal data tables.
- Avoid installing complex third-party IDE plugins, maintaining seamless portability for anyone pulling your repository.

TinyGivenWhenThenParser gives you complete data extraction power (strings, collections, custom types, and data tables) while maintaining the pure speed of your favorite vanilla unit test runners.

***NuGet Package***

https://nuget.org

***Automatic 'And' Context Swapping***

When parsing multi-line scenarios, the parser intelligently replaces leading `And` tokens with the preceding line's context (`Given`, `When`, or `Then`) automatically. This lets you write natural-looking, flowing text blocks while mapping lines cleanly against uniform regex definitions.

## Example Usage:

***Initialize a GWT Parser with a Test Case***

```csharp
[SetUp]
public void SetUp()
{
    const string multilineCase = @"Given Tom has 2 apples and 3 oranges
                                   And Jerry has 1 apple and 1 orange";
                                   
    // Initialize parser with the string scenario to analyze
    _gwtParser = TinyGWTParser.WithTestCase(multilineCase);
}
```

***Parse Simple Data from a Single Matching Line***

```csharp
[Test]
public void Your_test_method()
{
    // Define your Regex pattern with capture groups
    var parseResult = _gwtParser
        .WithPattern(@"^Given (.*) has (\d+) apple(?:s|) and (\d+) orange(?:s|)$")
        .ParseSingleLine();

    // Access parsed string arrays via the result object
    string[] lineData = parseResult.ParsedData.Line; 
    // Result: ["Tom", "2", "3"]
}
```

***Parse Multiple Lines matching a Pattern (with Automatic Context Swap)***

```csharp
[Test]
public void Your_test_method()
{
    // Automatically transforms 'And' lines into their preceding 'Given' or 'When/Then' keys
    var parseResult = _gwtParser
        .WithPattern(@"Given (.*) has (\d+) apple(?:s|) and (\d+) orange(?:s|)")
        .ParseMultiLines(From.TestCaseReplacedAndWithGivenWhenThen);

    // Iterates through every matching scenario group captured
    foreach(var match in parseResult.ParsedData)
    {
        string[] lineData = match.Line;
        // 1st Loop: ["Tom", "2", "3"]
        // 2nd Loop: ["Jerry", "1", "1"]
    }
}
```

***Dynamically Convert Parsed Tokens into Specific Types (`.To` Method)***

```csharp
[Test]
public void Your_test_method()
{
    const string @case = "Given test data - Name: Tom, Age: 2, Favorite Fruit: apple, ID: 6579328A-B45A-48EB-BC1C-68018157F47A";
    var parser = TinyGWTParser.WithTestCase(@case);

    // Bind capture strings directly to specific keys and primitive/complex types
    var result = parser.WithPattern(@"Given test data - Name: (.*), Age: (\d+), Favorite Fruit: (apple|orange), ID: (.*)")
        .To("Name".As<string>(), "Age".As<int>(), "FavoriteFruit".As<Fruit>(), "Id".As<Guid>())
        .ParseSingleLine();

    // Access values cleanly as properties via a dynamically typed output object
    var name = result.ParsedData.Line.Name; // "Tom" (string)
    var age = result.ParsedData.Line.Age;   // 2 (int)
}
```

***Construct Strongly-Typed Class Instances or Properties directly***

```csharp
[Test]
public void Your_test_method()
{
    // Instruct the parser to map the parameters automatically via Using.Constructor or Using.Properties
    var result = _gwtParser
        .WithPattern(@"Given (.*) has (\d+) apple(?:s|) and (\d+) orange(?:s|)")
        .To("name".As<string>(), "quantity".As<int>(), "favoriteFruit".As<Fruit>(), "date".As<DateTime>(), "time".As<TimeSpan>(), "dateTimeOffset".As<DateTimeOffset>(), "id".As<Guid>(), "hourOfDay".As<HourOfDay>())
        .ParseMultiLines<ObjectWithConstructor>(Using.Constructor);
        
    // Optionally inject static contextual variables not present in the text string
    var advancedResult = _gwtParser
        .WithPattern(@"Given Time: (.*)")
        .To("time".As<TimeSpan>())
        .WithAdditionalValuesOf("Tom".For("name"), Fruit.Apple.For("favoriteFruit"))
        .ParseSingleLine<ObjectWithConstructor>(Using.Constructor);
}
```

***Parse Markdown GWT Tables into Strongly-Typed Collections***

```csharp
[Test]
public void Your_test_method()
{
    const string tableCase = @"Given following attendees
                               +-------+------+----------+
                               | name  | team | title    |
                               +-------+------+----------+
                               | Tom   | 6    | TechLead |
                               | Jerry | 2    | Manager  |
                               +-------+------+----------+";

    var parser = TinyGWTParser.WithTestCase(tableCase);

    // Directly bind GWT Markdown tables (supports horizontal and vertical flipped tables out of the box)
    var result = parser.WithPattern(@"^Given following attendees$")
                       .ParseSingleLine()
                       .WithTableOf<Attendee>();

    var firstAttendeeName = result.ParsedData.Table.First().Name; // "Tom"
    var firstAttendeeTeam = result.ParsedData.Table.First().Team; // 6
}
```

***Additional Notes***

- **Custom Mappers:** You can declare custom mapper definitions by implementing `IParser<T>` to handle custom text patterns (like reading complex `DateTime` structures) effortlessly.
- **Safe Evaluation:** If a scenario text block contains no matching lines, the dynamic parser gracefully initializes fields with their `default(T)` equivalents instead of crashing your suite with unhandled exceptions.