namespace Arbiter.Services;

/// <summary>
/// Defines an interface for providing a factory method to construct objects of type <typeparamref name="T"/>
/// from CSV-style row data.
/// </summary>
/// <typeparam name="T">The type of object to construct from row data.</typeparam>
/// <example>
/// The following example demonstrates how to implement <see cref="ISupportReader{T}"/> for a <c>Person</c> class:
/// <code>
/// public record Person(string Name, int Age, string Email)
///     : ISupportReader&lt;Person&gt;
/// {
///     public static Person RowFactory(IReadOnlyList&lt;string&gt; row, IReadOnlyList&lt;string&gt;? headers = null)
///         =&gt; new Person(
///             row[0],
///             int.Parse(row[1]),
///             row[2]
///         );
/// }
/// </code>
/// </example>
public interface ISupportReader<T>
{
    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> from a row of CSV data.
    /// </summary>
    /// <param name="row">
    /// A read-only list of field values representing a single CSV row.
    /// </param>
    /// <param name="headers">
    /// An optional read-only list of header column names. May be <see langword="null"/> if headers are not available.
    /// </param>
    /// <returns>
    /// An instance of <typeparamref name="T"/> constructed from the provided row data.
    /// </returns>
    static abstract T RowFactory(IReadOnlyList<string> row, IReadOnlyList<string>? headers = null);
}
