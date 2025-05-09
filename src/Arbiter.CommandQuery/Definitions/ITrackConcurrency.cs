namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> indicating the implemented type supports tracking concurrency
/// </summary>
public interface ITrackConcurrency
{
    /// <summary>
    /// Gets or sets the row version concurrency value.
    /// </summary>
    /// <value>
    /// The row version concurrency value.
    /// </value>
    long RowVersion { get; set; }
}
