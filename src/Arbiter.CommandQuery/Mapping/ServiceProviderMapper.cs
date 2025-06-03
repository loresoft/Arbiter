using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.Mapping;

/// <summary>
/// A default mapper that uses a service provider to resolve the mapping.
/// </summary>
/// <param name="serviceProvider">Service provider to use for resolving the mapping</param>
public sealed class ServiceProviderMapper(IServiceProvider serviceProvider) : IMapper
{
    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(source))]
    public TDestination? Map<TSource, TDestination>(TSource? source)
    {
        if (source is null)
            return default;

        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        return mapper.Map(source);
    }

    /// <inheritdoc />
    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        mapper.Map(source, destination);
    }

    /// <inheritdoc />
    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        return mapper.ProjectTo(source);
    }
}
