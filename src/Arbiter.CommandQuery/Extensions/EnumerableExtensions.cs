using Arbiter.CommandQuery.Models;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/> sequences.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts a sequence of values to a list of entity identifier models.
    /// </summary>
    /// <typeparam name="T">The type of the entity identifier value.</typeparam>
    /// <param name="values">The values to convert.</param>
    /// <returns>A list of entity identifier models containing the supplied values.</returns>
    public static IList<EntityIdentifierModel<T>> ToIdentifierList<T>(this IEnumerable<T> values)
        => [.. values.Select(v => new EntityIdentifierModel<T> { Id = v })];

}
