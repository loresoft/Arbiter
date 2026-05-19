namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Describes an additional claim to include in logging scopes and activity tags.
/// </summary>
public sealed record ClaimLoggingType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimLoggingType"/> class.
    /// </summary>
    /// <param name="claimType">The claim type to read from the current user.</param>
    /// <param name="scopeKey">The logging scope key to write.</param>
    /// <param name="activityTagName">The optional activity tag name to write.</param>
    public ClaimLoggingType(string claimType, string scopeKey, string? activityTagName = null)
        : this([claimType], scopeKey, activityTagName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimLoggingType"/> class.
    /// </summary>
    /// <param name="claimTypes">The ordered claim types to read from the current user.</param>
    /// <param name="scopeKey">The logging scope key to write.</param>
    /// <param name="activityTagName">The optional activity tag name to write.</param>
    public ClaimLoggingType(IEnumerable<string> claimTypes, string scopeKey, string? activityTagName = null)
    {
        ArgumentNullException.ThrowIfNull(claimTypes);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeKey);

        ClaimTypes = [.. claimTypes];
        if (ClaimTypes.Length == 0)
            throw new ArgumentException("At least one claim type is required.", nameof(claimTypes));

        ScopeKey = scopeKey;
        ActivityTagName = activityTagName;
    }

    /// <summary>
    /// Gets the ordered claim types to read from the current user.
    /// </summary>
    public string[] ClaimTypes { get; }

    /// <summary>
    /// Gets the logging scope key to write.
    /// </summary>
    public string ScopeKey { get; }

    /// <summary>
    /// Gets the optional activity tag name to write.
    /// </summary>
    public string? ActivityTagName { get; }
}
