using System.Buffers.Text;
using System.ComponentModel;

using BenchmarkDotNet.Attributes;

using ArbiterBase62 = Arbiter.Services.Base62;
using SimpleBase62 = SimpleBase.Base62;

namespace Arbiter.Benchmarks.Services;

[MemoryDiagnoser]
[Description("Base62")]
public class Base62Benchmark
{
    private byte[] _data = null!;

    private string _arbiterEncoded = null!;
    private string _simpleEncoded = null!;
    private string _base64UrlEncoded = null!;

    [Params(16, 128, 1024)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _data = new byte[DataSize];
        new Random(42).NextBytes(_data);

        _arbiterEncoded = ArbiterBase62.EncodeToString(_data);
        _simpleEncoded = SimpleBase62.Default.Encode(_data);
        _base64UrlEncoded = Base64Url.EncodeToString(_data);
    }

    [Benchmark(Baseline = true)]
    public string ArbiterBase62Encode() => ArbiterBase62.EncodeToString(_data);

    [Benchmark]
    public string SimpleBaseBase62Encode() => SimpleBase62.Default.Encode(_data);

    [Benchmark]
    public string Base64UrlEncode() => Base64Url.EncodeToString(_data);

    [Benchmark]
    public byte[] ArbiterBase62Decode() => ArbiterBase62.DecodeFromChars(_arbiterEncoded);

    [Benchmark]
    public byte[] SimpleBaseBase62Decode() => SimpleBase62.Default.Decode(_simpleEncoded);

    [Benchmark]
    public byte[] Base64UrlDecode() => Base64Url.DecodeFromChars(_base64UrlEncoded);
}
