using MessagePack;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Represents a claim type and value pair.
/// </summary>
[MessagePackObject(true)]
public sealed class ClaimType
{
    /// <summary>
    /// Gets or sets the claim type identifier.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the claim value.
    /// </summary>
    public string Value { get; set; } = null!;
}
