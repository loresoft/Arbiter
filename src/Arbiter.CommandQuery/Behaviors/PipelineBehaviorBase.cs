using System.Diagnostics;

using Arbiter.CommandQuery.Handlers;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A base pipeline behavior to surround the inner handler
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public abstract partial class PipelineBehaviorBase<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly string _typeName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineBehaviorBase{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this handler.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="loggerFactory"/> is null</exception>"
    protected PipelineBehaviorBase(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        // Capture the type name once to avoid repeated calls to GetType in logging
        var type = GetType();
        _typeName = type.Name;

        Logger = loggerFactory.CreateLogger(type);
    }

    /// <summary>
    /// Gets the <see cref="ILogger"/> for this pipeline behavior.
    /// </summary>
    /// <value>
    /// The <see cref="ILogger"/> for this pipeline behavior.
    /// </value>
    protected ILogger Logger { get; }


    /// <inheritdoc />
    public virtual async ValueTask<TResponse?> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var startTime = Stopwatch.GetTimestamp();
        try
        {
            LogStart(Logger, _typeName, request);
            return await Process(request, next, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            var elaspsed = Stopwatch.GetElapsedTime(startTime);
            LogFinish(Logger, _typeName, request, elaspsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Processes the specified request with the additional behavior.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse" /></returns>
    protected abstract ValueTask<TResponse?> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);


    [LoggerMessage(1, LogLevel.Trace, "Processing behavior '{Behavior}' for request '{Request}' ...")]
    static partial void LogStart(ILogger logger, string behavior, IRequest<TResponse> request);

    [LoggerMessage(2, LogLevel.Trace, "Processed behavior '{Behavior}' for request '{Request}': {Elapsed} ms")]
    static partial void LogFinish(ILogger logger, string behavior, IRequest<TResponse> request, double elapsed);
}
