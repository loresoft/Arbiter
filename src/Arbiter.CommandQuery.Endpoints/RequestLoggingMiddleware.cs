using System.Buffers;
using System.Diagnostics;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Middleware for logging HTTP request and response details.
/// </summary>
public partial class RequestLoggingMiddleware
{
    // Optimal buffer size for most request bodies
    private const int DefaultBufferSize = 1024;

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;
    private readonly ReadOnlyMemory<char>[] _allowedMimeTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The request logging configuration options.</param>
    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<RequestLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;

        // Pre-process MIME types into ReadOnlyMemory<char> for zero-allocation span comparisons
        _allowedMimeTypes = [.. _options.RequestBodyMimeTypes.Select(static t => t.AsMemory())];
    }

    /// <summary>
    /// Invokes the middleware to process an HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // start timing
        var timestamp = Stopwatch.GetTimestamp();

        // capture request details
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path.Value;

        // capture request body if configured
        var requestBody = await ReadBody(context.Request).ConfigureAwait(false);

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            // capture response details
            var statusCode = context.Response.StatusCode;

            // calculate elapsed time
            var elapsed = Stopwatch.GetElapsedTime(timestamp);

            // log the request
            if (string.IsNullOrEmpty(requestBody))
                LogRequestBasic(_logger, _options.LogLevel, requestMethod, requestPath, statusCode, elapsed.TotalMilliseconds);
            else
                LogRequestBody(_logger, _options.LogLevel, requestMethod, requestPath, statusCode, elapsed.TotalMilliseconds, requestBody);
        }
    }

    /// <summary>
    /// Determines whether the request body should be logged based on configuration settings.
    /// </summary>
    /// <param name="request">The HTTP request to evaluate.</param>
    /// <returns><c>true</c> if the request body should be logged; otherwise, <c>false</c>.</returns>
    private bool ShouldLogBody(HttpRequest request)
    {
        if (!_options.IncludeRequestBody)
            return false;

        if (request.ContentLength is null or 0)
            return false;

        if (request.ContentLength > _options.RequestBodyMaxSize)
            return false;

        if (string.IsNullOrEmpty(request.ContentType))
            return false;

        // Extract media type without allocating new strings
        var contentType = request.ContentType.AsSpan();
        var semicolonIndex = contentType.IndexOf(';');
        if (semicolonIndex >= 0)
            contentType = contentType[..semicolonIndex];

        contentType = contentType.Trim();

        // Check against pre-processed MIME types with zero allocation
        foreach (var allowedType in _allowedMimeTypes)
        {
            if (contentType.Equals(allowedType.Span, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reads the request body as a string for logging purposes.
    /// </summary>
    /// <param name="request">The HTTP request containing the body to read.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the request body as a string,
    /// or <c>null</c> if the body should not be logged or an error occurs.
    /// </returns>
    /// <remarks>
    /// This method enables buffering on the request to allow multiple reads of the body stream.
    /// Any errors during reading are silently ignored and return <c>null</c>.
    /// </remarks>
    private async ValueTask<string?> ReadBody(HttpRequest request)
    {
        if (!_options.IncludeRequestBody || !ShouldLogBody(request))
            return null;

        try
        {
            // enable buffering to allow multiple reads
            request.EnableBuffering();

            // read the body stream directly
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: DefaultBufferSize,
                leaveOpen: true);

            var bodyAsText = await reader
                .ReadToEndAsync(request.HttpContext.RequestAborted)
                .ConfigureAwait(false);

            // reset the request body stream position for further processing
            request.Body.Position = 0;

            return bodyAsText;
        }
        catch
        {
            // ignore any errors reading the body for logging
            return null;
        }
    }

    [LoggerMessage(EventId = 1, Message = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms")]
    private static partial void LogRequestBasic(ILogger logger, LogLevel logLevel, string? requestMethod, string? requestPath, int statusCode, double elapsed);

    [LoggerMessage(EventId = 2, Message = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms;\n{RequestBody}")]
    private static partial void LogRequestBody(ILogger logger, LogLevel logLevel, string? requestMethod, string? requestPath, int statusCode, double elapsed, string? requestBody);
}
