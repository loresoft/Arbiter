using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.Mapping;

/// <summary>
/// Provides a default implementation of <see cref="IMapper"/> that resolves mapping services using an <see cref="IServiceProvider"/>.
/// </summary>
/// <param name="serviceProvider">The service provider used to resolve mapping services.</param>
public sealed class ServiceProviderMapper(IServiceProvider serviceProvider) : IMapper
{
    /// <summary>
    /// Maps the specified <typeparamref name="TSource"/> object to a new <typeparamref name="TDestination"/> object using a registered <see cref="IMapper{TSource, TDestination}"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <returns>The mapped destination object, or <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public TDestination? Map<TSource, TDestination>(TSource? source)
    {
        if (source is null)
            return default;

        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        return mapper.Map(source);
    }

    /// <summary>
    /// Maps the specified <typeparamref name="TSource"/> object into the provided <typeparamref name="TDestination"/> object using a registered <see cref="IMapper{TSource, TDestination}"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The destination object to map into.</param>
    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        mapper.Map(source, destination);
    }

    /// <summary>
    /// Projects the elements of the specified <see cref="IQueryable{TSource}"/> to <see cref="IQueryable{TDestination}"/> using a registered <see cref="IMapper{TSource, TDestination}"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source queryable to project from.</param>
    /// <returns>A queryable of the mapped destination type.</returns>
    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper<TSource, TDestination>>();
        return mapper.ProjectTo(source);
    }
}
