using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Mediation;

/// <summary>
/// A default implementation of the <see cref="IMediator"/> interface.
/// </summary>
public sealed class Mediator : IMediator
{
    private static readonly ConcurrentDictionary<Type, IHandler> _handlerCache = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly Counter<long> _sendCounter;
    private readonly Counter<long> _publishCounter;
    private readonly Histogram<double> _sendDuration;
    private readonly Histogram<double> _publishDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve handlers and behaviors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;

        // Use IMeterFactory when available (production / host). Fall back to a standalone
        // Meter that is a no-op when no MeterProvider is listening (e.g. unit tests).
        var meterFactory = serviceProvider.GetService<IMeterFactory>();

        var meter = meterFactory?.Create(MediatorTelemetry.MeterName, ThisAssembly.Version)
            ?? new Meter(MediatorTelemetry.MeterName, ThisAssembly.Version);

        _sendCounter = meter.CreateCounter<long>(MediatorTelemetry.SendCount, "requests", "Number of mediator send operations");
        _publishCounter = meter.CreateCounter<long>(MediatorTelemetry.PublishCount, "notifications", "Number of mediator publish operations");
        _sendDuration = meter.CreateHistogram<double>(MediatorTelemetry.SendDuration, "ms", "Duration of mediator send operations");
        _publishDuration = meter.CreateHistogram<double>(MediatorTelemetry.PublishDuration, "ms", "Duration of mediator publish operations");
    }

    /// <inheritdoc />
    public async ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = typeof(TRequest);
        Type responseType = typeof(TResponse);

        using var activity = MediatorTelemetry.Source.StartActivity(
            name: $"{MediatorTelemetry.SendOperation} {requestType.Name}",
            kind: ActivityKind.Internal);

        activity?.SetTag(MediatorTelemetry.RequestTypeTag, requestType.FullName);
        activity?.SetTag(MediatorTelemetry.ResponseTypeTag, responseType.FullName);

        var startTime = Stopwatch.GetTimestamp();
        try
        {
            var handler = (IHandler<TResponse>)_handlerCache.GetOrAdd(requestType, _
                => new RequestHandler<TRequest, TResponse>());

            // create a new scope for each request to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                var result = await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);

                _sendCounter.Add(1);
                activity?.SetStatus(ActivityStatusCode.Ok);

                return result;
            }
        }
        catch (Exception ex)
        {
            MediatorTelemetry.RecordException(activity, ex);
            throw;
        }
        finally
        {
            _sendDuration.Record(Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        }
    }

    /// <inheritdoc />
    public async ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        using var activity = MediatorTelemetry.Source.StartActivity(
            name: $"{MediatorTelemetry.SendOperation} {requestType.Name}",
            kind: ActivityKind.Internal);

        activity?.SetTag(MediatorTelemetry.RequestTypeTag, requestType.FullName);
        activity?.SetTag(MediatorTelemetry.ResponseTypeTag, responseType.FullName);

        var startTime = Stopwatch.GetTimestamp();
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
                var result = await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);

                _sendCounter.Add(1);
                activity?.SetStatus(ActivityStatusCode.Ok);

                return result;
            }
        }
        catch (Exception ex)
        {
            MediatorTelemetry.RecordException(activity, ex);
            throw;
        }
        finally
        {
            _sendDuration.Record(Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        }
    }

    /// <inheritdoc />
    public async ValueTask<object?> Send(
        object request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var typeKey = request.GetType();

        using var activity = MediatorTelemetry.Source.StartActivity(
            name: $"{MediatorTelemetry.SendOperation} {typeKey.Name}",
            kind: ActivityKind.Internal);

        activity?.SetTag(MediatorTelemetry.RequestTypeTag, typeKey.FullName);

        var startTime = Stopwatch.GetTimestamp();
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
                var result = await handler
                    .Handle(request, serviceScope.ServiceProvider, cancellationToken)
                    .ConfigureAwait(false);

                _sendCounter.Add(1);
                activity?.SetStatus(ActivityStatusCode.Ok);

                return result;
            }
        }
        catch (Exception ex)
        {
            MediatorTelemetry.RecordException(activity, ex);
            throw;
        }
        finally
        {
            _sendDuration.Record(Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        }
    }

    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        using var activity = MediatorTelemetry.Source.StartActivity(
            name: $"{MediatorTelemetry.PublishOperation} {typeof(TNotification).Name}",
            kind: ActivityKind.Internal);

        activity?.SetTag(MediatorTelemetry.NotificationTypeTag, typeof(TNotification).FullName);

        var startTime = Stopwatch.GetTimestamp();
        try
        {
            // create a new scope to make sure handlers are disposed
            var serviceScope = _serviceProvider.CreateAsyncScope();
            await using (serviceScope.ConfigureAwait(false))
            {
                var handlers = serviceScope.ServiceProvider.GetServices<INotificationHandler<TNotification>>().ToArray();
                if (handlers.Length == 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return;
                }

                // start all handlers then await them
                var tasks = handlers.Select(handler => handler.Handle(notification, cancellationToken)).ToArray();
                for (var i = 0; i < tasks.Length; i++)
                    await tasks[i].ConfigureAwait(false);

                _publishCounter.Add(1);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
        }
        catch (Exception ex)
        {
            MediatorTelemetry.RecordException(activity, ex);
            throw;
        }
        finally
        {
            _publishDuration.Record(Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
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
