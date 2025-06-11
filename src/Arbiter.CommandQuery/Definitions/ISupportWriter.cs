namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Defines an interface for providing CSV-style header and row data extraction
/// from a specified type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of object to extract headers and row values from.</typeparam>
/// <example>
/// The following example demonstrates how to implement <see cref="ISupportWriter{T}"/> for a <c>Person</c> class:
/// <code>
/// public record Person(string Name, int Age, string Email)
///     : ISupportWriter&lt;Person&gt;
/// {
///     public static IEnumerable&lt;string&gt;? Headers()
///         =&gt; [nameof(Name), nameof(Age), nameof(Email)];
///
///     public static Func&lt;Person, IEnumerable&lt;string?&gt;&gt; RowSelector()
///         =&gt; static p =&gt; [p.Name, p.Age.ToString(), p.Email];
/// }
/// </code>
/// </example>
public interface ISupportWriter<T>
{
    /// <summary>
    /// Returns the column headers for the CSV output.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{String}"/> representing the headers,
    /// or <c>null</c> if headers are not applicable.
    /// </returns>
    static abstract IEnumerable<string>? Headers();

    /// <summary>
    /// Returns a delegate that extracts row values from an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <returns>
    /// A function that takes a <typeparamref name="T"/> instance and returns
    /// an <see cref="IEnumerable{String}"/> representing a CSV row.
    /// </returns>
    static abstract Func<T, IEnumerable<string?>> RowSelector();
}
