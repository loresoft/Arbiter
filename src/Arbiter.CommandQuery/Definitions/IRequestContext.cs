namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Defines contextual information about the current request, such as the originating
/// IP address and user agent.
/// </summary>
/// <remarks>
/// Implementations are typically populated once per request, before the request is dispatched,
/// by calling <see cref="ApplyContext(string?, string?)"/>. Values are informational and may be
/// absent when the transport does not provide them.
/// </remarks>
/// <seealso cref="IRequestPrincipal"/>
public interface IRequestContext : IRequestPrincipal
{
    /// <summary>
    /// Gets the IP address the request originated from.
    /// </summary>
    /// <value>The client IP address, or <see langword="null"/> when it is not available.</value>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the user agent reported by the client that issued the request.
    /// </summary>
    /// <value>The client user agent, or <see langword="null"/> when it is not available.</value>
    string? UserAgent { get; }

    /// <summary>
    /// Applies the specified request information to this context.
    /// </summary>
    /// <param name="ipAddress">The IP address the request originated from; may be <see langword="null"/>.</param>
    /// <param name="userAgent">The user agent reported by the client; may be <see langword="null"/>.</param>
    /// <remarks>
    /// After this method returns, <see cref="IpAddress"/> and <see cref="UserAgent"/> reflect the supplied values.
    /// </remarks>
    void ApplyContext(string? ipAddress, string? userAgent);
}
