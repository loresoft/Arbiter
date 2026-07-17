using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Health.HealthChecks;

/// <summary>
/// Represents a health check that verifies an HTTP endpoint by sending a request to a configured URL.
/// </summary>
public sealed class UrlHealthCheck : IHealthCheck
{
    private readonly string _url;
    private readonly ILogger<UrlHealthCheck> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpMethod _httpMethod;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record health check failures.</param>
    /// <param name="httpClientFactory">The factory used to create an <see cref="HttpClient"/> instance.</param>
    /// <param name="url">The URL to check for health status.</param>
    /// <param name="httpMethod">The HTTP method used for the health check request. Defaults to <see cref="HttpMethod.Head"/> when <see langword="null"/>.</param>
    public UrlHealthCheck(
        ILogger<UrlHealthCheck> logger,
        IHttpClientFactory httpClientFactory,
        string url,
        HttpMethod? httpMethod = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentException.ThrowIfNullOrEmpty(url);

        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _url = url;

        _httpMethod = httpMethod ?? HttpMethod.Head;
    }

    /// <summary>
    /// Checks the configured URL and returns the corresponding health status.
    /// </summary>
    /// <param name="context">The context associated with the health check execution.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the health check operation.</param>
    /// <returns>A <see cref="HealthCheckResult"/> that represents the health state of the configured URL.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            using var httpClient = _httpClientFactory.CreateClient(nameof(UrlHealthCheck));

            var request = new HttpRequestMessage(_httpMethod, _url);

            var response = await httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return HealthCheckResult.Healthy($"URL '{_url}' is healthy.");

            return HealthCheckResult.Unhealthy($"URL '{_url}' returned status code {response.StatusCode}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking URL health for '{Url}'", _url);

            var description = $"Error checking URL health for '{_url}': {ex.Message}";

            return new HealthCheckResult(
                status: context.Registration.FailureStatus,
                description: description,
                exception: ex);
        }
    }

}
