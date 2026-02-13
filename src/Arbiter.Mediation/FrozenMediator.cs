using System.Diagnostics.CodeAnalysis;

using Arbiter.Mediation.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Mediation;

/// <summary>
/// Provides a mediator implementation that facilitates sending requests and publishing notifications while managing
/// service scopes.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It ensures that handlers are disposed of properly by
/// creating a new service scope for each request or notification. It is important to register request handlers with the
/// MediatorRegistry during application startup for proper functionality.</remarks>
/// <param name="serviceProvider">The service provider used to resolve dependencies for handling requests and notifications.</param>
/// <param name="diagnostic">An optional diagnostic object used for tracking and logging the mediation process.</param>
public sealed class FrozenMediator(IServiceProvider serviceProvider, IMediatorDiagnostic? diagnostic = null) : IMediator
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
            var handler = GetHandler(typeof(TRequest));

            // create a new scope for each request to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                var result = await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);

                return (TResponse?)result;
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

        using var activity = diagnostic?.StartSend(request);

        try
        {
            var handler = GetHandler(request.GetType());

            // create a new scope for each request to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                var result = await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);

                return (TResponse?)result;
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

        using var activity = diagnostic?.StartSend(request);

        try
        {
            var handler = GetHandler(request.GetType());

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

    private static IMediatorHandler GetHandler(Type requestType)
    {
        if (MediatorRegistry.TryGetHandler(requestType, out var handler) && handler != null)
            return handler;

        throw new InvalidOperationException(
            $"No mediator handler registered for request type '{requestType.FullName}'. " +
            $"FrozenMediator requires MediatorRegistry.Register() to be called during application startup " +
            $"to pre-register requests for AOT compatibility.");
    }
}
