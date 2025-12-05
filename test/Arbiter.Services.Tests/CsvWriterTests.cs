using Arbiter.Services;

namespace Arbiter.Services.Tests;

public class CsvWriterTests
{
    [Test]
    public async Task WriteAsyncWithTextWriteTest()
    {
        string[] headers = [nameof(Person.Name), nameof(Person.Age), nameof(Person.Email)];

        List<Person> people =
        [
            new("Alice", 30, "alice@example.com"),
            new("Bob", 25, "bob@example.com"),
            new("Charlie", 35, "charlie@example.com")
        ];

        var csvContent = await CsvWriter.WriteAsync(
            headers: headers,
            rows: people,
            selector: static p => [p.Name, p.Age.ToString(), p.Email]
        );

        csvContent.Should().NotBeNullOrEmpty();
        csvContent.Should().Contain("Name,Age,Email");
        csvContent.Should().Contain("Alice,30,alice@example.com");
        csvContent.Should().Contain("Bob,25,bob@example.com");
        csvContent.Should().Contain("Charlie,35,charlie@example.com");
    }

    [Test]
    public async Task WriteAsyncWithSupportWriterTest()
    {
        List<Person> people =
        [
            new("Alice", 30, "alice@example.com"),
            new("Bob", 25, "bob@example.com"),
            new("Charlie", 35, "charlie@example.com")
        ];

        var csvContent = await CsvWriter.WriteAsync(people);

        csvContent.Should().NotBeNullOrEmpty();
        csvContent.Should().Contain("Name,Age,Email");
        csvContent.Should().Contain("Alice,30,alice@example.com");
        csvContent.Should().Contain("Bob,25,bob@example.com");
        csvContent.Should().Contain("Charlie,35,charlie@example.com");
    }

    [Test]
    public async Task WriteAsyncWithStreamWritesCorrectly()
    {
        List<Person> people =
        [
            new("Alice", 30, "alice@example.com"),
            new("Bob", 25, "bob@example.com"),
            new("Charlie", 35, "charlie@example.com")
        ];

        using var memoryStream = new MemoryStream();
        await CsvWriter.WriteAsync(memoryStream, people);

        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream);
        var csvContent = await reader.ReadToEndAsync();

        csvContent.Should().NotBeNullOrEmpty();
        csvContent.Should().Contain("Name,Age,Email");
        csvContent.Should().Contain("Alice,30,alice@example.com");
        csvContent.Should().Contain("Bob,25,bob@example.com");
        csvContent.Should().Contain("Charlie,35,charlie@example.com");
    }

    [Test]
    public async Task WriteAsyncWithTextWriterWritesCorrectly()
    {
        List<Person> people =
        [
            new("Alice", 30, "alice@example.com"),
            new("Bob", 25, "bob@example.com"),
            new("Charlie", 35, "charlie@example.com")
        ];

        using var stringWriter = new StringWriter();
        await CsvWriter.WriteAsync(stringWriter, people);
        var csvContent = stringWriter.ToString();

        csvContent.Should().NotBeNullOrEmpty();
        csvContent.Should().Contain("Name,Age,Email");
        csvContent.Should().Contain("Alice,30,alice@example.com");
        csvContent.Should().Contain("Bob,25,bob@example.com");
        csvContent.Should().Contain("Charlie,35,charlie@example.com");
    }

    [Test]
    public async Task WriteAsyncWithSpecialCharactersHandlesCorrectly()
    {
        List<Person> people =
        [
            new("Alice, with comma", 30, "alice@example.com"),
            new("Bob\" with quote", 25, "bob@example.com"),
            new("Charlie\n with newline", 35, "charlie@example.com")
        ];

        var csvContent = await CsvWriter.WriteAsync(
            headers: [nameof(Person.Name), nameof(Person.Age), nameof(Person.Email)],
            rows: people,
            selector: static p => [p.Name, p.Age.ToString(), p.Email]
        );

        csvContent.Should().NotBeNullOrEmpty();
        csvContent.Should().Contain("\"Alice, with comma\",30,alice@example.com");
        csvContent.Should().Contain("\"Bob\"\" with quote\",25,bob@example.com");
        csvContent.Should().Contain("\"Charlie\n with newline\",35,charlie@example.com");
    }

    [Test]
    public async Task WriteAsyncWithNullValuesHandlesCorrectly()
    {
        List<Person> people =
        [
            new("Alice", 30, null),
            new(null, 25, "bob@example.com"),
        ];

        var csvContent = await CsvWriter.WriteAsync(
            headers: [nameof(Person.Name), nameof(Person.Age), nameof(Person.Email)],
            rows: people,
            selector: static p => [p.Name, p.Age.ToString(), p.Email]
        );

        csvContent.Should().NotBeNullOrEmpty();
        csvContent.Should().Contain("Alice,30,");
        csvContent.Should().Contain(",25,bob@example.com");
    }
}

public record Person(string? Name, int Age, string? Email)
    : ISupportWriter<Person>
{
    public static IEnumerable<string>? Headers()
        => [nameof(Name), nameof(Age), nameof(Email)];

    public static Func<Person, IEnumerable<string?>> RowSelector()
        => static p => [p.Name, p.Age.ToString(), p.Email];
}
