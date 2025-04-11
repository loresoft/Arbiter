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
    ///   <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
    /// </value>
    bool IsDeleted { get; set; }
}
