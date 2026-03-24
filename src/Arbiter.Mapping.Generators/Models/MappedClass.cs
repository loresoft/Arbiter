namespace Arbiter.Mapping.Generators.Models;

/// <summary>
/// Represents a source or destination type involved in a mapping.
/// </summary>
public record MappedClass
{
    /// <summary>
    /// Gets the fully qualified type name including the global prefix.
    /// </summary>
    public string FullyQualified { get; init; } = null!;

    /// <summary>
    /// Gets the namespace of the type.
    /// </summary>
    public string EntityNamespace { get; init; } = null!;

    /// <summary>
    /// Gets the short name of the type.
    /// </summary>
    public string EntityName { get; init; } = null!;
}
