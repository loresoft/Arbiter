using System.ComponentModel;

using BenchmarkDotNet.Attributes;

namespace Arbiter.Benchmarks.Mapping;

[Description("MappingBenchmark")]
public class MappingBenchmark
{
    private Priority[] _priorities = null!;

    private PriorityAutoMapper _priorityAutoMapper = null!;
    private PriorityBaseMapper _priorityBaseMapper = null!;
    private PriorityMapperly _priorityMapperly = null!;
    private PriorityManualMapper _priorityManualMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<PriorityProfile>());
        var mapper = config.CreateMapper();

        _priorityAutoMapper = new PriorityAutoMapper(mapper);
        _priorityBaseMapper = new PriorityBaseMapper();
        _priorityMapperly = new PriorityMapperly();
        _priorityManualMapper = new PriorityManualMapper();

        _priorities = Enumerable
            .Range(1, 10)
            .Select(i => new Priority
            {
                Name = $"Name {i:00}",
                Description = $"Description {i:00}",
                DisplayOrder = i,
                IsActive = i % 2 == 0,
                Id = Guid.NewGuid().ToString(),
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "Benchmark",
                Updated = DateTimeOffset.UtcNow,
                UpdatedBy = "Benchmark"

            })
            .ToArray();
    }

    [Benchmark]
    public PriorityReadModel? PriorityAutoMapper_SingleMap()
    {
        return _priorityAutoMapper.Map(_priorities[0]);
    }

    [Benchmark]
    public PriorityReadModel? PriorityBaseMapper_SingleMap()
    {
        return _priorityBaseMapper.Map(_priorities[0]);
    }

    [Benchmark]
    public PriorityReadModel? PriorityMapperly_SingleMap()
    {
        return _priorityMapperly.Map(_priorities[0]);
    }

    [Benchmark]
    public PriorityReadModel PriorityManualMapper_SingleMap()
    {
        return _priorityManualMapper.Map(_priorities[0]);
    }

    [Benchmark]
    public PriorityReadModel PriorityAutoMapper_MapToExisting()
    {
        var destinations = new PriorityReadModel();
        _priorityAutoMapper.Map(_priorities[0], destinations);
        return destinations;
    }

    [Benchmark]
    public PriorityReadModel PriorityBaseMapper_MapToExisting()
    {
        var destinations = new PriorityReadModel();
        _priorityBaseMapper.Map(_priorities[0], destinations);
        return destinations;
    }

    [Benchmark]
    public PriorityReadModel PriorityMapperly_MapToExisting()
    {
        var destinations = new PriorityReadModel();
        _priorityMapperly.Map(_priorities[0], destinations);
        return destinations;
    }

    [Benchmark]
    public PriorityReadModel PriorityManualMapper_MapToExisting()
    {
        var destinations = new PriorityReadModel();
        _priorityManualMapper.Map(_priorities[0], destinations);
        return destinations;
    }

    [Benchmark]
    public PriorityReadModel[] PriorityAutoMapper_ProjectTo()
    {
        return _priorityAutoMapper.ProjectTo(_priorities.AsQueryable()).ToArray();
    }

    [Benchmark]
    public PriorityReadModel[] PriorityBaseMapper_ProjectTo()
    {
        return _priorityBaseMapper.ProjectTo(_priorities.AsQueryable()).ToArray();
    }

    [Benchmark]
    public PriorityReadModel[] PriorityMapperly_ProjectTo()
    {
        return _priorityMapperly.ProjectTo(_priorities.AsQueryable()).ToArray();
    }

    [Benchmark]
    public PriorityReadModel[] PriorityManualMapper_ProjectTo()
    {
        return _priorityManualMapper.ProjectTo(_priorities.AsQueryable()).ToArray();
    }
}
