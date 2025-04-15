namespace Arbiter;

/// <summary>
/// An <see langword="interface"/> for a request handler
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
/// <remarks>
/// There should be only one handlers for a request.  Use <see cref="IPipelineBehavior{TRequest, TResponse}"/> to create a pipeline of additional behaviors.
/// </remarks>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles a request and returns the response
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse"/></returns>
    /// <exception cref="ArgumentNullException">When <paramref name="request"/> is null</exception>"
    ValueTask<TResponse?> Handle(TRequest request, CancellationToken cancellationToken = default);
}
