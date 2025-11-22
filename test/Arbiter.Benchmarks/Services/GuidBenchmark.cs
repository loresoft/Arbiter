using System.ComponentModel;

using Arbiter.CommandQuery.Extensions;

using BenchmarkDotNet.Attributes;

namespace Arbiter.Benchmarks.Services;

[MemoryDiagnoser]
[Description("GUID Generation")]
public class GuidBenchmark
{
    [Benchmark(Baseline = true)]
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }

    [Benchmark]
    public Guid NewSequentialId()
    {
        return Guid.NewSequentialId();
    }

    [Benchmark]
    public Guid CreateVersion7()
    {
        return Guid.CreateVersion7();
    }
}
