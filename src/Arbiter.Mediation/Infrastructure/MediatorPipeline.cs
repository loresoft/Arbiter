namespace Arbiter.Mediation.Infrastructure;

/// <summary>
/// Represents a pipeline that executes a behavior and then calls the next handler in the chain.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public readonly struct MediatorPipeline<TRequest, TResponse>(
        IPipelineBehavior<TRequest, TResponse> behavior,
        IRequestHandler<TRequest, TResponse> next)
        : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request by executing the behavior and then calling the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The response from the handler.</returns>
    public readonly ValueTask<TResponse?> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        var child = next;

        return behavior.Handle(
            request,
            token => child.Handle(request, token),
            cancellationToken);
    }
}
