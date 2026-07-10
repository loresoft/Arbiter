using System.Threading.Channels;

using Arbiter.Mediation;

namespace Arbiter.Queue;

/// <summary>
/// Provides an in-process background queue for mediator requests.
/// </summary>
public sealed class BackgroundQueue : IBackgroundQueue
{
    private readonly Channel<IRequest> _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundQueue" /> class.
    /// </summary>
    public BackgroundQueue()
    {
        var options = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        };

        _channel = Channel.CreateUnbounded<IRequest>(options);
    }

    /// <inheritdoc />
    public ValueTask Enqueue<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);
        return _channel.Writer.WriteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets the channel reader for the background queue.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous dequeue operation. The value of the TResult parameter contains the dequeued request.</returns>
    public ValueTask<IRequest> Dequeue(
        CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAsync(cancellationToken);
}
