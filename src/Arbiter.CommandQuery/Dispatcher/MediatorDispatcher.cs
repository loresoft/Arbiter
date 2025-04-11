namespace Arbiter.CommandQuery.Dispatcher;

public class MediatorDispatcher : IDispatcher
{
    private readonly IMediator _mediator;

    public MediatorDispatcher(IMediator sender)
    {
        _mediator = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        return await _mediator.Send<TRequest, TResponse>(request, cancellationToken).ConfigureAwait(false);
    }
}
