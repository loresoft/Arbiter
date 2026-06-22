using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Functions;

/// <summary>
/// Performs a health check against the Azure Functions host status endpoint.
/// </summary>
public sealed partial class FunctionHealthCheck : IHealthCheck
{
    private readonly ILogger<FunctionHealthCheck> _logger;
    private readonly FunctionClient _functionClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger used to write health check diagnostics.</param>
    /// <param name="functionClient">The client used to query Functions host status.</param>
    public FunctionHealthCheck(
        ILogger<FunctionHealthCheck> logger,
        FunctionClient functionClient)
    {
        _logger = logger;
        _functionClient = functionClient;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {

        try
        {
            var result = await _functionClient.CheckHostStatus(cancellationToken).ConfigureAwait(false);

            LogHostStatus(_logger, result?.Id, result?.State);

            return HealthCheckResult.Healthy($"Function host '{result?.Id}' status: {result?.State}");
        }
        catch (Exception ex)
        {
            var functionUrl = _functionClient.HttpClient.BaseAddress?.Host ?? "unknown";

            LogHostHealthCheckFailed(_logger, ex, functionUrl, ex.Message);

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Function host health check failed for '{functionUrl}': {ex.Message}",
                exception: ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Function host '{host}' health check status: {status}")]
    private static partial void LogHostStatus(ILogger logger, string? host, string? status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Function host health check failed for '{functionHost}': {message}")]
    private static partial void LogHostHealthCheckFailed(ILogger logger, Exception exception, string functionHost, string message);
}
