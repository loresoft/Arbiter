using System.Diagnostics.CodeAnalysis;

namespace Arbiter.Mapping;

/// <summary>
/// Defines the contract for a type-agnostic mapper that maps between arbitrary source and destination types.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Execute a mapping from the source object to a new destination object.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to create.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <returns>A new instance of <typeparamref name="TDestination"/> mapped from <paramref name="source"/>.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    TDestination? Map<TSource, TDestination>(TSource? source);

    /// <summary>
    /// Execute a mapping from the source object to an existing destination object.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to map into.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination object to map into.</param>
    void Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// Project from a source queryable to the destination queryable using the provided mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to project to.</typeparam>
    /// <param name="source">The source queryable to project from.</param>
    /// <returns>A queryable of <typeparamref name="TDestination"/> projected from <paramref name="source"/>.</returns>
    IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source);
}

/// <summary>
/// Defines the mapping contract from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public interface IMapper<in TSource, TDestination>
{
    /// <summary>
    /// Execute a mapping from the source object to a new destination object.
    /// </summary>
    /// <param name="source">The source object to map from.</param>
    /// <returns>A new instance of <typeparamref name="TDestination"/> mapped from <paramref name="source"/>.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    TDestination? Map(TSource? source);

    /// <summary>
    /// Execute a mapping from the source object to an existing destination object.
    /// </summary>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination object to map into.</param>
    void Map(TSource source, TDestination destination);

    /// <summary>
    /// Project from a source queryable to the destination queryable using the provided mapping.
    /// </summary>
    /// <param name="source">The source queryable to project from.</param>
    /// <returns>A queryable of <typeparamref name="TDestination"/> projected from <paramref name="source"/>.</returns>
    IQueryable<TDestination> ProjectTo(IQueryable<TSource> source);
}
