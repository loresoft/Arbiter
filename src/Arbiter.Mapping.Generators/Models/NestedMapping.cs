using Arbiter.Mapping.Generators.Infrastructure;

namespace Arbiter.Mapping.Generators.Models;

/// <summary>
/// Represents a generated private helper method that deep clones a nested object, collection, or dictionary.
/// </summary>
public record NestedMapping
{
    /// <summary>
    /// Gets the generated helper method name.
    /// </summary>
    public string MethodName { get; init; } = null!;

    /// <summary>
    /// Gets the kind of helper (<see cref="MappingKind.Complex"/>, <see cref="MappingKind.Collection"/>,
    /// or <see cref="MappingKind.Dictionary"/>).
    /// </summary>
    public MappingKind Kind { get; init; }

    /// <summary>
    /// Gets the fully qualified helper parameter type.
    /// </summary>
    public string SourceType { get; init; } = null!;

    /// <summary>
    /// Gets the fully qualified helper return type.
    /// </summary>
    public string DestinationType { get; init; } = null!;

    /// <summary>
    /// Gets the destination constructor parameter names for <see cref="MappingKind.Complex"/> helpers.
    /// </summary>
    public EquatableArray<string> ConstructorParameters { get; init; } = new();

    /// <summary>
    /// Gets the property mappings for <see cref="MappingKind.Complex"/> helpers.
    /// </summary>
    public EquatableArray<PropertyMapping> Properties { get; init; } = new();

    /// <summary>
    /// Gets the collection type created by <see cref="MappingKind.Collection"/> and <see cref="MappingKind.Dictionary"/> helpers.
    /// </summary>
    public CollectionKind CollectionKind { get; init; }

    /// <summary>
    /// Gets the name of the <see cref="MappingKind.Complex"/> helper used to clone each element (or dictionary value).
    /// </summary>
    public string ElementMethodName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the fully qualified source element (or dictionary value) type.
    /// </summary>
    public string ElementSourceType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the fully qualified destination element (or dictionary value) type.
    /// </summary>
    public string ElementDestinationType { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the destination element (or dictionary value) type allows null.
    /// </summary>
    public bool IsElementNullable { get; init; }

    /// <summary>
    /// Gets the fully qualified dictionary key type.
    /// </summary>
    public string KeyType { get; init; } = string.Empty;
}
