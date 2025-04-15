namespace Arbiter.CommandQuery.Dispatcher;

/// <summary>
/// An <see langword="interface"/> to represent a dispatcher for sending request messages.
/// </summary>
/// <remarks>
/// Dispatcher is an abstraction over the <see cref="IMediator"/> pattern, allowing for sending of requests over
/// HTTP for remote scenarios and directly to <see cref="IMediator"/> for server side scenarios.  Use this abstraction
/// when using the Blazor Interactive Auto rendering mode.
/// </remarks>
public interface IDispatcher
{
    /// <summary>
    /// Sends a request to the message dispatcher.
    /// </summary>
    /// <typeparam name="TRequest"> The type of request being sent</typeparam>
    /// <typeparam name="TResponse"> The type of response from the dispatcher</typeparam>
    /// <param name="request">The request being sent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse"/></returns>
    ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
}
