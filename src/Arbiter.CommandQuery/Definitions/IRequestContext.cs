namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Defines contextual information about the current request, such as the originating
/// IP address and user agent.
/// </summary>
public interface IRequestContext
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
    void ApplyContext(string? ipAddress, string? userAgent);
}
