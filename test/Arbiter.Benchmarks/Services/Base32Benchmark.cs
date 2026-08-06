using System.Buffers.Text;
using System.ComponentModel;

using BenchmarkDotNet.Attributes;

using ArbiterBase32 = Arbiter.Services.Base32;
using SimpleBase32 = SimpleBase.Base32;

namespace Arbiter.Benchmarks.Services;

[MemoryDiagnoser]
[Description("Base32")]
public class Base32Benchmark
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

        _arbiterEncoded = ArbiterBase32.EncodeToString(_data);
        _simpleEncoded = SimpleBase32.Crockford.Encode(_data);
        _base64UrlEncoded = Base64Url.EncodeToString(_data);
    }

    [Benchmark(Baseline = true)]
    public string ArbiterBase32Encode() => ArbiterBase32.EncodeToString(_data);

    [Benchmark]
    public string SimpleBaseBase32Encode() => SimpleBase32.Crockford.Encode(_data);

    [Benchmark]
    public string Base64UrlEncode() => Base64Url.EncodeToString(_data);

    [Benchmark]
    public byte[] ArbiterBase32Decode() => ArbiterBase32.DecodeFromChars(_arbiterEncoded);

    [Benchmark]
    public byte[] SimpleBaseBase32Decode() => SimpleBase32.Crockford.Decode(_simpleEncoded);

    [Benchmark]
    public byte[] Base64UrlDecode() => Base64Url.DecodeFromChars(_base64UrlEncoded);
}
