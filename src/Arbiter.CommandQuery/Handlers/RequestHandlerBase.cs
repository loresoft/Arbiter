using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Handlers;

/// <summary>
/// A base handler for a request
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public abstract partial class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly string _typeName = typeof(RequestHandlerBase<TRequest, TResponse>).Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestHandlerBase{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this handler.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="loggerFactory"/> is null</exception>"
    protected RequestHandlerBase(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        Logger = loggerFactory.CreateLogger(_typeName);
    }

    /// <summary>
    /// Gets the <see cref="ILogger"/> for this handler.
    /// </summary>
    /// <value>
    /// The <see cref="ILogger"/> for this handler.
    /// </value>
    protected ILogger Logger { get; }

    /// <inheritdoc />
    public virtual async ValueTask<TResponse?> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startTime = Stopwatch.GetTimestamp();
        try
        {
            LogStart(Logger, _typeName, request);
            return await Process(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            LogFinish(Logger, _typeName, request, elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Processes the specified request.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse" /></returns>
    protected abstract ValueTask<TResponse?> Process(TRequest request, CancellationToken cancellationToken = default);


    [LoggerMessage(1, LogLevel.Trace, "Processing handler '{Handler}' for request '{Request}' ...")]
    static partial void LogStart(ILogger logger, string handler, IRequest<TResponse> request);

    [LoggerMessage(2, LogLevel.Trace, "Processed handler '{Handler}' for request '{Request}': {Elapsed} ms")]
    static partial void LogFinish(ILogger logger, string handler, IRequest<TResponse> request, double elapsed);
}
