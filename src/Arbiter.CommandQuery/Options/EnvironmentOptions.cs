namespace Arbiter.CommandQuery.Options;

/// <summary>
/// Options describing the hosting environment the Arbiter components are running in.
/// </summary>
/// <remarks>
/// <para>
/// These options mirror the values exposed by the host environment (such as <c>IHostEnvironment</c>) for
/// scenarios where the host is not available, for example Blazor WebAssembly components rendered on the client.
/// </para>
/// <para>
/// The options are bound from the root of <c>IConfiguration</c>, so the values are set as top level keys in <c>appsettings.json</c>.
/// </para>
/// </remarks>
/// <example>
/// <para>Set the options in <c>appsettings.json</c> (for Blazor WebAssembly, use <c>wwwroot/appsettings.json</c>):</para>
/// <code language="json">
/// {
///   "ApplicationName": "Tracker",
///   "CompanyName": "LoreSoft",
///   "ProjectName": "Arbiter",
///   "EnvironmentName": "Production",
///   "BaseAddress": "https://tracker.example.com/",
///   "DefaultCacheTime": "00:10:00"
/// }
/// </code>
/// <para>Values can be overridden in code, which takes precedence over configuration:</para>
/// <code language="csharp">
/// builder.Services.AddEnvironmentOptions(options => options.EnvironmentName = "Staging");
/// </code>
/// </example>
public class EnvironmentOptions
{
    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the name of the company that owns the application.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the name of the project the application belongs to.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the name of the hosting environment, such as <c>Development</c>, <c>Staging</c> or <c>Production</c>.
    /// </summary>
    /// <value>Defaults to <c>Development</c>.</value>
    public string EnvironmentName { get; set; } = "Development";

    /// <summary>
    /// Gets or sets the base address of the application.
    /// </summary>
    public string? BaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the default amount of time cached items are retained.
    /// </summary>
    /// <value>Defaults to 5 minutes.</value>
    public TimeSpan DefaultCacheTime { get; set; } = TimeSpan.FromMinutes(5);
}
