using System.Text;

using Arbiter.Services;

namespace Arbiter.Services.Tests;

public class CsvReaderTests
{
    public record Person(string? Name, int Age, string? Email);

    public record PersonWithReader(string? Name, int Age, string? Email) : ISupportReader<PersonWithReader>
    {
        public static PersonWithReader RowFactory(IReadOnlyList<string> row, IReadOnlyList<string>? headers = null)
        {
            return new PersonWithReader(
                Name: row.ElementAtOrDefault(0) ?? string.Empty,
                Age: int.TryParse(row.ElementAtOrDefault(1), out var age) ? age : 0,
                Email: row.ElementAtOrDefault(2) ?? string.Empty
            );
        }
    }

    private const string CsvWithHeaders = """
        Name,Age,Email
        Alice,30,alice@example.com
        Bob,25,bob@example.com
        Charlie,35,charlie@example.com
        """;

    private const string CsvWithSpecialChars = """
        Name,Age,Email
        "Alice, with comma",30,alice@example.com
        "Bob"" with quote",25,bob@example.com
        "Charlie
         with newline",35,charlie@example.com
        """;

    private const string CsvWithNulls = """
        Name,Age,Email
        Alice,30,
        ,25,bob@example.com
        """;

    private static Person CreatePerson(IReadOnlyList<string> fields, IReadOnlyList<string>? headers)
    {
        return new Person(
            Name: fields.ElementAtOrDefault(0) ?? string.Empty,
            Age: int.TryParse(fields.ElementAtOrDefault(1), out var age) ? age : 0,
            Email: fields.ElementAtOrDefault(2) ?? string.Empty
        );
    }

