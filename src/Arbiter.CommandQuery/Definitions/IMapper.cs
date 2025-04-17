using System.Diagnostics.CodeAnalysis;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> defining the contract for a mapper.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Execute a mapping from the source object to a new destination object.
    /// </summary>
    /// <typeparam name="TSource">Source type to use</typeparam>
    /// <typeparam name="TDestination">Destination type to create</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <returns>Mapped destination object</returns>
    [return: NotNullIfNotNull(nameof(source))]
    TDestination? Map<TSource, TDestination>(TSource? source);

    /// <summary>
    /// Execute a mapping from the source object to the existing destination object.
    /// </summary>
    /// <typeparam name="TSource">Source type to use</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <param name="destination">Destination object to map into</param>
    void Map<TSource, TDestination>(TSource source, TDestination destination);



    /// <summary>
    /// Project from a source queryable to the destination queryable using the provided mapping
    /// </summary>
    /// <typeparam name="TSource">Source type to use</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source queryable to map from</param>
    /// <returns>Mapped destination queryable</returns>
    IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source);
}
