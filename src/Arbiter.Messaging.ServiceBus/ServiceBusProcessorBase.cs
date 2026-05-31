using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides a base class for processing messages from an Azure Service Bus queue or topic subscription.
/// </summary>
/// <remarks>
/// Derived types implement <see cref="ProcessMessageAsync(ProcessMessageEventArgs)" /> to handle received messages.
/// The processor runs as an <see cref="IHostedService" />; it begins processing when the host starts and stops and
/// disposes the underlying <see cref="ServiceBusProcessor" /> when the host stops.
/// </remarks>
public abstract partial class ServiceBusProcessorBase : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger _logger;

    private bool _disposed;
    private bool _handlersAttached;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusProcessorBase" /> class.
    /// </summary>
    /// <param name="processor">The Service Bus processor used to receive messages.</param>
    /// <param name="logger">The logger used to write processing messages.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="processor" /> or <paramref name="logger" /> is <see langword="null" />.
    /// </exception>
    protected ServiceBusProcessorBase(ServiceBusProcessor processor, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(logger);

        _processor = processor;
        _logger = logger;
    }


    /// <summary>
    /// Gets the fully qualified namespace of the Service Bus instance the processor is connected to.
    /// </summary>
    protected string FullyQualifiedNamespace => _processor.FullyQualifiedNamespace;

    /// <summary>
    /// Gets the name of the entity (queue or topic) the processor receives messages from.
    /// </summary>
    protected string EntityPath => _processor.EntityPath;


    /// <summary>
    /// Starts processing messages from the configured entity.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the start operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor.ProcessMessageAsync += OnProcessMessageAsync;
        _processor.ProcessErrorAsync += OnProcessErrorAsync;
        _handlersAttached = true;

        LogStartingProcessor(_logger, _processor.EntityPath);

        await _processor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stops processing messages and detaches the event handlers.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the stop operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogStoppingProcessor(_logger, _processor.EntityPath);

        try
        {
            if (_processor.IsProcessing)
                await _processor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            DetachHandlers();
        }
    }


    /// <summary>
    /// Processes a message received from the configured entity.
    /// </summary>
    /// <param name="args">The event arguments containing the received message and message settlement operations.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    /// <remarks>
    /// Use the deserialization helpers in <see cref="ServiceBusMessageExtensions" /> to read the message body
    /// as a strongly-typed value.
    /// </remarks>
    protected abstract Task ProcessMessageAsync(ProcessMessageEventArgs args);

    /// <summary>
    /// Handles an error raised by the processor. The default implementation logs the error.
    /// </summary>
    /// <param name="args">The event arguments describing the error.</param>
    /// <returns>A task that represents the asynchronous error handling operation.</returns>
    protected virtual Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        LogProcessingError(_logger, args.Exception, args.EntityPath, args.ErrorSource);

        return Task.CompletedTask;
    }


    private Task OnProcessMessageAsync(ProcessMessageEventArgs args)
        => ProcessMessageAsync(args);

    private Task OnProcessErrorAsync(ProcessErrorEventArgs args)
        => ProcessErrorAsync(args);


    /// <summary>
    /// Releases the resources used by the processor.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases the resources used by the processor.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by the processor.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            DetachHandlers();

            // ServiceBusProcessor only exposes asynchronous disposal; block here so the synchronous
            // IDisposable contract is honored. Prefer DisposeAsync to avoid sync-over-async.
            _processor.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        _disposed = true;
    }

    /// <summary>
    /// Asynchronously releases the managed resources used by the processor.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        DetachHandlers();

        await _processor.DisposeAsync().ConfigureAwait(false);

        _disposed = true;
    }


    private void DetachHandlers()
    {
        if (!_handlersAttached)
            return;

        _processor.ProcessMessageAsync -= OnProcessMessageAsync;
        _processor.ProcessErrorAsync -= OnProcessErrorAsync;
        _handlersAttached = false;
    }


    [LoggerMessage(Level = LogLevel.Information, Message = "Starting Service Bus processor for entity '{EntityPath}'")]
    private static partial void LogStartingProcessor(ILogger logger, string entityPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Stopping Service Bus processor for entity '{EntityPath}'")]
    private static partial void LogStoppingProcessor(ILogger logger, string entityPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing Service Bus message for entity '{EntityPath}' from source '{ErrorSource}'")]
    private static partial void LogProcessingError(ILogger logger, Exception exception, string entityPath, ServiceBusErrorSource errorSource);
}
