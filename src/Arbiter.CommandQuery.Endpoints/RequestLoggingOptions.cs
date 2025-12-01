using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Configuration options for request logging middleware.
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to include the request body in the logs.
    /// </summary>
    /// <value>
    /// <c>true</c> to include the request body; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// Enabling request body logging may have a performance impact, especially for large payloads
    /// or high-traffic applications. Use <see cref="RequestBodyMaxSize"/> to limit the size of
    /// logged request bodies and <see cref="RequestBodyMimeTypes"/> to filter which content types are logged.
    /// </remarks>
    public bool IncludeRequestBody { get; set; }

    /// <summary>
    /// Gets or sets the set of MIME types for which the request body should be logged.
    /// </summary>
    /// <value>
    /// A set of MIME type strings. Default includes "application/json", "application/xml", and "text/xml".
    /// </value>
    /// <remarks>
    /// The comparison is case-insensitive. Only request bodies with MIME types in this set will be logged
    /// when <see cref="IncludeRequestBody"/> is set to <c>true</c>.
    /// </remarks>
    public ISet<string> RequestBodyMimeTypes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "application/xml",
        "text/xml",
    };

    /// <summary>
    /// Gets or sets the maximum size (in bytes) of the request body to log.
    /// </summary>
    /// <value>
    /// The maximum size in bytes. Default is 10,240 bytes (10 KB).
    /// </value>
    /// <remarks>
    /// Request bodies larger than this size will not be logged.
    /// </remarks>
    public int RequestBodyMaxSize { get; set; } = 10 * 1024; // 10 KB

    /// <summary>
    /// Gets or sets the log level for request logging.
    /// </summary>
    /// <value>
    /// The log level to use. Default is <see cref="LogLevel.Information"/>.
    /// </value>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}
