namespace Arbiter;

/// <summary>
/// An <see langword="interface"/> defining a mediator to encapsulate request/response and publishing patterns
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to the appropriate handler and returns the response
    /// </summary>
    /// <typeparam name="TRequest"> The type of request being sent</typeparam>
    /// <typeparam name="TResponse"> The type of response from the handler</typeparam>
    /// <param name="request">Incoming request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null</exception>
    ValueTask<TResponse> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Sends a notification to multiple handlers
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being sent</typeparam>
    /// <param name="notification">The notification to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task for notification operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null</exception>
    ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
