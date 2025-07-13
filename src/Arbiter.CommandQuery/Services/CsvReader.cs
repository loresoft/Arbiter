using System.Buffers;
using System.Text;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// Provides static methods to read and parse CSV data from various sources such as streams, strings, and text readers.
/// </summary>
/// <remarks>
/// <para>
/// This class supports both synchronous and asynchronous reading of CSV-formatted data into sequences of objects or arrays, with support for header rows,
/// custom field parsing, and standard CSV conventions (including quoted fields, escaped quotes, and configurable delimiters).
/// </para>
/// <para>
/// Overloads are provided to read from <see cref="Stream"/>, <see cref="string"/>, and <see cref="TextReader"/> sources,
/// and to return either parsed objects or raw field arrays. The default delimiter is a comma (<c>,</c>), and the default quote character is a double quote (<c>"</c>).
/// </para>
/// <para>
/// All methods handle cancellation tokens where applicable, and throw <see cref="ArgumentNullException"/> if required arguments are null.
/// </para>
/// </remarks>
/// <example>
/// The following example demonstrates how to use <c>Read</c> to read a list of people from CSV:
/// <code>
/// public record Person(string Name, int Age, string Email);
///
/// string csvContent = "Name,Age,Email\nAlice,30,alice@example.com\nBob,25,bob@example.com";
///
/// var people = CsvReader.Read(csvContent, (fields, headers) =>
///     new Person(
///         fields[0],
///         int.Parse(fields[1]),
///         fields[2]
///     )
/// );
/// </code>
/// </example>
public static class CsvReader
{
    /// <summary>
    /// Asynchronously reads CSV-formatted data from a <see cref="TextReader"/> and parses each row into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to create from the data rows.</typeparam>
    /// <param name="reader">The <see cref="TextReader"/> from which the CSV content will be read.</param>
    /// <param name="parser">
    /// A function that maps each row's fields and the header row to an object of type <typeparamref name="T"/>.
    /// The first argument is the current row's fields, and the second is the header fields (if present).
    /// </param>
    /// <param name="hasHeader">
    /// Indicates if the first row is a header row. If <c>true</c>, the first row is treated as headers and passed to the parser for each subsequent row.
    /// If <c>false</c>, all rows are treated as data and <paramref name="parser"/> receives <c>null</c> for the headers argument.
    /// Default is <c>true</c>.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is an <see cref="IReadOnlyList{T}"/> of parsed objects.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> or <paramref name="parser"/> is <c>null</c>.</exception>
    public static async Task<IReadOnlyList<T>> ReadAsync<T>(
        TextReader reader,
        Func<string[], string[]?, T> parser,
        bool hasHeader = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(parser);

        var content = await reader
            .ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);

