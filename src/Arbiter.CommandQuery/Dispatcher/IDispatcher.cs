namespace Arbiter.CommandQuery.Dispatcher;

public interface IDispatcher
{
    ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
}
