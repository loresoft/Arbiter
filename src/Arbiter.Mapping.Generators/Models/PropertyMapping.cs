using Arbiter.Mapping.Generators.Infrastructure;

namespace Arbiter.Mapping.Generators.Models;

/// <summary>
/// Represents a mapping between a source property path and a destination property.
/// </summary>
public record PropertyMapping : IEquatable<PropertyMapping>
{
    /// <summary>
    /// The name of the destination property.
    /// </summary>
    public string DestinationName { get; init; } = null!;

    /// <summary>
    /// The source property path segments (e.g., ["Id"] or ["Person", "FirstName"]).
    /// </summary>
    public EquatableArray<string> SourcePath { get; init; }

    /// <summary>
    /// Per-segment nullability for the source path. Each element corresponds to a segment
    /// in <see cref="SourcePath"/> and indicates whether that navigation property is nullable.
    /// The final segment (the leaf property) is always false since it is read, not navigated.
    /// </summary>
    public EquatableArray<bool> SourceSegmentNullable { get; init; }

    /// <summary>
    /// The raw source expression text from the lambda body (e.g., "src.Locations.Count(l => l.IsActive)").
    /// When non-empty, this expression is emitted directly with parameter name substitution,
    /// and <see cref="SourcePath"/> and <see cref="SourceSegmentNullable"/> are ignored.
    /// </summary>
    public string SourceExpression { get; init; } = string.Empty;

    /// <summary>
    /// The lambda parameter name used in <see cref="SourceExpression"/> (e.g., "src").
    /// Used to substitute the correct parameter name in the generated code.
    /// </summary>
    public string SourceExpressionParameter { get; init; } = string.Empty;

    /// <summary>
    /// Whether the destination property allows null values.
    /// </summary>
    public bool IsDestinationNullable { get; init; }

    /// <summary>
    /// Whether the destination property type is a string.
    /// </summary>
    public bool IsDestinationString { get; init; }

    /// <summary>
    /// Whether this property should be ignored in the mapping.
    /// </summary>
    public bool IsIgnored { get; init; }

    /// <summary>
    /// Whether the destination property is read-only (no setter or init-only).
    /// Read-only properties can be assigned via constructor or object initializer but not via direct assignment.
    /// </summary>
    public bool IsReadOnly { get; init; }
}
