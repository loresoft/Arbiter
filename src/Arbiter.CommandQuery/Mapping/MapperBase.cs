using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Mapping;

/// <summary>
/// Provides a base implementation for mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public abstract class MapperBase<TSource, TDestination> : IMapper<TSource, TDestination>
    where TSource : class
    where TDestination : class, new()
{
    /// <summary>
    /// Maps the specified <paramref name="source"/> object to a new <typeparamref name="TDestination"/> instance.
    /// </summary>
    /// <param name="source">The source object to map from.</param>
    /// <returns>
    /// A new <typeparamref name="TDestination"/> instance with mapped values from <paramref name="source"/>,
    /// or <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(source))]
    public TDestination? Map(TSource? source)
    {
        if (source is null)
            return default;

        var destination = new TDestination();
        Map(source, destination);

        return destination;
    }

    /// <summary>
    /// Maps the values from the specified <paramref name="source"/> object into the provided <paramref name="destination"/> object.
    /// </summary>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The destination object to map into.</param>
    public abstract void Map(TSource source, TDestination destination);

    /// <summary>
    /// Projects a queryable source sequence to a queryable destination sequence using the mapping configuration.
    /// </summary>
    /// <param name="source">The source queryable to project from.</param>
    /// <returns>A queryable of <typeparamref name="TDestination"/> with mapped values.</returns>
    public abstract IQueryable<TDestination> ProjectTo(IQueryable<TSource> source);
}
