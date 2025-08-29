using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.Benchmarks.Mapping;

public class PriorityAutoMapper(AutoMapper.IMapper mapper) : IMapper<Priority, PriorityReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public PriorityReadModel? Map(Priority? source)
    {
        if (source is null)
            return default;

        return mapper.Map<Priority, PriorityReadModel>(source)!;
    }

    public void Map(Priority source, PriorityReadModel destination)
    {
        mapper.Map(source, destination);
    }

    public IQueryable<PriorityReadModel> ProjectTo(IQueryable<Priority> source)
    {
        return mapper.ProjectTo<PriorityReadModel>(source);
    }
}