        return Read(content.AsSpan(), parser, hasHeader);
    }

    /// <summary>
    /// Asynchronously reads CSV-formatted data from a <see cref="Stream"/> and parses each row into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to create from the data rows.</typeparam>
    /// <param name="stream">The input <see cref="Stream"/> to read the CSV content from.</param>
    /// <param name="parser">
    /// A function that maps each row's fields and the header row to an object of type <typeparamref name="T"/>.
    /// The first argument is the current row's fields, and the second is the header fields (if present).
    /// </param>
    /// <param name="hasHeader">
    /// Indicates if the first row is a header row. If <c>true</c>, the first row is treated as headers and passed to the parser for each subsequent row.
    /// If <c>false</c>, all rows are treated as data and <paramref name="parser"/> receives <c>null</c> for the headers argument.
    /// Default is <c>true</c>.
    /// </param>
    /// <param name="encoding">The text encoding to use. Defaults to UTF-8 if <c>null</c>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is an <see cref="IReadOnlyList{T}"/> of parsed objects.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stream"/> or <paramref name="parser"/> is <c>null</c>.</exception>
    public static async Task<IReadOnlyList<T>> ReadAsync<T>(
        Stream stream,
        Func<string[], string[]?, T> parser,
        bool hasHeader = true,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(parser);

        encoding ??= Encoding.UTF8;
        using var reader = new StreamReader(stream, encoding, bufferSize: 4096, leaveOpen: true);

        return await ReadAsync(reader, parser, hasHeader, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads CSV-formatted data from a <see cref="TextReader"/> and returns each row as a string array.
    /// The first row is typically the header row.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> from which the CSV content will be read.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is an <see cref="IReadOnlyList{T}"/> of string arrays, each representing a row in the CSV data.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <c>null</c>.</exception>
    public static async Task<IReadOnlyList<string[]>> ReadAsync(
        TextReader reader,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var content = await reader
            .ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);

        return Read(content.AsSpan());
    }

    /// <summary>
    /// Asynchronously reads CSV-formatted data from a <see cref="Stream"/> and returns each row as a string array.
    /// The first row is typically the header row.
    /// </summary>
    /// <param name="stream">The input <see cref="Stream"/> to read the CSV content from.</param>
    /// <param name="encoding">The text encoding to use. Defaults to UTF-8 if <c>null</c>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is an <see cref="IReadOnlyList{T}"/> of string arrays, each representing a row in the CSV data.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stream"/> is <c>null</c>.</exception>
    public static async Task<IReadOnlyList<string[]>> ReadAsync(
        Stream stream,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        encoding ??= Encoding.UTF8;
        using var reader = new StreamReader(stream, encoding, bufferSize: 4096, leaveOpen: true);

        return await ReadAsync(reader, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads CSV-formatted data from a character buffer and parses each row into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to create from the data rows.</typeparam>
    /// <param name="csvContent">The buffer containing CSV-formatted characters.</param>
    /// <param name="parser">
    /// A function that maps each row's fields and the header row to an object of type <typeparamref name="T"/>.
    /// The first argument is the current row's fields, and the second is the header fields (if present).
    /// </param>
    /// <param name="hasHeader">
    /// Indicates if the first row is a header row. If <c>true</c>, the first row is treated as headers and passed to the parser for each subsequent row.
    /// If <c>false</c>, all rows are treated as data and <paramref name="parser"/> receives <c>null</c> for the headers argument.
    /// Default is <c>true</c>.
    /// </param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of parsed objects.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parser"/> is <c>null</c>.</exception>
    public static IReadOnlyList<T> Read<T>(
        ReadOnlySpan<char> csvContent,
        Func<string[], string[]?, T> parser,
        bool hasHeader = true)
    {
        ArgumentNullException.ThrowIfNull(parser);

        var results = new List<T>();
        string[]? headers = null;
        bool isFirstRow = true;

        ReadCore(csvContent, ',', '"', row =>
        {
            if (isFirstRow && hasHeader)
            {
                headers = row;
                isFirstRow = false;
                return;
            }
            if (isFirstRow)
            {
                isFirstRow = false;
            }
            var item = parser(row, hasHeader ? headers : null);
            results.Add(item);
        });

        return results;
    }

    /// <summary>
    /// Reads CSV-formatted data from a character buffer and returns each row as a string array.
    /// The first row is typically the header row.
    /// </summary>
    /// <param name="csvContent">The buffer containing CSV-formatted characters.</param>
    /// <param name="delimiter">The delimiter character to use (default is comma).</param>
    /// <param name="quote">The quote character to use for quoted fields (default is double quote).</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of string arrays, each representing a row in the CSV data.
    /// </returns>
    public static IReadOnlyList<string[]> Read(
        ReadOnlySpan<char> csvContent,
        char delimiter = ',',
        char quote = '"')
    {
        var rows = new List<string[]>();

        ReadCore(csvContent, delimiter, quote, rows.Add);

        return rows;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "String parsing logic")]
    private static void ReadCore(ReadOnlySpan<char> span, char delimiter, char quote, Action<string[]> processRow)
    {
        var currentRow = new List<string>();

        int fieldStart = 0;
        int position = 0;
        bool inQuotes = false;

        while (position < span.Length)
        {
            char c = span[position];

            if (c == quote)
            {
                if (inQuotes)
                {
                    // Check if next character is also a quote (escaped quote)
                    if (position + 1 < span.Length && span[position + 1] == quote)
                        position++; // Skip the escaped quote
                    else
                        inQuotes = false;
                }
                else
                {
                    inQuotes = true;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                // End of field
                var field = ReadField(span[fieldStart..position], quote);
                currentRow.Add(field);
                fieldStart = position + 1;
            }
            else if ((c == '\r' || c == '\n') && !inQuotes)
            {
                // End of row
                if (position > fieldStart || currentRow.Count > 0)
                {
                    var field = ReadField(span[fieldStart..position], quote);
                    currentRow.Add(field);
                    processRow([.. currentRow]);
                    currentRow.Clear();
                }

                // Skip \r\n combination
                if (c == '\r' && position + 1 < span.Length && span[position + 1] == '\n')
                    position++;

                fieldStart = position + 1;
            }

            position++;
        }

        // Add the last field and row if there's content
        if (position > fieldStart || currentRow.Count > 0)
        {
            var field = ReadField(span[fieldStart..position], quote);
            currentRow.Add(field);
            processRow([.. currentRow]);
        }
    }

    private static string ReadField(ReadOnlySpan<char> fieldSpan, char quote)
    {
        if (fieldSpan.Length == 0)
            return string.Empty;

        // Check if field is quoted
        if (fieldSpan.Length < 2 || fieldSpan[0] != quote || fieldSpan[^1] != quote)
            return fieldSpan.ToString();

        // Remove outer quotes and process escaped quotes
        var innerSpan = fieldSpan[1..^1];
        return ReadEscaped(innerSpan, quote);
    }

    private static string ReadEscaped(ReadOnlySpan<char> fieldSpan, char quote)
    {
        if (fieldSpan.Length == 0)
            return string.Empty;

        // Check if we have any escaped quotes
        if (fieldSpan.IndexOf(quote) == -1)
            return fieldSpan.ToString();

        char[]? rented = null;

        try
        {
            const int StackAllocThreshold = 256;

            // Use a rented buffer or stack allocation for performance
            Span<char> buffer = fieldSpan.Length <= StackAllocThreshold
                ? stackalloc char[fieldSpan.Length]
                : (rented = ArrayPool<char>.Shared.Rent(fieldSpan.Length));

            int writeIndex = 0;

            for (int i = 0; i < fieldSpan.Length; i++)
            {
                if (fieldSpan[i] == quote && i + 1 < fieldSpan.Length && fieldSpan[i + 1] == quote)
                {
                    // Escaped quote - add single quote and skip next
                    buffer[writeIndex++] = quote;
                    i++; // Skip the next quote
                }
                else
                {
                    buffer[writeIndex++] = fieldSpan[i];
                }
            }

            return buffer[..writeIndex].ToString();
        }
        finally
        {
            // Return rented buffer to the pool if it was used
            if (rented != null)
                ArrayPool<char>.Shared.Return(rented);
        }
    }
}
