using Microsoft.Extensions.Logging;

namespace Arbiter.OpenTelemetry.Server;

/// <summary>
/// Options for configuring OpenTelemetry server instrumentation.
/// </summary>
public sealed class OpenTelemetryServerOptions
{
    /// <summary>
    /// Gets or sets the optional build version to associate with the application.
    /// </summary>
    public string? BuildVersion { get; set; }

    /// <summary>
    /// Gets or sets the minimum log level for application logging.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Gets the category log level filters for application logging.
    /// </summary>
    public IDictionary<string, LogLevel> LoggingFilters { get; } = new Dictionary<string, LogLevel>(StringComparer.Ordinal)
    {
        ["Microsoft"] = LogLevel.Information,
        ["Microsoft.AspNetCore"] = LogLevel.Warning,
        ["Microsoft.Hosting.Lifetime"] = LogLevel.Information,
    };

    /// <summary>
    /// Gets the list of request path segments to exclude from ASP.NET Core tracing.
    /// </summary>
    public IList<string> FilteredSegments { get; } = [];


    /// <summary>
    /// Sets the optional build version to associate with the application.
    /// </summary>
    /// <param name="buildVersion">The build version, or <see langword="null" /> to clear it.</param>
    /// <returns>The current <see cref="OpenTelemetryServerOptions" /> instance for chaining.</returns>
    public OpenTelemetryServerOptions WithBuildVersion(string? buildVersion)
    {
        BuildVersion = buildVersion;
        return this;
    }

    /// <summary>
    /// Sets the minimum log level for application logging.
    /// </summary>
    /// <param name="minimumLogLevel">The minimum log level.</param>
    /// <returns>The current <see cref="OpenTelemetryServerOptions" /> instance for chaining.</returns>
    public OpenTelemetryServerOptions WithMinimumLogLevel(LogLevel minimumLogLevel)
    {
        MinimumLogLevel = minimumLogLevel;
        return this;
    }

    /// <summary>
    /// Adds or replaces a category log level filter for application logging.
    /// </summary>
    /// <param name="category">The logging category.</param>
    /// <param name="logLevel">The minimum log level for the category.</param>
    /// <returns>The current <see cref="OpenTelemetryServerOptions" /> instance for chaining.</returns>
    public OpenTelemetryServerOptions AddLoggingFilter(string category, LogLevel logLevel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        LoggingFilters[category] = logLevel;
        return this;
    }

    /// <summary>
    /// Adds a request path segment to exclude from ASP.NET Core tracing.
    /// </summary>
    /// <param name="segment">The request path segment to exclude.</param>
    /// <returns>The current <see cref="OpenTelemetryServerOptions" /> instance for chaining.</returns>
    public OpenTelemetryServerOptions AddFilteredSegment(string segment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        FilteredSegments.Add(segment);
        return this;
    }
}