    [Test]
    public void Read_FromString_ReturnsCorrectObjects()
    {
        var people = CsvReader.Read(CsvWithHeaders, CreatePerson);

        people.Should().HaveCount(3);
        people[0].Should().Be(new Person("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new Person("Bob", 25, "bob@example.com"));
        people[2].Should().Be(new Person("Charlie", 35, "charlie@example.com"));
    }

    [Test]
    public async Task ReadAsync_FromTextReader_ReturnsCorrectObjects()
    {
        using var reader = new StringReader(CsvWithHeaders);
        var people = await CsvReader.ReadAsync(reader, CreatePerson);

        people.Should().HaveCount(3);
        people[0].Should().Be(new Person("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new Person("Bob", 25, "bob@example.com"));
        people[2].Should().Be(new Person("Charlie", 35, "charlie@example.com"));
    }

    [Test]
    public async Task ReadAsync_FromStream_ReturnsCorrectObjects()
    {
        var bytes = Encoding.UTF8.GetBytes(CsvWithHeaders);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync(stream, CreatePerson);

        people.Should().HaveCount(3);
        people[0].Should().Be(new Person("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new Person("Bob", 25, "bob@example.com"));
        people[2].Should().Be(new Person("Charlie", 35, "charlie@example.com"));
    }

    [Test]
    public void Read_WithSpecialCharacters_ParsesCorrectly()
    {
        var people = CsvReader.Read(CsvWithSpecialChars, CreatePerson);

        people.Should().HaveCount(3);
        people[0].Name.Should().Be("Alice, with comma");
        people[1].Name.Should().Be("Bob\" with quote");
        people[2].Name.Should().Contain($"Charlie{Environment.NewLine} with newline");
    }

    [Test]
    public void Read_WithNullValues_ParsesCorrectly()
    {
        var people = CsvReader.Read(CsvWithNulls, CreatePerson);

        people.Should().HaveCount(2);
        people[0].Should().Be(new Person("Alice", 30, ""));
        people[1].Should().Be(new Person("", 25, "bob@example.com"));
    }

    [Test]
    public void Read_RawRows_ReturnsStringArrays()
    {
        var rows = CsvReader.Read(CsvWithHeaders);

        rows.Should().HaveCount(4); // header + 3 data rows
        rows[0].Should().BeEquivalentTo(["Name", "Age", "Email"]);
        rows[1].Should().BeEquivalentTo(["Alice", "30", "alice@example.com"]);
        rows[2].Should().BeEquivalentTo(["Bob", "25", "bob@example.com"]);
        rows[3].Should().BeEquivalentTo(["Charlie", "35", "charlie@example.com"]);
    }

    [Test]
    public void Read_RawRows_WithSpecialCharacters_ReturnsCorrectly()
    {
        var rows = CsvReader.Read(CsvWithSpecialChars);

        rows.Should().HaveCount(4); // header + 3 data rows
        rows[0].Should().BeEquivalentTo(["Name", "Age", "Email"]);
        rows[1].Should().BeEquivalentTo(["Alice, with comma", "30", "alice@example.com"]);
        rows[2].Should().BeEquivalentTo(["Bob\" with quote", "25", "bob@example.com"]);
        rows[3].Should().BeEquivalentTo([$"Charlie{Environment.NewLine} with newline", "35", "charlie@example.com"]);
    }

    [Test]
    public void Read_EmptyString_ReturnsEmptyList()
    {
        var rows = CsvReader.Read(string.Empty, CreatePerson);
        rows.Should().BeEmpty();
    }

    [Test]
    public void Read_OnlyHeaders_NoDataRows()
    {
        var csv = "Name,Age,Email";
        var people = CsvReader.Read(csv, CreatePerson);
        people.Should().BeEmpty();
    }

    [Test]
    public void Read_NoHeaders_ParsesAllRows()
    {
        var csv = "Alice,30,alice@example.com\r\nBob,25,bob@example.com";
        var people = CsvReader.Read(csv, CreatePerson, hasHeader: false);

        people.Should().HaveCount(2);
        people[0].Should().Be(new Person("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new Person("Bob", 25, "bob@example.com"));
    }

    [Test]
    public void Read_HandlesEscapedQuotesAndLargeField()
    {
        var quoted = new string('"', 300) + "abc" + new string('"', 300);
        var csv = $"Name,Age,Email\n{quoted},30,alice@example.com";
        var people = CsvReader.Read(csv, CreatePerson);

        people.Should().HaveCount(1);
        people[0].Name.Should().Contain("abc");
    }

    [Test]
    public void Read_HandlesDifferentDelimiters()
    {
        var csv = "Name;Age;Email\nAlice;30;alice@example.com";
        var rows = CsvReader.Read(csv, delimiter: ';');
        rows.Should().HaveCount(2);
        rows[1].Should().BeEquivalentTo(["Alice", "30", "alice@example.com"]);
    }

    [Test]
    public void Read_HandlesEmptyLines()
    {
        var csv = "Name,Age,Email\n\nAlice,30,alice@example.com\n\n";
        var people = CsvReader.Read(csv, CreatePerson);

        people.Should().HaveCount(1);
        people[0].Should().Be(new Person("Alice", 30, "alice@example.com"));
    }

    [Test]
    public void Read_ThrowsOnNullParser()
    {
        Func<IReadOnlyList<string>, IReadOnlyList<string>?, Person> parser = null!;

        Assert.Throws<ArgumentNullException>(() => CsvReader.Read("a,b,c", parser!));
    }

    #region ISupportReader Tests

    [Test]
    public async Task ReadAsync_WithISupportReader_FromTextReader_ReturnsCorrectObjects()
    {
        using var reader = new StringReader(CsvWithHeaders);
        var people = await CsvReader.ReadAsync<PersonWithReader>(reader);

        people.Should().HaveCount(3);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new PersonWithReader("Bob", 25, "bob@example.com"));
        people[2].Should().Be(new PersonWithReader("Charlie", 35, "charlie@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromTextReader_NoHeaders_ReturnsCorrectObjects()
    {
        var csv = "Alice,30,alice@example.com\r\nBob,25,bob@example.com";
        using var reader = new StringReader(csv);
        var people = await CsvReader.ReadAsync<PersonWithReader>(reader, hasHeader: false);

        people.Should().HaveCount(2);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new PersonWithReader("Bob", 25, "bob@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromTextReader_WithSpecialCharacters_ParsesCorrectly()
    {
        using var reader = new StringReader(CsvWithSpecialChars);
        var people = await CsvReader.ReadAsync<PersonWithReader>(reader);

        people.Should().HaveCount(3);
        people[0].Name.Should().Be("Alice, with comma");
        people[1].Name.Should().Be("Bob\" with quote");
        people[2].Name.Should().Contain($"Charlie{Environment.NewLine} with newline");
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromTextReader_WithNullValues_ParsesCorrectly()
    {
        using var reader = new StringReader(CsvWithNulls);
        var people = await CsvReader.ReadAsync<PersonWithReader>(reader);

        people.Should().HaveCount(2);
        people[0].Should().Be(new PersonWithReader("Alice", 30, ""));
        people[1].Should().Be(new PersonWithReader("", 25, "bob@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromTextReader_EmptyString_ReturnsEmptyList()
    {
        using var reader = new StringReader(string.Empty);
        var people = await CsvReader.ReadAsync<PersonWithReader>(reader);
        people.Should().BeEmpty();
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromTextReader_OnlyHeaders_ReturnsEmptyList()
    {
        var csv = "Name,Age,Email";
        using var reader = new StringReader(csv);
        var people = await CsvReader.ReadAsync<PersonWithReader>(reader);
        people.Should().BeEmpty();
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromTextReader_WithCancellationToken()
    {
        using var reader = new StringReader(CsvWithHeaders);
        using var cts = new CancellationTokenSource();
        
        var people = await CsvReader.ReadAsync<PersonWithReader>(reader, cancellationToken: cts.Token);

        people.Should().HaveCount(3);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_ReturnsCorrectObjects()
    {
        var bytes = Encoding.UTF8.GetBytes(CsvWithHeaders);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream);

        people.Should().HaveCount(3);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new PersonWithReader("Bob", 25, "bob@example.com"));
        people[2].Should().Be(new PersonWithReader("Charlie", 35, "charlie@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_NoHeaders_ReturnsCorrectObjects()
    {
        var csv = "Alice,30,alice@example.com\r\nBob,25,bob@example.com";
        var bytes = Encoding.UTF8.GetBytes(csv);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream, hasHeader: false);

        people.Should().HaveCount(2);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
        people[1].Should().Be(new PersonWithReader("Bob", 25, "bob@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_WithSpecialCharacters_ParsesCorrectly()
    {
        var bytes = Encoding.UTF8.GetBytes(CsvWithSpecialChars);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream);

        people.Should().HaveCount(3);
        people[0].Name.Should().Be("Alice, with comma");
        people[1].Name.Should().Be("Bob\" with quote");
        people[2].Name.Should().Contain($"Charlie{Environment.NewLine} with newline");
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_WithNullValues_ParsesCorrectly()
    {
        var bytes = Encoding.UTF8.GetBytes(CsvWithNulls);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream);

        people.Should().HaveCount(2);
        people[0].Should().Be(new PersonWithReader("Alice", 30, ""));
        people[1].Should().Be(new PersonWithReader("", 25, "bob@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_WithCustomEncoding_ReturnsCorrectObjects()
    {
        var bytes = Encoding.ASCII.GetBytes(CsvWithHeaders);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream, encoding: Encoding.ASCII);

        people.Should().HaveCount(3);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_EmptyString_ReturnsEmptyList()
    {
        var bytes = Encoding.UTF8.GetBytes(string.Empty);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream);
        people.Should().BeEmpty();
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_OnlyHeaders_ReturnsEmptyList()
    {
        var csv = "Name,Age,Email";
        var bytes = Encoding.UTF8.GetBytes(csv);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream);
        people.Should().BeEmpty();
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_WithCancellationToken()
    {
        var bytes = Encoding.UTF8.GetBytes(CsvWithHeaders);
        using var stream = new MemoryStream(bytes);
        using var cts = new CancellationTokenSource();

        var people = await CsvReader.ReadAsync<PersonWithReader>(stream, cancellationToken: cts.Token);

        people.Should().HaveCount(3);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
    }

    [Test]
    public async Task ReadAsync_WithISupportReader_FromStream_WithNullEncoding_UsesUTF8()
    {
        var bytes = Encoding.UTF8.GetBytes(CsvWithHeaders);
        using var stream = new MemoryStream(bytes);
        var people = await CsvReader.ReadAsync<PersonWithReader>(stream, encoding: null);

        people.Should().HaveCount(3);
        people[0].Should().Be(new PersonWithReader("Alice", 30, "alice@example.com"));
    }

    [Test]
    public void ReadAsync_WithISupportReader_FromTextReader_ThrowsOnNullReader()
    {
        TextReader reader = null!;
        Assert.ThrowsAsync<ArgumentNullException>(() => CsvReader.ReadAsync<PersonWithReader>(reader));
    }

    [Test]
    public void ReadAsync_WithISupportReader_FromStream_ThrowsOnNullStream()
    {
        Stream stream = null!;
        Assert.ThrowsAsync<ArgumentNullException>(() => CsvReader.ReadAsync<PersonWithReader>(stream));
    }

    #endregion
}
