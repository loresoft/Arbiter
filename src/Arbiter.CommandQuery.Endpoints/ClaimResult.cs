using Arbiter.CommandQuery.Models;

using MessagePack;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Represents claim-related endpoint output including headers and claims.
/// </summary>
[MessagePackObject(true)]
public sealed class ClaimResult
{
    /// <summary>
    /// Gets or sets the request headers.
    /// </summary>
    public IList<KeyValue<string, string>>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the request cookies.
    /// </summary>
    public IList<KeyValue<string, string>>? Cookies { get; set; }

    /// <summary>
    /// Gets or sets the resolved claims.
    /// </summary>
    public IList<ClaimType>? Claims { get; set; }

}
