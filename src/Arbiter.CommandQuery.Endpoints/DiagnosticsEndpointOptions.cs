using Microsoft.AspNetCore.Builder;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Options used to control which <see cref="DiagnosticsEndpoint"/> routes are exposed and how they are authorized.
/// </summary>
/// <remarks>
/// Diagnostics routes expose sensitive operational information. The configuration and cache clear routes are
/// disabled by default and must be explicitly enabled.
/// </remarks>
public class DiagnosticsEndpointOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the <c>config-debugger</c> route is exposed.
    /// </summary>
    /// <value><see langword="true"/> to expose the route; otherwise <see langword="false"/>. The default is <see langword="false"/>.</value>
    /// <remarks>
    /// This route returns every configuration value, including connection strings and secrets sourced from
    /// configuration providers. It is disabled by default and should be restricted to administrators.
    /// </remarks>
    public bool ConfigurationEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the <c>health-check</c> route is exposed.
    /// </summary>
    /// <value><see langword="true"/> to expose the route; otherwise <see langword="false"/>. The default is <see langword="true"/>.</value>
    public bool HealthCheckEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <c>claims-check</c> route is exposed.
    /// </summary>
    /// <value><see langword="true"/> to expose the route; otherwise <see langword="false"/>. The default is <see langword="true"/>.</value>
    /// <remarks>This route only returns the claims of the calling user.</remarks>
    public bool ClaimsCheckEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <c>cache-clear</c> route is exposed.
    /// </summary>
    /// <value><see langword="true"/> to expose the route; otherwise <see langword="false"/>. The default is <see langword="false"/>.</value>
    /// <remarks>
    /// When no cache tags are specified, this route invalidates the entire cache, which can cause a significant
    /// load spike. It is disabled by default and should be restricted to administrators.
    /// </remarks>
    public bool CacheClearEnabled { get; set; }


    /// <summary>
    /// Gets or sets the default authorization policy name applied to all exposed diagnostics routes.
    /// </summary>
    /// <value>The authorization policy name, or <see langword="null"/> to only require an authenticated user.</value>
    public string? AuthorizationPolicy { get; set; }

    /// <summary>
    /// Gets or sets the authorization policy name for the <c>config-debugger</c> route.
    /// </summary>
    /// <value>The authorization policy name, or <see langword="null"/> to use <see cref="AuthorizationPolicy"/>.</value>
    public string? ConfigurationPolicy { get; set; }

    /// <summary>
    /// Gets or sets the authorization policy name for the <c>health-check</c> route.
    /// </summary>
    /// <value>The authorization policy name, or <see langword="null"/> to use <see cref="AuthorizationPolicy"/>.</value>
    public string? HealthCheckPolicy { get; set; }

    /// <summary>
    /// Gets or sets the authorization policy name for the <c>claims-check</c> route.
    /// </summary>
    /// <value>The authorization policy name, or <see langword="null"/> to use <see cref="AuthorizationPolicy"/>.</value>
    public string? ClaimsCheckPolicy { get; set; }

    /// <summary>
    /// Gets or sets the authorization policy name for the <c>cache-clear</c> route.
    /// </summary>
    /// <value>The authorization policy name, or <see langword="null"/> to use <see cref="AuthorizationPolicy"/>.</value>
    public string? CacheClearPolicy { get; set; }


    /// <summary>
    /// Gets or sets an action used to further customize every exposed diagnostics route.
    /// </summary>
    /// <remarks>
    /// The action is invoked after the authorization policy is applied, allowing conventions such as role
    /// requirements, rate limiting, or host filtering to be added.
    /// </remarks>
    public Action<RouteHandlerBuilder>? ConfigureEndpoint { get; set; }

    /// <summary>
    /// Gets or sets an action used to further customize the <c>config-debugger</c> route.
    /// </summary>
    /// <remarks>The action is invoked after <see cref="ConfigureEndpoint"/>.</remarks>
    public Action<RouteHandlerBuilder>? ConfigureConfigurationEndpoint { get; set; }

    /// <summary>
    /// Gets or sets an action used to further customize the <c>health-check</c> route.
    /// </summary>
    /// <remarks>The action is invoked after <see cref="ConfigureEndpoint"/>.</remarks>
    public Action<RouteHandlerBuilder>? ConfigureHealthCheckEndpoint { get; set; }

    /// <summary>
    /// Gets or sets an action used to further customize the <c>claims-check</c> route.
    /// </summary>
    /// <remarks>The action is invoked after <see cref="ConfigureEndpoint"/>.</remarks>
    public Action<RouteHandlerBuilder>? ConfigureClaimsCheckEndpoint { get; set; }

    /// <summary>
    /// Gets or sets an action used to further customize the <c>cache-clear</c> route.
    /// </summary>
    /// <remarks>The action is invoked after <see cref="ConfigureEndpoint"/>.</remarks>
    public Action<RouteHandlerBuilder>? ConfigureCacheClearEndpoint { get; set; }
}
