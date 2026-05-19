using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Configuration options for request logging middleware.
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// The default scope key for user name-based scoping.
    /// </summary>
    public const string DefaultUserNameScopeKey = "UserName";

    /// <summary>
    /// The default scope key used to store and retrieve user identifier information.
    /// </summary>
    public const string DefaultUserIdentifierScopeKey = "UserId";

    /// <summary>
    /// Default activity tag name for the end user name.
    /// </summary>
    public const string DefaultUserNameActivityTagName = "enduser.name";

    /// <summary>
    /// Default activity tag name for the end user identifier.
    /// </summary>
    public const string DefaultUserIdentifierActivityTagName = "enduser.id";


    /// <summary>
    /// Gets or sets a value indicating whether to include the request body in the logs.
    /// </summary>
    /// <value>
    /// <c>true</c> to include the request body; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Enabling request body logging may have a performance impact, especially for large payloads
    /// or high-traffic applications. Use <see cref="RequestBodyMaxSize"/> to limit the size of
    /// logged request bodies and <see cref="RequestBodyMimeTypes"/> to filter which content types are logged.
    /// </para>
    /// <para>
    /// Request bodies can contain sensitive data (for example credentials, tokens, or personal data),
    /// so enable this setting only when appropriate for your security and compliance requirements.
    /// </para>
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

    /// <summary>
    /// Gets or sets the list of request paths to ignore for logging.
    /// </summary>
    /// <remarks>
    /// Supports glob patterns for flexible path matching (e.g., "/health", "/api/*/status", "**.svg").
    /// </remarks>
    public IList<string>? IgnorePaths { get; set; }


    /// <summary>
    /// Gets or sets the user name claim configuration for logging scopes and activity tags.
    /// </summary>
    public ClaimLoggingType UserNameClaim { get; set; } = new(
        [ClaimTypes.Name, ClaimTypes.Upn, ClaimTypes.Email, ClaimTypes.NameIdentifier],
        DefaultUserNameScopeKey,
        DefaultUserNameActivityTagName);

    /// <summary>
    /// Gets or sets the user identifier claim configuration for logging scopes and activity tags.
    /// </summary>
    public ClaimLoggingType UserIdentifierClaim { get; set; } = new(
        [ClaimTypes.NameIdentifier, "sub", "oid"],
        DefaultUserIdentifierScopeKey,
        DefaultUserIdentifierActivityTagName);

    /// <summary>
    /// Gets the list of additional claims to include in logging scopes and activity tags.
    /// </summary>
    public IList<ClaimLoggingType> AdditionalClaims { get; } = [];


    /// <summary>
    /// Adds a request path to ignore for logging.
    /// </summary>
    /// <param name="globPattern">The path or glob pattern to ignore (e.g., "/health", "/api/*/status", "**.svg").</param>
    /// <returns>The current <see cref="RequestLoggingOptions"/> instance for method chaining.</returns>
    public RequestLoggingOptions IgnorePath(string globPattern)
    {
        IgnorePaths ??= [];
        IgnorePaths.Add(globPattern);

        return this;
    }


    /// <summary>
    /// Configures the user name claim to include in logging scopes and activity tags.
    /// </summary>
    /// <param name="claimTypes">The ordered claim types to read from the current user.</param>
    /// <param name="scopeKey">The logging scope key to write.</param>
    /// <param name="activityTagName">The optional activity tag name to write.</param>
    /// <returns>The current <see cref="RequestLoggingOptions"/> instance for method chaining.</returns>
    public RequestLoggingOptions ConfigureUserName(
        IEnumerable<string> claimTypes,
        string scopeKey = DefaultUserNameScopeKey,
        string? activityTagName = DefaultUserNameActivityTagName)
    {
        UserNameClaim = new ClaimLoggingType(claimTypes, scopeKey, activityTagName);

        return this;
    }

    /// <summary>
    /// Configures the user identifier claim to include in logging scopes and activity tags.
    /// </summary>
    /// <param name="claimTypes">The ordered claim types to read from the current user.</param>
    /// <param name="scopeKey">The logging scope key to write.</param>
    /// <param name="activityTagName">The optional activity tag name to write.</param>
    /// <returns>The current <see cref="RequestLoggingOptions"/> instance for method chaining.</returns>
    public RequestLoggingOptions ConfigureUserIdentifier(
        IEnumerable<string> claimTypes,
        string scopeKey = DefaultUserIdentifierScopeKey,
        string? activityTagName = DefaultUserIdentifierActivityTagName)
    {
        UserIdentifierClaim = new ClaimLoggingType(claimTypes, scopeKey, activityTagName);

        return this;
    }

    /// <summary>
    /// Adds a claim to include in logging scopes and activity tags.
    /// </summary>
    /// <param name="claimType">The claim type to read from the current user.</param>
    /// <param name="scopeKey">The logging scope key to write.</param>
    /// <param name="activityTagName">The optional activity tag name to write.</param>
    /// <returns>The current <see cref="RequestLoggingOptions"/> instance for method chaining.</returns>
    public RequestLoggingOptions IncludeClaim(
        string claimType,
        string scopeKey,
        string? activityTagName = null)
    {
        AdditionalClaims.Add(new ClaimLoggingType(claimType, scopeKey, activityTagName));

        return this;
    }

    /// <summary>
    /// Adds a claim to include in logging scopes and activity tags.
    /// </summary>
    /// <param name="claimTypes">The ordered claim types to read from the current user.</param>
    /// <param name="scopeKey">The logging scope key to write.</param>
    /// <param name="activityTagName">The optional activity tag name to write.</param>
    /// <returns>The current <see cref="RequestLoggingOptions"/> instance for method chaining.</returns>
    public RequestLoggingOptions IncludeClaim(
        IEnumerable<string> claimTypes,
        string scopeKey,
        string? activityTagName = null)
    {
        AdditionalClaims.Add(new ClaimLoggingType(claimTypes, scopeKey, activityTagName));

        return this;
    }
}
