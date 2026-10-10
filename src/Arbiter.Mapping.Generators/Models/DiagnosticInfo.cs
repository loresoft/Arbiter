using Arbiter.Mapping.Generators.Infrastructure;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Arbiter.Mapping.Generators.Models;

/// <summary>
/// Equatable representation of a diagnostic produced by the source generator, safe to cache in the incremental pipeline.
/// </summary>
public record DiagnosticInfo
{
    /// <summary>
    /// Gets the diagnostic descriptor identifier.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Gets the file path of the diagnostic location.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the text span of the diagnostic location.
    /// </summary>
    public TextSpan TextSpan { get; init; }

    /// <summary>
    /// Gets the line span of the diagnostic location.
    /// </summary>
    public LinePositionSpan LineSpan { get; init; }

    /// <summary>
    /// Gets the message format arguments.
    /// </summary>
    public EquatableArray<string> Arguments { get; init; } = new();

    /// <summary>
    /// Creates a <see cref="Location"/> from the stored location information.
    /// </summary>
    /// <returns>The diagnostic location, or <see cref="Location.None"/> when no file path is available.</returns>
    public Location ToLocation()
    {
        if (string.IsNullOrEmpty(FilePath))
            return Location.None;

        return Location.Create(FilePath, TextSpan, LineSpan);
    }
}
