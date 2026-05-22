namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents a log filter message with optional name, key, and value criteria.
/// </summary>
/// <param name="Name">The optional filter name.</param>
/// <param name="Key">The optional filter key.</param>
/// <param name="Value">The optional filter value.</param>
public record FilterMessage(
    string? Name,
    string? Key,
    string? Value
);
