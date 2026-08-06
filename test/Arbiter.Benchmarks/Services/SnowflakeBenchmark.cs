using System.ComponentModel;

using Arbiter.Services;

using BenchmarkDotNet.Attributes;

namespace Arbiter.Benchmarks.Services;

[Description("Snowflake")]
[MemoryDiagnoser]
public class SnowflakeBenchmark
{
    private Snowflake _arbiterSnowflake = null!;

    [GlobalSetup]
    public void Setup()
    {
        var epoch = DateTimeOffset.FromUnixTimeMilliseconds(1288834974657);
        _arbiterSnowflake = new Snowflake(instanceId: 1, epoch: epoch.UtcDateTime);
    }

    [Benchmark(Baseline = true)]
    public long ArbiterSnowflakeNextId() => _arbiterSnowflake.NextId();
}
