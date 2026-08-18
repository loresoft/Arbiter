using Arbiter.CommandQuery.Definitions;

using Microsoft.AspNetCore.Http;

namespace Arbiter.Dispatcher.Server;

/// <summary>
/// Extension methods for populating an <see cref="IRequestContext"/> from an <see cref="HttpContext"/>.
/// </summary>
public static class RequestContextExtensions
{
    /// <summary>
    /// Applies the client IP address and user agent from the specified <paramref name="httpContext"/>
    /// to the <paramref name="requestContext"/>.
    /// </summary>
    /// <param name="requestContext">The request context to update.</param>
    /// <param name="httpContext">The HTTP context containing the connection and header information.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="requestContext"/> is <see langword="null"/>.
    /// </exception>
    public static void ApplyContext(this IRequestContext requestContext, HttpContext? httpContext)
    {
        ArgumentNullException.ThrowIfNull(requestContext);

        if (httpContext == null)
            return;

        var httpRequest = httpContext.Request;

        requestContext.ApplyContext(
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: httpRequest.Headers.UserAgent.ToString()
        );
    }
}
