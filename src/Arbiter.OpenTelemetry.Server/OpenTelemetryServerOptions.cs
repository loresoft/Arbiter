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
}
