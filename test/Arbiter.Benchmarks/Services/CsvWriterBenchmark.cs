using System.Globalization;

using Arbiter.Services;

using BenchmarkDotNet.Attributes;

namespace Arbiter.Benchmarks.Services;

[MemoryDiagnoser]
public class CsvWriterBenchmark
{
    private List<Person> _people = null!;
    private string[]? _headers;

    [GlobalSetup]
    public void Setup()
    {
        _headers = [nameof(Person.Name), nameof(Person.Age), nameof(Person.Email)];
        _people =
        [
            new("Alice", 30, "alice@example.com"),
            new("Bob", 25, "bob@example.com"),
            new("Charlie", 35, "charlie@example.com")
        ];
    }

    [Benchmark]
    public async Task<string> ArbiterCsvWriterSelector()
    {
        return await CsvWriter.WriteAsync(
            headers: _headers,
            rows: _people,
            selector: static p => [p.Name, p.Age.ToString(), p.Email]
        );
    }

    [Benchmark]
    public async Task<string> ArbiterCsvWriterInterface()
    {
        return await CsvWriter.WriteAsync(_people);
    }

    [Benchmark]
    public string CsvHelperWriteRecords()
    {
        using var writer = new StringWriter();
        using var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(_people);

        writer.Flush();
        return writer.ToString();
    }

    [Benchmark]
    public string CsvHelperWriteField()
    {
        using var writer = new StringWriter();
        using var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write header
        csv.WriteField("Name");
        csv.WriteField("Age");
        csv.WriteField("Email");
        csv.NextRecord();

        // Write records
        foreach (var p in _people)
        {
            csv.WriteField(p.Name);
            csv.WriteField(p.Age);
            csv.WriteField(p.Email);
            csv.NextRecord();
        }

        writer.Flush();
        return writer.ToString();
    }

    public record Person(string Name, int Age, string Email) : ISupportWriter<Person>
    {
        public static IEnumerable<string>? Headers()
            => [nameof(Name), nameof(Age), nameof(Email)];

        public static Func<Person, IEnumerable<string?>> RowSelector()
            => static p => [p.Name, p.Age.ToString(), p.Email];
    }
}
