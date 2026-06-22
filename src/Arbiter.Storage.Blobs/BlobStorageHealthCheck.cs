using Azure.Storage.Blobs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Storage.Blobs;

/// <summary>
/// Performs a health check for an Azure Blob Storage container by validating container access and properties.
/// </summary>
public sealed partial class BlobStorageHealthCheck : IHealthCheck
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<BlobStorageHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageHealthCheck"/> class.
    /// </summary>
    /// <param name="blobContainerClient">The blob container client used for health checks.</param>
    /// <param name="logger">The logger used to write health check diagnostics.</param>
    public BlobStorageHealthCheck(
        BlobContainerClient blobContainerClient,
        ILogger<BlobStorageHealthCheck> logger)
    {
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var containerUrl = _blobContainerClient.Uri.ToString();

        try
        {
            await _blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var response = await _blobContainerClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var properties = response.Value;

            LogHealthCheck(_logger, containerUrl, properties.LastModified);

            return HealthCheckResult.Healthy($"Container '{containerUrl}' health check; Last Modified: {properties.LastModified} ");
        }
        catch (Exception ex)
        {
            LogHealthCheckError(_logger, ex, containerUrl, ex.Message);

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Container '{containerUrl}' health check failed: {ex.Message}",
                exception: ex);
        }
    }


    [LoggerMessage(Level = LogLevel.Information, Message = "Container '{ContainerName}' health check; Last Modified: {LastModified}")]
    private static partial void LogHealthCheck(ILogger logger, string containerName, DateTimeOffset lastModified);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error checking blob container health for '{ContainerName}': {ErrorMessage}")]
    private static partial void LogHealthCheckError(ILogger logger, Exception exception, string containerName, string errorMessage);
}
