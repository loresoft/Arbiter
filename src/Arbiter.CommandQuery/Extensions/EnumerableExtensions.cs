using Arbiter.CommandQuery.Models;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/> sequences.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts a sequence of values to a delimited string.
    /// </summary>
    /// <typeparam name="T">The type of objects to delimit.</typeparam>
    /// <param name="values">The values to convert.</param>
    /// <param name="delimiter">The delimiter to insert between values. Defaults to a comma when <see langword="null"/>.</param>
    /// <returns>A delimited string of the values.</returns>
    public static string ToDelimitedString<T>(this IEnumerable<T?> values, string? delimiter = ",")
        => string.Join(delimiter ?? ",", values);

    /// <summary>
    /// Converts a sequence of strings to a delimited string.
    /// </summary>
    /// <param name="values">The string values to convert.</param>
    /// <param name="delimiter">The delimiter to insert between values. Defaults to a comma when <see langword="null"/>.</param>
    /// <returns>A delimited string of the values.</returns>
    public static string ToDelimitedString(this IEnumerable<string?> values, string? delimiter = ",")
        => string.Join(delimiter ?? ",", values);

    /// <summary>
    /// Converts a sequence of values to a list of entity identifier models.
    /// </summary>
    /// <typeparam name="T">The type of the entity identifier value.</typeparam>
    /// <param name="values">The values to convert.</param>
    /// <returns>A list of entity identifier models containing the supplied values.</returns>
    public static IList<EntityIdentifierModel<T>> ToIdentifierList<T>(this IEnumerable<T> values)
        => [.. values.Select(v => new EntityIdentifierModel<T> { Id = v })];

}
