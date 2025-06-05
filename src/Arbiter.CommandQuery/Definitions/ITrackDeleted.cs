namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Defines a contract for entities that support soft deletion. Implementing this interface allows an
/// entity to be marked as deleted without being physically removed from storage.
/// </summary>
public interface ITrackDeleted
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity instance is considered deleted (soft delete).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is marked as deleted; otherwise, <see langword="false"/>.
    /// </value>
    bool IsDeleted { get; set; }
}
