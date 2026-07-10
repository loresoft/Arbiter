using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Processes Service Bus messages containing background mediator requests.
/// </summary>
/// <param name="processor">The Service Bus processor used to receive background request messages.</param>
/// <param name="logger">The logger used to write processor diagnostics.</param>
/// <param name="backgroundService">The service used to deserialize and dispatch received messages.</param>
public sealed class ServiceBusBackgroundProcessor(
    ServiceBusProcessor processor,
    ILogger<ServiceBusBackgroundProcessor> logger,
    ServiceBusBackgroundService backgroundService)
    : ServiceBusProcessorBase(processor, logger)
{
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "<Pending>")]
    protected override async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        await backgroundService
            .ProcessMessageAsync(args)
            .ConfigureAwait(false);
    }
}
