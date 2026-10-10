namespace Arbiter.Mapping.Generators.Models;

/// <summary>
/// Describes the concrete collection type created when deep cloning a collection.
/// </summary>
public enum CollectionKind
{
    /// <summary>
    /// Not a collection.
    /// </summary>
    None,

    /// <summary>
    /// Creates a <c>List&lt;T&gt;</c>; used for <c>List&lt;T&gt;</c>, <c>IList&lt;T&gt;</c>, <c>ICollection&lt;T&gt;</c>,
    /// <c>IEnumerable&lt;T&gt;</c>, <c>IReadOnlyList&lt;T&gt;</c> and <c>IReadOnlyCollection&lt;T&gt;</c> destinations.
    /// </summary>
    List,

    /// <summary>
    /// Creates a <c>T[]</c>.
    /// </summary>
    Array,

    /// <summary>
    /// Creates a <c>HashSet&lt;T&gt;</c>; used for <c>HashSet&lt;T&gt;</c> and <c>ISet&lt;T&gt;</c> destinations.
    /// </summary>
    HashSet,

    /// <summary>
    /// Creates a <c>Dictionary&lt;TKey, TValue&gt;</c>; used for <c>Dictionary&lt;,&gt;</c>, <c>IDictionary&lt;,&gt;</c>
    /// and <c>IReadOnlyDictionary&lt;,&gt;</c> destinations.
    /// </summary>
    Dictionary,
}
