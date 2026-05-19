using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Extension methods for configuring request logging middleware in the application pipeline.
/// </summary>
public static class RequestLoggingExtensions
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
    ///
    /// app.UseAuthentication();
    /// app.UseAuthorization();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseRequestLogging(
        this IApplicationBuilder builder,
        Action<RequestLoggingOptions>? configureOptions = null)
    {
        ConfigureOptions(builder, configureOptions);

        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }

    /// <summary>
    /// Adds the claim logging middleware to the application's request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <param name="configureOptions">An optional action to configure the <see cref="RequestLoggingOptions"/>.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This middleware resolves the current user name and user identifier from <c>HttpContext.User</c>, then adds them to
    /// the logging scope and the current <see cref="System.Diagnostics.Activity"/> (when present). Any configured additional
    /// claims are also added to both the logging scope and current activity.
    /// Register this middleware after <c>UseAuthorization</c> so endpoint policy authentication has populated <c>HttpContext.User</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseAuthentication();
    /// app.UseAuthorization();
    ///
    /// app.UseClaimLogging(options =&gt;
    /// {
    ///     options.IncludeClaim("email", "UserEmail", "enduser.email");
    /// });
    /// </code>
    /// </example>
    public static IApplicationBuilder UseClaimLogging(
        this IApplicationBuilder builder,
        Action<RequestLoggingOptions>? configureOptions = null)
    {
        ConfigureOptions(builder, configureOptions);

        return builder.UseMiddleware<ClaimLoggingMiddleware>();
    }

    private static void ConfigureOptions(
        IApplicationBuilder builder,
        Action<RequestLoggingOptions>? configureOptions)
    {
        if (configureOptions is null)
            return;

        var options = builder.ApplicationServices.GetRequiredService<IOptions<RequestLoggingOptions>>();
        configureOptions(options.Value);
    }
}
