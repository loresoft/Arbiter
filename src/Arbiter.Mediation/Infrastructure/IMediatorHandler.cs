namespace Arbiter.Mediation.Infrastructure;

/// <summary>
/// Internal handler interface for mediator request processing.
/// </summary>
public interface IMediatorHandler
{
    /// <summary>
    /// Handles a request using runtime type information.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="serviceProvider">Service provider for resolving handlers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    ValueTask<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

/// <summary>
/// Internal typed handler interface for mediator request processing.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IMediatorHandler<TResponse> : IMediatorHandler
{
    /// <summary>
    /// Handles a request with compile-time type information.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="serviceProvider">Service provider for resolving handlers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    ValueTask<TResponse?> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

