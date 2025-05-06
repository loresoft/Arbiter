using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

namespace Tracker.WebService.Adapters;

[RegisterSingleton<IMapper>(Duplicate = DuplicateStrategy.Replace)]
internal class AutoMapperAdapter(AutoMapper.IMapper mapper) : IMapper
{
    [return: NotNullIfNotNull(nameof(source))]
    public TDestination? Map<TSource, TDestination>(TSource? source)
    {
        if (source is null)
            return default;

        return mapper.Map<TSource, TDestination>(source)!;
    }

    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        mapper.Map(source, destination);
    }

    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
    {
        return mapper.ProjectTo<TDestination>(source);
    }
}
