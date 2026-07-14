using Arbiter.Mediation;
using Arbiter.Queue;

using Azure.Messaging.ServiceBus;

using MessagePack;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides a Service Bus backed background queue for mediator requests.
/// </summary>
public sealed class ServiceBusBackgroundQueue(
    ServiceBusSender sender,
    MessagePackSerializerOptions? options = null)
    : IBackgroundQueue
{
    /// <inheritdoc />
    public async ValueTask Enqueue<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        await sender
            .SendAsMessagePackAsync(request, options, cancellationToken)
            .ConfigureAwait(false);
    }
}
