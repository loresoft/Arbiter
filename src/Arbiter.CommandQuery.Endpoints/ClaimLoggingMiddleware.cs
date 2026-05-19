using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Middleware that resolves user identity claims from <see cref="HttpContext.User"/> and adds them to
/// the ambient logging scope and current <see cref="System.Diagnostics.Activity"/> for downstream telemetry.
/// </summary>
public class ClaimLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClaimLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The request logging configuration options.</param>
    public ClaimLoggingMiddleware(
        RequestDelegate next,
        ILogger<ClaimLoggingMiddleware> logger,
        IOptions<RequestLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Invokes the middleware to enrich downstream logging scopes and activity tags.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = ClaimLoggingContext.BeginScope(_logger, context.User, _options);

        await _next(context).ConfigureAwait(false);
    }
}
