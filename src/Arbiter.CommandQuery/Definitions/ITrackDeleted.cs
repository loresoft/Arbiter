namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> indicating the implemented type supports soft delete
/// </summary>
public interface ITrackDeleted
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is deleted.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance is deleted; otherwise, <see langword="false"/>.
    /// </value>
    bool IsDeleted { get; set; }
}
