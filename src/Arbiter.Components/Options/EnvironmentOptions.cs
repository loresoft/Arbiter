namespace Arbiter.Components.Options;

/// <summary>
/// Options describing the hosting environment the Arbiter components are running in.
/// </summary>
/// <remarks>
/// <para>
/// These options mirror the values exposed by the host environment (such as <c>IHostEnvironment</c>) for
/// scenarios where the host is not available, for example Blazor WebAssembly components rendered on the client.
/// </para>
/// <para>
/// Values are bound from <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> and can be overridden
/// by the delegate passed to <see cref="ComponentServiceExtensions.AddArbiterComponents"/>.
/// </para>
/// </remarks>
public class EnvironmentOptions
{
    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the name of the hosting environment, such as <c>Development</c>, <c>Staging</c> or <c>Production</c>.
    /// </summary>
    /// <value>Defaults to <c>Development</c>.</value>
    public string EnvironmentName { get; set; } = "Development";

    /// <summary>
    /// Gets or sets the base address of the application.
    /// </summary>
    public string? BaseAddress { get; set; }
}
