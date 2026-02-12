using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Mediation;

/// <summary>
/// A default implementation of the <see cref="IMediator"/> interface.
/// </summary>
/// <param name="serviceProvider">Service provider to resolve handlers and behaviors.</param>
/// <param name="diagnostic">An optional diagnostic service for logging activities and metrics.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null</exception>
public sealed class Mediator(IServiceProvider serviceProvider, IMediatorDiagnostic? diagnostic = null) : IMediator
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

        using var activity = diagnostic?.StartSend<TRequest, TResponse>();

        try
        {
            var handler = (IHandler<TResponse>)_handlerCache.GetOrAdd(typeof(TRequest), _
                => new RequestHandler<TRequest, TResponse>());

            // create a new scope for each request to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                return await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            diagnostic?.ActivityError(activity, ex, request);
            throw;
        }
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("This overload relies on reflection over types that may be removed when trimming.")]
    [RequiresDynamicCode("This overload uses MakeGenericType which requires dynamic code generation.")]
    public async ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        using var activity = diagnostic?.StartSend(request);

        try
        {
            var handler = (IHandler<TResponse>)_handlerCache.GetOrAdd(requestType, _ =>
            {
                var wrapperType = typeof(RequestHandler<,>).MakeGenericType(requestType, responseType);
                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Unable to create instance of {wrapperType}.");

                return (IHandler)wrapper;
            });

            // create a new scope for each request to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                return await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            diagnostic?.ActivityError(activity, ex, request);
            throw;
        }
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("This overload relies on reflection over types that may be removed when trimming.")]
    [RequiresDynamicCode("This overload uses MakeGenericType which requires dynamic code generation.")]
    public async ValueTask<object?> Send(
        object request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var typeKey = request.GetType();

        using var activity = diagnostic?.StartSend(request);

        try
        {
            var handler = _handlerCache.GetOrAdd(typeKey, requestType =>
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

            // create a new scope for each request to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                return await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            diagnostic?.ActivityError(activity, ex, request);
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

        using var activity = diagnostic?.StartPublish<TNotification>();

        try
        {
            // create a new scope to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                var handlers = serviceScope.ServiceProvider.GetServices<INotificationHandler<TNotification>>().ToArray();
                if (handlers.Length == 0)
                    return;

                // start all handlers then await them
                var tasks = handlers.Select(handler => handler.Handle(notification, cancellationToken)).ToArray();
                for (var i = 0; i < tasks.Length; i++)
                    await tasks[i].ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            diagnostic?.ActivityError(activity, ex, notification);
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
        public readonly ValueTask<TResponse?> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            var child = next;

            return behavior.Handle(
                request,
                token => child.Handle(request, token),
                cancellationToken);
        }
    }
}
