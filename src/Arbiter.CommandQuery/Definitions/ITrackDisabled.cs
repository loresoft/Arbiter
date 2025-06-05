namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Represents an entity that can be marked as disabled.
/// </summary>
/// <remarks>
/// This interface is typically used to indicate whether an object is in a disabled state. Implementing
/// types can use the <see cref="IsDisabled"/> property to manage or query the disabled status.
/// </remarks>
public interface ITrackDisabled
{
    /// <summary>
    /// Gets or sets a value indicating whether an entity is disabled.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance is disabled; otherwise, <see langword="false"/>.
    /// </value>
    bool IsDisabled { get; set; }
}
