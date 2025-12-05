using System.Globalization;
using System.Text;

using Arbiter.Services;

using BenchmarkDotNet.Attributes;

namespace Arbiter.Benchmarks.Services;

[MemoryDiagnoser]
public class CsvReaderBenchmark
{
    private string _csvContent = null!;
    private byte[] _csvBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _csvContent = """
        Name,Age,Email
        Alice,30,alice@example.com
        Bob,25,bob@example.com
        Charlie,35,charlie@example.com
        """.ReplaceLineEndings("\n");

        _csvBytes = Encoding.UTF8.GetBytes(_csvContent);
    }

    [Benchmark]
    public async Task<IReadOnlyList<Person>> ArbiterCsvReaderStream()
    {
        using var stream = new MemoryStream(_csvBytes, writable: false);

        return await CsvReader.ReadAsync(
            stream: stream,
            rowFactory: CreatePerson
        );
    }

    [Benchmark]
    public IReadOnlyList<Person> ArbiterCsvReaderSync()
    {
        var buffer = _csvContent.AsSpan();
        return CsvReader.Read(
            buffer,
            rowFactory: CreatePerson
        );
    }


    [Benchmark]
    public List<Person> CsvHelperReadRecords()
    {
        using var reader = new StringReader(_csvContent);
        using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<Person>().ToList();
    }


    private static Person CreatePerson(IReadOnlyList<string> fields, IReadOnlyList<string>? headers)
    {
        return new Person(
            Name: fields[0],
            Age: int.TryParse(fields[1], out var age) ? age : 0,
            Email: fields[2]);
    }

    public record Person(string Name, int Age, string Email);
}
