using System.Diagnostics.CodeAnalysis;

namespace Tracker.Extensions;

/// <summary>
/// String extension methods
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Truncates the specified text.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="keep">The number of characters to keep.</param>
    /// <param name="ellipsis">The ellipsis string to use when truncating. (Default ...)</param>
    /// <returns>
    /// A truncate string.
    /// </returns>
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Truncate(this string? text, int keep, string ellipsis = "...")
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (text!.Length <= keep)
            return text;

        ellipsis ??= string.Empty;

        if (text.Length <= keep + ellipsis.Length || keep < ellipsis.Length)
            return text[..keep];

        int prefix = keep - ellipsis.Length;
        return string.Concat(text[..prefix], ellipsis);
    }

    /// <summary>
    /// Indicates whether the specified String object is null or an empty string
    /// </summary>
    /// <param name="item">A String reference</param>
    /// <returns>
    ///     <see langword="true"/> if is <see langword="null"/> or empty; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? item)
    {
        return string.IsNullOrEmpty(item);
    }

    /// <summary>
    /// Indicates whether a specified string is null, empty, or consists only of white-space characters
    /// </summary>
    /// <param name="item">A String reference</param>
    /// <returns>
    ///      <see langword="true"/> if is <see langword="null"/> or empty; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? item)
    {
        if (item == null)
            return true;

        for (int i = 0; i < item.Length; i++)
            if (!char.IsWhiteSpace(item[i]))
                return false;

        return true;
    }

    /// <summary>
    /// Determines whether the specified string is not <see cref="IsNullOrWhiteSpace"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    ///   <see langword="true"/> if the specified <paramref name="value"/> is not <see cref="IsNullOrWhiteSpace"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasValue([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Combines two strings with the specified separator.
    /// </summary>
    /// <param name="first">The first string.</param>
    /// <param name="second">The second string.</param>
    /// <param name="separator">The separator string.</param>
    /// <returns>A string combining the <paramref name="first"/> and <paramref name="second"/> parameters with the <paramref name="separator"/> between them</returns>
    [return: NotNullIfNotNull(nameof(first))]
    [return: NotNullIfNotNull(nameof(second))]
    public static string? Combine(this string? first, string? second, char separator = '/')
    {
        if (string.IsNullOrEmpty(first))
            return second;

        if (string.IsNullOrEmpty(second))
            return first;

        bool firstEndsWith = first[^1] == separator;
        bool secondStartsWith = second[0] == separator;

        if (firstEndsWith && secondStartsWith)
            return string.Concat(first, second[1..]);

        return firstEndsWith || secondStartsWith
            ? string.Concat(first, second)
            : $"{first}{separator}{second}";
    }

}
