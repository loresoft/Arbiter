using Arbiter.Mediation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbiter.Queue;

/// <summary>
/// Processes queued background mediator requests.
/// </summary>
internal sealed partial class BackgroundQueueWorker(
    ILogger<BackgroundQueueWorker> logger,
    BackgroundQueue backgroundQueue,
    IServiceProvider serviceProvider)
    : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IRequest? item = null;

            try
            {
                item = await backgroundQueue
                    .Dequeue(stoppingToken)
                    .ConfigureAwait(false);

                // need to use service scope to resolve mediator in case it is registered as scoped
                var mediator = serviceProvider.GetRequiredService<IMediator>();

                await mediator
                    .Send(item, stoppingToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                var type = item?.GetType();
                var typeName = type?.FullName ?? type?.Name ?? "Unknown";

                LogProcessingError(logger, ex, typeName, ex.Message);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing background request '{RequestType}': {ErrorMessage}")]
    private static partial void LogProcessingError(ILogger logger, Exception exception, string requestType, string errorMessage);
}
