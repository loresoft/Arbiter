using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Storage.Blobs;

/// <summary>
/// Initializes configured Azure Blob Storage containers when the host starts.
/// </summary>
public sealed partial class BlobStorageInitializer : IHostedService
{
    private readonly ILogger<BlobStorageInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageInitializer" /> class.
    /// </summary>
    /// <param name="logger">The logger used to write initialization messages.</param>
    /// <param name="serviceProvider">The service provider used to resolve configured options and dependencies.</param>
    public BlobStorageInitializer(ILogger<BlobStorageInitializer> logger, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Starts Blob Storage container initialization.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel initialization.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var registrations = _serviceProvider.GetServices<BlobStorageRegistration>();
        if (registrations?.Any() != true)
        {
            LogNoContainersConfigured(_logger);
            return;
        }

        var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<BlobStorageOptions>>();

        foreach (var registration in registrations)
        {
            var option = optionsMonitor.Get(registration.OptionsName);

            try
            {
                await Initialize(option, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogErrorInitializingContainers(_logger, ex, option.NameOrConnectionString);
            }
        }
    }

    /// <summary>
    /// Stops the hosted service.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the stop operation.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    private async Task Initialize(
        BlobStorageOptions option,
        CancellationToken cancellationToken)
    {
        foreach (var container in option.Containers)
        {
            var containerClient = _serviceProvider.GetRequiredKeyedService<BlobContainerClient>(container.Key);
            await InitializeContainer(containerClient, container.Value, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task InitializeContainer(
        BlobContainerClient containerClient,
        PublicAccessType publicAccessType,
        CancellationToken cancellationToken)
    {
        var containerName = containerClient.Name;

        LogCreatingContainer(_logger, containerName);

        try
        {
            var response = await containerClient
                .CreateIfNotExistsAsync(publicAccessType: publicAccessType, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (response is null)
                LogContainerAlreadyExists(_logger, containerName);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            LogContainerAlreadyExists(_logger, ex, containerName);
        }
    }


    [LoggerMessage(Level = LogLevel.Debug, Message = "No Azure Blob Storage containers configured for initialization.")]
    private static partial void LogNoContainersConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error initializing Azure Blob Storage containers for '{NameOrConnectionString}'")]
    private static partial void LogErrorInitializingContainers(ILogger logger, Exception exception, string nameOrConnectionString);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating Blob Container '{ContainerName}'")]
    private static partial void LogCreatingContainer(ILogger logger, string containerName);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Blob Container '{ContainerName}' already exists")]
    private static partial void LogContainerAlreadyExists(ILogger logger, string containerName);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Blob Container '{ContainerName}' already exists")]
    private static partial void LogContainerAlreadyExists(ILogger logger, Exception exception, string containerName);
}
