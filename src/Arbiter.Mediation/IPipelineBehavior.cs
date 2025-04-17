namespace Arbiter.Mediation;

/// <summary>
/// An <see langword="interface"/> for a pipeline behavior to surround the inner handler.
/// Implementations add additional behavior and await the next delegate.
/// </summary>
/// <typeparam name="TRequest"> The type of request being sent</typeparam>
/// <typeparam name="TResponse"> The type of response from the handler</typeparam>
/// <remarks>
/// There can be multiple pipeline behaviors for a request handler.
/// </remarks>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Pipeline handler to perform any additional behavior and await the <paramref name="next"/> delegate as necessary
    /// </summary>
    /// <param name="request">The request being sent</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse"/></returns>
    ValueTask<TResponse?> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
