using System.Diagnostics;

using Azure.Messaging.WebPubSub.Clients;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides a base class for listening to messages published to the Azure Web PubSub groups declared for a hub by
/// <see cref="WebPubSubExtensions.AddWebPubSub(Microsoft.Extensions.DependencyInjection.IServiceCollection, object, string, System.Action{WebPubSubBuilder}, System.Action{WebPubSubOptionsBuilder})" />.
/// </summary>
/// <remarks>
/// Derive from this class to process group and server messages by implementing
/// <see cref="ProcessGroupMessageAsync(WebPubSubGroupMessageEventArgs)" /> and
/// <see cref="ProcessServerMessageAsync(WebPubSubServerMessageEventArgs)" />.
/// <para>
/// The processor is managed as an <see cref="IHostedService" />. It defers the connection until
/// <see cref="IHostApplicationLifetime.ApplicationStarted" /> so host startup is not blocked, and it detaches
/// handlers and shuts down the underlying <see cref="WebPubSubClient" /> during host stop/disposal.
/// </para>
/// <para>
/// Azure Web PubSub does not persist messages for disconnected clients. Messages sent while this processor is
/// disconnected are not replayed and must be tolerated by the application.
/// </para>
/// </remarks>
public abstract partial class WebPubSubProcessorBase : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly WebPubSubHubContext _context;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly WebPubSubClient _client;
    private readonly CancellationTokenSource _stoppingTokenSource = new();

    private CancellationTokenRegistration _startedRegistration;
    private Task _connectTask = Task.CompletedTask;
    private bool _disposed;
    private bool _handlersAttached;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPubSubProcessorBase" /> class.
    /// </summary>
    /// <param name="context">The resolved hub, group, and client configuration for the listener.</param>
    /// <param name="lifetime">The host lifetime used to defer connecting until the application has started.</param>
    /// <param name="logger">The logger used to write processing messages.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context" />, <paramref name="lifetime" /> or <paramref name="logger" /> is <see langword="null" />.
    /// </exception>
    protected WebPubSubProcessorBase(WebPubSubHubContext context, IHostApplicationLifetime lifetime, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(lifetime);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _lifetime = lifetime;
        Logger = logger;

        _client = CreateClient(context);
    }


    /// <summary>
    /// Gets the name of the hub the processor receives messages from.
    /// </summary>
    protected string HubName => _context.HubName;

    /// <summary>
    /// Gets the groups declared for the hub that the processor joins when the connection is established.
    /// </summary>
    protected IReadOnlyDictionary<string, string> Groups => _context.Groups;

    /// <summary>
    /// Gets the logger used to write processing messages.
    /// </summary>
    protected ILogger Logger { get; }


    /// <summary>
    /// Schedules the connection to the configured hub to run once the application has started.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the start operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    /// <remarks>
    /// The connection is established from the <see cref="IHostApplicationLifetime.ApplicationStarted" /> callback so the
    /// host, and any web server, finishes starting before the listener connects.
    /// </remarks>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // schedule the connection to run after the host has started, so startup is not blocked by the connection
        _startedRegistration = _lifetime.ApplicationStarted.Register(() => _connectTask = ConnectAsync(_stoppingTokenSource.Token));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disconnects from the configured hub and detaches the event handlers.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the stop operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <remarks>
    /// The event handlers are always detached, even when the underlying client fails to stop.
    /// </remarks>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogStoppingProcessor(Logger, _context.HubName);

        // cancel the connection if it is still pending
        await _startedRegistration
            .DisposeAsync()
            .ConfigureAwait(false);

        // cancel the stopping token so the connection can be aborted if it is still pending
        await _stoppingTokenSource
            .CancelAsync()
            .ConfigureAwait(false);

        try
        {
            // wait for a pending connection so the client is not stopped while it is still starting
            await _connectTask
                .ConfigureAwait(false);

            await _client
                .StopAsync()
                .ConfigureAwait(false);
        }
        finally
        {
            DetachHandlers();
        }
    }


    /// <summary>
    /// Processes a message received from the configured hub.
    /// </summary>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    /// <remarks>
    /// Use the deserialization helpers in <see cref="WebPubSubMessageExtensions" /> to read the message data
    /// as a strongly-typed value. Web PubSub has no dead-letter queue; malformed messages must be logged and dropped.
    /// </remarks>
    protected abstract Task ProcessGroupMessageAsync(WebPubSubGroupMessageEventArgs args);

    /// <summary>
    /// Handles a message received from the Web PubSub server. The default implementation does nothing.
    /// </summary>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    protected abstract Task ProcessServerMessageAsync(WebPubSubServerMessageEventArgs args);


    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _client.GroupMessageReceived += OnGroupMessageReceivedAsync;
        _client.ServerMessageReceived += OnServerMessageReceivedAsync;
        _client.Connected += OnConnectedAsync;
        _client.Disconnected += OnDisconnectedAsync;

        _handlersAttached = true;

        LogStartingProcessor(Logger, _context.HubName);

        try
        {
            await _client
                .StartAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var group in _context.Groups.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                LogJoiningGroup(Logger, group, _context.HubName);

                await _client
                    .JoinGroupAsync(group, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // the host is shutting down before the connection completed
        }
        catch (Exception ex)
        {
            // the connection runs detached from host startup; log so the failure is not lost
            LogStartFailed(Logger, _context.HubName, ex);
        }
    }


    private async Task OnGroupMessageReceivedAsync(WebPubSubGroupMessageEventArgs args)
    {
        using var activity = WebPubSubTelemetry.Source.StartActivity(
            WebPubSubTelemetry.ProcessGroupOperation,
            ActivityKind.Consumer);

        if (activity is not null)
            activity.DisplayName = $"{WebPubSubTelemetry.ProcessGroupOperation} {args.Message.Group}";

        activity?.SetTag(WebPubSubTelemetry.MessagingSystemTag, "azure.webpubsub");
        activity?.SetTag(WebPubSubTelemetry.DestinationNameTag, _context.HubName);
        activity?.SetTag(WebPubSubTelemetry.DestinationGroupTag, args.Message.Group);
        activity?.SetTag(WebPubSubTelemetry.OperationTypeTag, "process");

        try
        {
            await ProcessGroupMessageAsync(args).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception exception)
        {
            WebPubSubTelemetry.RecordException(activity, exception);
            throw;
        }
    }

    private async Task OnServerMessageReceivedAsync(WebPubSubServerMessageEventArgs args)
    {
        using var activity = WebPubSubTelemetry.Source.StartActivity(
            WebPubSubTelemetry.ProcessServerOperation,
            ActivityKind.Consumer);

        if (activity is not null)
            activity.DisplayName = $"{WebPubSubTelemetry.ProcessServerOperation} {_context.HubName}";

        activity?.SetTag(WebPubSubTelemetry.MessagingSystemTag, "azure.webpubsub");
        activity?.SetTag(WebPubSubTelemetry.DestinationNameTag, _context.HubName);
        activity?.SetTag(WebPubSubTelemetry.OperationTypeTag, "process");

        try
        {
            await ProcessServerMessageAsync(args).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception exception)
        {
            WebPubSubTelemetry.RecordException(activity, exception);
            throw;
        }
    }

    private Task OnConnectedAsync(WebPubSubConnectedEventArgs args)
    {
        LogConnected(Logger, _context.HubName, args.ConnectionId);
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(WebPubSubDisconnectedEventArgs args)
    {
        LogDisconnected(Logger, _context.HubName, args.ConnectionId);
        return Task.CompletedTask;
    }


    private static WebPubSubClient CreateClient(WebPubSubHubContext context)
    {
        var options = context.Options;

        // the client access token must grant join and send permissions for each group, otherwise the
        // service rejects the join request with "The client does not have permission to join group"
        var roles = context.Groups.Values
            .SelectMany(group => new[]
            {
                $"webpubsub.joinLeaveGroup.{group}",
                $"webpubsub.sendToGroup.{group}",
            })
            .ToArray();

        var credential = new WebPubSubClientCredential(
            clientAccessUriProvider: async cancellationToken =>
                await context.ServiceClient
                    .GetClientAccessUriAsync(
                        expiresAfter: options.ClientAccessTokenExpiration,
                        roles: roles,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false)
        );

        var clientOptions = new WebPubSubClientOptions
        {
            AutoReconnect = options.AutoReconnect,
            AutoRejoinGroups = options.AutoRejoinGroups,
            Protocol = options.UseReliableProtocol
                ? new WebPubSubJsonReliableProtocol()
                : new WebPubSubJsonProtocol(),
        };

        return new WebPubSubClient(credential, clientOptions);
    }


    /// <summary>
    /// Releases the resources used by the processor.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="DisposeAsync" />; the synchronous path blocks while the underlying client is disposed.
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases the resources used by the processor.
    /// </summary>
    /// <returns>A <see cref="ValueTask" /> that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
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
            _startedRegistration.Dispose();
            _stoppingTokenSource.Dispose();

            DetachHandlers();

            // WebPubSubClient only exposes asynchronous disposal; block here so the synchronous
            // IDisposable contract is honored. Prefer DisposeAsync to avoid sync-over-async.
            _client.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        _disposed = true;
    }

    /// <summary>
    /// Asynchronously releases the managed resources used by the processor.
    /// </summary>
    /// <returns>A <see cref="ValueTask" /> that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// Override to release additional managed resources; call the base implementation to detach handlers
    /// and dispose the underlying client.
    /// </remarks>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        _startedRegistration.Dispose();
        _stoppingTokenSource.Dispose();

        DetachHandlers();

        await _client.DisposeAsync().ConfigureAwait(false);

        _disposed = true;
    }


    private void DetachHandlers()
    {
        if (!_handlersAttached)
            return;

        _client.GroupMessageReceived -= OnGroupMessageReceivedAsync;
        _client.ServerMessageReceived -= OnServerMessageReceivedAsync;
        _client.Connected -= OnConnectedAsync;
        _client.Disconnected -= OnDisconnectedAsync;
        _handlersAttached = false;
    }


    [LoggerMessage(Level = LogLevel.Information, Message = "Starting Web PubSub processor for hub '{HubName}'")]
    private static partial void LogStartingProcessor(ILogger logger, string hubName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Stopping Web PubSub processor for hub '{HubName}'")]
    private static partial void LogStoppingProcessor(ILogger logger, string hubName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error starting Web PubSub processor for hub '{HubName}'")]
    private static partial void LogStartFailed(ILogger logger, string hubName, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Joining Web PubSub group '{GroupName}' on hub '{HubName}'")]
    private static partial void LogJoiningGroup(ILogger logger, string groupName, string hubName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Connected to Web PubSub hub '{HubName}' with connection '{ConnectionId}'")]
    private static partial void LogConnected(ILogger logger, string hubName, string? connectionId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Disconnected from Web PubSub hub '{HubName}' with connection '{ConnectionId}'")]
    private static partial void LogDisconnected(ILogger logger, string hubName, string? connectionId);
}
