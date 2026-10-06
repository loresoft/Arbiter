using System.Diagnostics.CodeAnalysis;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> indicating the implemented type has an identifier (Primary key)
/// </summary>
public interface IHaveIdentifier
{
    /// <summary>
    /// Gets the identifier for this instance.
    /// </summary>
    /// <returns>
    /// The identifier for this instance. Value type identifiers are boxed.
    /// </returns>
    object GetIdentifier();
}

/// <summary>
/// An <see langword="interface"/> indicating the implemented type has an identifier (Primary key)
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public interface IHaveIdentifier<TKey> : IHaveIdentifier
{
    /// <summary>
    /// Gets or sets the identifier for this instance.
    /// </summary>
    /// <value>
    /// The identifier for this instance.
    /// </value>
    [NotNull] TKey Id { get; set; }

    /// <inheritdoc />
    object IHaveIdentifier.GetIdentifier() => Id;
}
