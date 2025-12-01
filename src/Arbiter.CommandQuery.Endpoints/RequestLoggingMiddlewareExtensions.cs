using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Extension methods for configuring request logging middleware in the application pipeline.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds the request logging middleware to the application's request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <param name="configureOptions">An optional action to configure the <see cref="RequestLoggingOptions"/>.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This middleware logs HTTP request details including method, path, status code, and elapsed time.
    /// Optionally, it can also log request bodies based on the configured options.
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseRequestLogging(options =>
    /// {
    ///     options.LogLevel = LogLevel.Debug;
    ///     options.IncludeRequestBody = true;
    ///     options.RequestBodyMaxSize = 20 * 1024; // 20 KB
    /// });
    /// </code>
    /// </example>
    public static IApplicationBuilder UseRequestLogging(
        this IApplicationBuilder builder,
        Action<RequestLoggingOptions>? configureOptions = null)
    {
        var options = new RequestLoggingOptions();
        configureOptions?.Invoke(options);

        return builder.UseMiddleware<RequestLoggingMiddleware>(Options.Create(options));
    }
}
