using System.Diagnostics;
using System.Text.Json;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Arbiter.CommandQuery.Mvc;

/// <summary>
/// Middleware for handling exceptions and returning JSON-formatted error responses.
/// </summary>
/// <remarks>
/// This middleware captures unhandled exceptions during the request pipeline execution and formats the error response
/// as a JSON object, including details such as the error message, stack trace (in development), and trace ID.
/// </remarks>
public class JsonExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly Func<object, Task> _clearCacheHeadersDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="loggerFactory">The factory used to create a logger instance.</param>
    /// <param name="hostingEnvironment">The hosting environment to determine if the application is in development mode.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <c>null</c>.</exception>
    public JsonExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IWebHostEnvironment hostingEnvironment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _environment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _logger = loggerFactory?.CreateLogger<ExceptionHandlerMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        _clearCacheHeadersDelegate = ClearCacheHeaders;
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request and catch unhandled exceptions.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            context.Response.StatusCode = 499; // Client Closed Request
            context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);
            return;
        }
        catch (Exception middlewareError)
        {
            _logger.LogError(middlewareError, "An unhandled exception has occurred while executing the request.");

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the error page middleware will not be executed.");
                throw;
            }

            try
            {
                if (context.Response.Body.CanSeek)
                    context.Response.Body.SetLength(0L);

                context.Response.StatusCode = 500;
                context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);

                await WriteContent(context, middlewareError, _environment.IsDevelopment()).ConfigureAwait(false);
                return;
            }
            catch (Exception handlerError)
            {
                _logger.LogError(handlerError, "An exception was thrown attempting to execute the error handler.");
            }

            throw; // Re-throw the original exception if it couldn't be handled
        }
    }

    private static async Task WriteContent(HttpContext context, Exception exception, bool includeDetails)
    {
        if (context is null)
            return;

        context.Response.ContentType = "application/problem+json";

        var title = includeDetails ? "An error occurred: " + exception.Message : "An error occurred";
        var details = includeDetails ? exception.ToString() : null;

        var problem = new ProblemDetails
        {
            Status = 500,
            Title = title,
            Detail = details
        };

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        if (traceId != null)
            problem.Extensions["traceId"] = traceId;

        switch (exception)
        {
            case DomainException mediatorException:
                context.Response.StatusCode = mediatorException.StatusCode;
                break;
        }

        problem.Status = context.Response.StatusCode;

        var options = ResolveSerializerOptions(context);
        await JsonSerializer.SerializeAsync(context.Response.Body, problem, options).ConfigureAwait(false);
    }

    private Task ClearCacheHeaders(object state)
    {
        if (state is not HttpResponse response)
            return Task.CompletedTask;

        response.Headers[HeaderNames.CacheControl] = "no-cache";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "-1";
        response.Headers.Remove(HeaderNames.ETag);

        return Task.CompletedTask;
    }

    private static JsonSerializerOptions ResolveSerializerOptions(HttpContext httpContext)
    {
        return httpContext.RequestServices?.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions ?? new JsonSerializerOptions();
    }
}
