using Microsoft.Extensions.DependencyInjection;

namespace Arbiter;

/// <summary>
/// A default implementation of the <see cref="IMediator"/> interface.
/// </summary>
/// <param name="serviceProvider">Service provider to resolve handlers and behaviors</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null</exception>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <inheritdoc />
    public async ValueTask<TResponse> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);

        var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        var pipeline = _serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate(handler, static (next, behavior) => new PipelineBehaviorDelegate<TRequest, TResponse>(behavior, next));

        return await pipeline.Handle(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>().ToList();
        if (handlers.Count == 0)
            return;

        // start all handlers then await them
        foreach (var task in handlers.Select(handler => handler.Handle(notification, cancellationToken)))
            await task.ConfigureAwait(false);
    }


    private readonly struct PipelineBehaviorDelegate<TRequest, TResponse>(
            IPipelineBehavior<TRequest, TResponse> behavior,
            IRequestHandler<TRequest, TResponse> next)
            : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private readonly IPipelineBehavior<TRequest, TResponse> _behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
        private readonly IRequestHandler<TRequest, TResponse> _next = next ?? throw new ArgumentNullException(nameof(next));

        public readonly ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            var child = _next;
            ValueTask<TResponse> handler(CancellationToken token) => child.Handle(request, token);

            return _behavior.Handle(
                request,
                handler,
                cancellationToken);
        }
    }
}
