using Microsoft.AspNetCore.Components;

namespace Arbiter.Components.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to a <see cref="MarkupString"/> so it is rendered as markup rather than encoded text.
    /// </summary>
    /// <param name="value">The markup to render</param>
    /// <returns>
    /// A <see cref="MarkupString"/> for <paramref name="value"/>, or an empty markup string when
    /// <paramref name="value"/> is <see langword="null"/>
    /// </returns>
    /// <remarks>
    /// The content is rendered without encoding, so only pass values from a trusted source to avoid script
    /// injection.
    /// </remarks>
    public static MarkupString ToMarkupString(this string? value)
    {
        return new MarkupString(value ?? string.Empty);
    }
}
