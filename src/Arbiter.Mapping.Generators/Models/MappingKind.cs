namespace Arbiter.Mapping.Generators.Models;

/// <summary>
/// Describes how a destination property value is produced from the source value.
/// </summary>
public enum MappingKind
{
    /// <summary>
    /// The source value is assigned directly (reference copy for reference types).
    /// </summary>
    Direct,

    /// <summary>
    /// The source value is a nested object that is deep cloned via a generated helper method.
    /// </summary>
    Complex,

    /// <summary>
    /// The source value is a collection whose elements are deep cloned via a generated helper method.
    /// </summary>
    Collection,

    /// <summary>
    /// The source value is a dictionary whose values are deep cloned via a generated helper method.
    /// </summary>
    Dictionary,
}
