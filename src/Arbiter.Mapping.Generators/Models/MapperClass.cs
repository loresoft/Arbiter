using Arbiter.Mapping.Generators.Infrastructure;

namespace Arbiter.Mapping.Generators.Models;

/// <summary>
/// Represents the source generator model for a mapper class, including the source and
/// destination types and the resolved property mappings.
/// </summary>
public record MapperClass
{
    /// <summary>
    /// Gets the fully qualified name of the mapper class including the global prefix.
    /// </summary>
    public string FullyQualified { get; init; } = null!;

    /// <summary>
    /// Gets the namespace of the mapper class.
    /// </summary>
    public string EntityNamespace { get; init; } = null!;

    /// <summary>
    /// Gets the short name of the mapper class.
    /// </summary>
    public string EntityName { get; init; } = null!;

    /// <summary>
    /// Gets the generated output file name.
    /// </summary>
    public string OutputFile { get; init; } = null!;


    /// <summary>
    /// Gets the source type to map from.
    /// </summary>
    public MappedClass SourceClass { get; init; } = null!;

    /// <summary>
    /// Gets the destination type to map to.
    /// </summary>
    public MappedClass DestinationClass { get; init; } = null!;

    /// <summary>
    /// Gets the destination constructor parameter names in declaration order,
    /// matched to destination property names. When non-empty, the writer emits
    /// constructor argument syntax instead of an object initializer for create
    /// and project mappers.
    /// </summary>
    public EquatableArray<string> ConstructorParameters { get; init; } = new();

    /// <summary>
    /// Gets the resolved property mappings between source and destination.
    /// </summary>
    public EquatableArray<PropertyMapping> Properties { get; init; }

    /// <summary>
    /// Gets the using directives collected from the source file(s) of the mapper class.
    /// These are forwarded to the generated file so that raw source expressions (e.g. those
    /// referencing constants from an imported namespace) continue to compile.
    /// </summary>
    public EquatableArray<string> Imports { get; init; } = new();
}
