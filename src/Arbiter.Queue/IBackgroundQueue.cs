using Arbiter.Mediation;

namespace Arbiter.Queue;

/// <summary>
/// Represents a queue for background mediator requests.
/// </summary>
public interface IBackgroundQueue
{
    /// <summary>
    /// Enqueues a request to be processed in the background.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to enqueue.</typeparam>
    /// <param name="request">The request to enqueue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task for the enqueue operation.</returns>
    ValueTask Enqueue<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest;
}
