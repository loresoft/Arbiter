namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Defines an entity that has an alternate key that is globally unique across all entities.
/// </summary>
/// <remarks>
/// This interface provides a <see cref="Guid"/> based alternate key that can be used
/// for global identification of entities independent of the primary key. This is useful
/// for scenarios requiring a universally unique identifier for distributed systems,
/// client-side references, or cross-system integration.
/// </remarks>
public interface IHaveKey
{
    /// <summary>
    /// Gets or sets the globally unique alternate key for this entity.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that serves as a globally unique alternate key for this entity instance.
    /// </value>
    Guid Key { get; set; }
}
