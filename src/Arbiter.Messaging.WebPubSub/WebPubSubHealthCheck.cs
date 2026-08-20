using Azure;
using Azure.Messaging.WebPubSub;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Performs a health check for an Azure Web PubSub hub using a non-mutating group existence request.
/// </summary>
public sealed partial class WebPubSubHealthCheck : IHealthCheck
{
    private const string HealthCheckGroup = "__arbiter_health_check__";

    private readonly ILogger<WebPubSubHealthCheck> _logger;
    private readonly WebPubSubServiceClient _serviceClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPubSubHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger used to write health check diagnostics.</param>
    /// <param name="serviceClient">The Web PubSub service client for the hub being checked.</param>
    public WebPubSubHealthCheck(
        ILogger<WebPubSubHealthCheck> logger,
        WebPubSubServiceClient serviceClient)
    {
        _logger = logger;
        _serviceClient = serviceClient;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var endpoint = _serviceClient.Endpoint;
        var hubName = _serviceClient.Hub;

        try
        {
            await _serviceClient
                .GroupExistsAsync(
                    HealthCheckGroup,
                    new RequestContext { CancellationToken = cancellationToken })
                .ConfigureAwait(false);

            LogHubHealth(_logger, endpoint.Host, hubName);

            return HealthCheckResult.Healthy(
                $"Web PubSub '{endpoint.Host}' Hub '{hubName}' is reachable");
        }
        catch (Exception ex)
        {
            LogHubError(_logger, ex, endpoint.Host, hubName, ex.Message);

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Web PubSub '{endpoint.Host}' Hub '{hubName}' health check failed: {ex.Message}",
                exception: ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Web PubSub '{WebPubSubEndpoint}' Hub '{HubName}' health check succeeded")]
    private static partial void LogHubHealth(
        ILogger logger,
        string webPubSubEndpoint,
        string hubName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error checking Web PubSub health for endpoint '{WebPubSubEndpoint}', hub '{HubName}': {ErrorMessage}")]
    private static partial void LogHubError(
        ILogger logger,
        Exception exception,
        string webPubSubEndpoint,
        string hubName,
        string errorMessage);
}
