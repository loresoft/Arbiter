using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Mediation;

/// <summary>
/// A default implementation of the <see cref="IMediator"/> interface.
/// </summary>
/// <param name="serviceProvider">Service provider to resolve handlers and behaviors</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null</exception>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, IHandler> _handlerCache = new();

    private readonly IServiceProvider _serviceProvider = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <inheritdoc />
    public async ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);

        using var activity = Diagnostics.ActivitySource.StartActivity(Diagnostics.Activties.SendName, ActivityKind.Internal);
        activity?.SetTag(Diagnostics.Tags.RequestType, requestType.FullName);
        activity?.SetTag(Diagnostics.Tags.ResponseType, responseType.FullName);

        try
        {
            Diagnostics.Metrics.SendCounter.Increment(1, requestType.FullName, responseType.FullName);

            var handler = (IHandler<TResponse>)_handlerCache.GetOrAdd(requestType, _
                => new RequestHandler<TRequest, TResponse>());

            using var serviceScope = _serviceProvider.CreateScope();

            return await handler
                .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Diagnostics.Metrics.ErrorsCounter.Increment(1, requestType.FullName, responseType.FullName);

            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("exception", ex.ToString());
            throw;
        }
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("This overload relies on reflection over types that may be removed when trimming.")]
    public async ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        using var activity = Diagnostics.ActivitySource.StartActivity(Diagnostics.Activties.SendName, ActivityKind.Internal);
        activity?.SetTag(Diagnostics.Tags.RequestType, requestType.FullName);
        activity?.SetTag(Diagnostics.Tags.ResponseType, responseType.FullName);

        try
        {
            Diagnostics.Metrics.SendCounter.Increment(1, requestType.FullName, responseType.FullName);

            var handler = (IHandler<TResponse>)_handlerCache.GetOrAdd(requestType, _ =>
            {
                var wrapperType = typeof(RequestHandler<,>).MakeGenericType(requestType, responseType);
                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Unable to create instance of {wrapperType}.");

                return (IHandler)wrapper;
            });

            using var serviceScope = _serviceProvider.CreateScope();

            return await handler
                .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Diagnostics.Metrics.ErrorsCounter.Increment(1, requestType.FullName, responseType.FullName);

            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("exception", ex.ToString());
            throw;
        }
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("This overload relies on reflection over types that may be removed when trimming.")]
    public async ValueTask<object?> Send(
        object request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        using var activity = Diagnostics.ActivitySource.StartActivity(Diagnostics.Activties.SendName, ActivityKind.Internal);
        activity?.SetTag(Diagnostics.Tags.RequestType, requestType.FullName);

        try
        {
            Diagnostics.Metrics.SendCounter.Increment(1, requestType.FullName);

            var handler = _handlerCache.GetOrAdd(requestType, _ =>
            {
                // Get the generic response type
                var requestInterfaceType = requestType
                    .GetInterfaces()
                    .FirstOrDefault(static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

                if (requestInterfaceType is null)
                    throw new InvalidOperationException($"Request type {requestType} does not implement IRequest<> interface.");

                var genericArguments = requestInterfaceType.GetGenericArguments();
                if (genericArguments.Length != 1)
                    throw new InvalidOperationException($"Request type {requestType} does not implement IRequest<> interface with a single generic argument.");

                var responseType = genericArguments[0];

                var wrapperType = typeof(RequestHandler<,>).MakeGenericType(requestType, responseType);

                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Unable to create instance of {wrapperType}.");

                return (IHandler)wrapper;
            });

            using var serviceScope = _serviceProvider.CreateScope();

            return await handler
                .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Diagnostics.Metrics.ErrorsCounter.Increment(1, requestType.FullName);

            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("exception", ex.ToString());
            throw;
        }
    }


    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = typeof(TNotification);

        using var activity = Diagnostics.ActivitySource.StartActivity(Diagnostics.Activties.PublishName, ActivityKind.Internal);
        activity?.SetTag(Diagnostics.Tags.NotificationType, notificationType.FullName);

        try
        {
            Diagnostics.Metrics.PublishedCounter.Increment(1, notificationType: notificationType.FullName);

            using var serviceScope = _serviceProvider.CreateScope();

            var handlers = serviceScope.ServiceProvider.GetServices<INotificationHandler<TNotification>>().ToArray();
            if (handlers.Length == 0)
                return;

            // start all handlers then await them
            var tasks = handlers.Select(handler => handler.Handle(notification, cancellationToken)).ToArray();
            for (var i = 0; i < tasks.Length; i++)
                await tasks[i].ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            Diagnostics.Metrics.ErrorsCounter.Increment(1, notificationType: notificationType.FullName);

            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("exception", ex.ToString());
            throw;
        }
    }


    private interface IHandler
    {
        ValueTask<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    private interface IHandler<TResponse> : IHandler
    {
        ValueTask<TResponse?> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    private readonly struct RequestHandler<TRequest, TResponse> : IHandler<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async ValueTask<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return await Handle((TRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<TResponse?> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToArray();

            var pipeline = handler;

            // reverse the behaviors to maintain the order of execution
            for (var i = behaviors.Length - 1; i >= 0; i--)
                pipeline = new PipelineBehavior<TRequest, TResponse>(behaviors[i], pipeline);

            return pipeline.Handle((TRequest)request, cancellationToken);
        }
    }

    private readonly struct PipelineBehavior<TRequest, TResponse>(
            IPipelineBehavior<TRequest, TResponse> behavior,
            IRequestHandler<TRequest, TResponse> next)
            : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private readonly IPipelineBehavior<TRequest, TResponse> _behavior = behavior;
        private readonly IRequestHandler<TRequest, TResponse> _next = next;

        public readonly ValueTask<TResponse?> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            var child = _next;
            ValueTask<TResponse?> handler(CancellationToken token) => child.Handle(request, token);

            return _behavior.Handle(
                request,
                handler,
                cancellationToken);
        }
    }
}
