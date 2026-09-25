using System.ComponentModel;

using Arbiter.Services;

using BenchmarkDotNet.Attributes;

using ArbiterCuid2 = Arbiter.Services.Cuid2;
using VisusCuid2 = Visus.Cuid.Cuid2;

namespace Arbiter.Benchmarks.Services;

[MemoryDiagnoser]
[Description("Cuid2")]
public class Cuid2Benchmark
{
    private Snowflake _snowflake = null!;

    [GlobalSetup]
    public void Setup()
    {
        var epoch = DateTimeOffset.FromUnixTimeMilliseconds(1288834974657);
        _snowflake = new Snowflake();
    }

    [Benchmark(Baseline = true)]
    public ArbiterCuid2 ArbiterCuid2New() => ArbiterCuid2.NewCuid();

    [Benchmark]
    public string ArbiterCuid2String() => ArbiterCuid2.NewCuid().ToString();

    [Benchmark]
    public VisusCuid2 CuidNetCuid2New() => new VisusCuid2();

    [Benchmark]
    public string CuidNetCuid2String() => new VisusCuid2().ToString();

    [Benchmark]
    public Guid GuidNewGuid() => Guid.NewGuid();

    [Benchmark]
    public string GuidNewGuidString() => Guid.NewGuid().ToString();

    [Benchmark]
    public long SnowflakeNextId() => _snowflake.NextId();

    [Benchmark]
    public string SnowflakeNextIdString() => _snowflake.NextId().ToString();

    [Benchmark]
    public Ulid UlidNewUlid() => Ulid.NewUlid();

    [Benchmark]
    public string UlidNewUlidString() => Ulid.NewUlid().ToString();

}
