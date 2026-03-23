using System.Diagnostics.CodeAnalysis;

using Arbiter.Mediation;

namespace Arbiter.Dispatcher.Client;

/// <summary>
/// A dispatcher that uses <see cref="IMediator"/> to send requests.  Use for Blazor Interactive Server rendering mode.
/// </summary>
public class ServerDispatcher : IDispatcher
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerDispatcher"/> class.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="mediator"/> is null</exception>
    public ServerDispatcher(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <inheritdoc />
    public ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        return _mediator.Send<TRequest, TResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return _mediator.Send(request, cancellationToken);
    }
}
