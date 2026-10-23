using System.Net.Http.Headers;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Arbiter.Health.HealthChecks;

/// <summary>
/// Configures the <see cref="UrlHealthCheck"/> named <see cref="HttpClient"/> with a <c>User-Agent</c> header
/// that identifies the health probe.
/// </summary>
/// <remarks>
/// The header is added when the client is created by the factory, rather than for every probe. A <c>User-Agent</c>
/// already configured for the client by the application is preserved.
/// </remarks>
internal sealed class UrlHealthCheckOptions : IConfigureNamedOptions<HttpClientFactoryOptions>
{
    // created once; the header value is immutable so it is safe to share across clients
    private static readonly ProductInfoHeaderValue _userAgent = new("HealthCheck", ThisAssembly.FileVersion);

    /// <inheritdoc />
    public void Configure(HttpClientFactoryOptions options)
    {
    }

    /// <summary>
    /// Adds the <c>User-Agent</c> header when <paramref name="name"/> is the <see cref="UrlHealthCheck"/> client name.
    /// </summary>
    /// <param name="name">The name of the <see cref="HttpClient"/> being configured.</param>
    /// <param name="options">The options to configure.</param>
    public void Configure(string? name, HttpClientFactoryOptions options)
    {
        if (!string.Equals(name, nameof(UrlHealthCheck), StringComparison.Ordinal))
            return;

        options.HttpClientActions.Add(client =>
        {
            // respect a user agent configured by the application for this client
            if (client.DefaultRequestHeaders.UserAgent.Count == 0)
                client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
        });
    }
}
