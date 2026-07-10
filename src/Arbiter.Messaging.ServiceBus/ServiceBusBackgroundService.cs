using Arbiter.CommandQuery.Extensions;
using Arbiter.Mediation;

using Azure.Messaging.ServiceBus;

using MessagePack;

using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Processes Azure Service Bus messages as mediator requests.
/// </summary>
public class ServiceBusBackgroundService
{
    private readonly ILogger<ServiceBusBackgroundService> _logger;
    private readonly IMediator _mediator;
    private readonly MessagePackSerializerOptions? _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusBackgroundService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to write message processing diagnostics.</param>
    /// <param name="mediator">The mediator used to dispatch deserialized requests.</param>
    /// <param name="options">The optional MessagePack serializer options used to deserialize message bodies.</param>
    public ServiceBusBackgroundService(
        ILogger<ServiceBusBackgroundService> logger,
        IMediator mediator,
        MessagePackSerializerOptions? options = null)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options;
    }

    /// <summary>
    /// Processes a received Service Bus message by deserializing it as a mediator request and dispatching it.
    /// </summary>
    /// <param name="processMessage">The message event arguments containing the received message and settlement operations.</param>
    /// <returns>A task that represents the asynchronous message processing operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="processMessage"/> is <see langword="null" />.</exception>
    public Task ProcessMessageAsync(ProcessMessageEventArgs processMessage)
    {
        ArgumentNullException.ThrowIfNull(processMessage);

        var message = processMessage.Message;

        if (message == null)
        {
            _logger.LogError("Message is null");
            return Task.CompletedTask;
        }

        return ProcessMessageAsync(
            message,
            completeMessage: processMessage.CompleteMessageAsync,
            deadLetterMessage: processMessage.DeadLetterMessageAsync,
            cancellationToken: processMessage.CancellationToken);
    }

    /// <summary>
    /// Processes a received Service Bus message by deserializing it as a mediator request and dispatching it.
    /// </summary>
    /// <param name="message">The Service Bus message to process.</param>
    /// <param name="completeMessage">The callback used to complete the message after successful dispatch.</param>
    /// <param name="deadLetterMessage">The callback used to dead-letter invalid messages or messages that fail during dispatch.</param>
    /// <param name="cancellationToken">A token used to cancel the processing operation.</param>
    /// <returns>A task that represents the asynchronous message processing operation.</returns>
    /// <remarks>
    /// This overload can be used from Azure Functions Service Bus triggers by passing the trigger message and settlement
    /// callbacks from <c>ServiceBusMessageActions</c>, without requiring this library to reference Azure Functions packages.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="message" />, <paramref name="completeMessage" />, or <paramref name="deadLetterMessage" /> is <see langword="null" />.
    /// </exception>
    public async Task ProcessMessageAsync(
        ServiceBusReceivedMessage message,
        Func<ServiceBusReceivedMessage, CancellationToken, Task> completeMessage,
        Func<ServiceBusReceivedMessage, string, string?, CancellationToken, Task> deadLetterMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(completeMessage);
        ArgumentNullException.ThrowIfNull(deadLetterMessage);

        if (message.Subject.IsNullOrWhiteSpace())
        {
            _logger.LogError("Message subject is empty, unable to determine request type");

            await deadLetterMessage(message, "Message subject is empty, unable to determine request type", null, cancellationToken).ConfigureAwait(false);

            return;
        }


        using var scope = _logger.BeginScope("Processing message ID: {MessageId}; Type: {MessageType}", message.MessageId, message.Subject);

        try
        {
            var request = message.ReadFromMessagePack<IRequest>(_options, cancellationToken);
            if (request == null)
            {
                _logger.LogError("Invalid background message, failed to deserialize");

                await deadLetterMessage(message, "Invalid background message, failed to deserialize", null, cancellationToken).ConfigureAwait(false);

                return;
            }

            await _mediator
                .Send(request, cancellationToken)
                .ConfigureAwait(false);

            // Complete the message
            await completeMessage(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message ID {MessageId}: {ErrorMessage}", message.MessageId, ex.Message);

            await deadLetterMessage(message, "Error processing message", ex.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}
