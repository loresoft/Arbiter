using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Arbiter.Mediation.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Mediation;

/// <summary>
/// A default implementation of the <see cref="IMediator"/> interface.
/// </summary>
/// <param name="serviceProvider">Service provider to resolve handlers and behaviors.</param>
/// <param name="diagnostic">An optional diagnostic service for logging activities and metrics.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null</exception>
public sealed class ReflectionMediator(IServiceProvider serviceProvider, IMediatorDiagnostic? diagnostic = null) : IMediator
{
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
            var handler = (IMediatorHandler<TResponse>)MediatorRegistry.GetOrAdd(typeof(TRequest), _
                => new MediatorHandler<TRequest, TResponse>());

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
            var handler = (IMediatorHandler<TResponse>)MediatorRegistry.GetOrAdd(requestType, _ =>
            {
                var wrapperType = typeof(MediatorHandler<,>).MakeGenericType(requestType, responseType);
                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Unable to create instance of {wrapperType}.");

                return (IMediatorHandler)wrapper;
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
            var handler = MediatorRegistry.GetOrAdd(typeKey, static requestType =>
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

                var wrapperType = typeof(MediatorHandler<,>).MakeGenericType(requestType, responseType);

                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Unable to create instance of {wrapperType}.");

                return (IMediatorHandler)wrapper;
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
}
