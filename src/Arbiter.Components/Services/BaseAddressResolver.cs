using Arbiter.CommandQuery.Definitions;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Arbiter.Components.Services;

/// <summary>
/// An <see cref="IBaseAddressResolver"/> that resolves the base address from the Blazor
/// <see cref="NavigationManager"/> when one is available, falling back to configuration.
/// </summary>
/// <remarks>
/// A hosted Blazor application knows its own base address at runtime, so the navigation manager is preferred and
/// no configuration is required. Configuration is used when there is no navigation manager, for example in a
/// background service or a test host.
/// </remarks>
public class BaseAddressResolver : IBaseAddressResolver
{
    /// <summary>
    /// The default configuration key the base address is read from when the navigation manager is not available.
    /// </summary>
    public const string BaseAddressKey = "BaseAddress";

    private readonly IConfiguration _configuration;
    private readonly NavigationManager? _navigationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAddressResolver"/> class.
    /// </summary>
    /// <param name="configuration">The configuration the base address is read from as a fallback</param>
    /// <param name="navigationManager">
    /// The navigation manager the base address is preferred from, or <see langword="null"/> when the application is not
    /// rendering a component
    /// </param>
    /// <exception cref="ArgumentNullException">When <paramref name="configuration"/> is <see langword="null"/></exception>
    public BaseAddressResolver(
        IConfiguration configuration,
        NavigationManager? navigationManager = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuration = configuration;
        _navigationManager = navigationManager;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <paramref name="configurationKey"/> is only used for the configuration fallback; it is ignored when a
    /// <see cref="NavigationManager"/> is available. A <see langword="null"/> key falls back to
    /// <see cref="BaseAddressKey"/>. The configuration fallback is also used when the navigation manager has not
    /// been initialized yet, which happens outside of a Blazor rendering context.
    /// </remarks>
    public string? GetBaseAddress(string? configurationKey = BaseAddressKey)
    {
        var baseUri = ReadBaseUri();
        if (!string.IsNullOrEmpty(baseUri))
            return baseUri;

        configurationKey ??= BaseAddressKey;

        // fallback to configuration
        return _configuration.GetValue<string>(configurationKey);
    }

    /// <summary>
    /// Reads the base address from the navigation manager when it is available and initialized.
    /// </summary>
    /// <returns>The base address, or <see langword="null"/> when the navigation manager cannot supply one</returns>
    private string? ReadBaseUri()
    {
        if (_navigationManager == null)
            return null;

        try
        {
            return _navigationManager.BaseUri;
        }
        catch (InvalidOperationException)
        {
            // the navigation manager has not been initialized; the caller falls back to configuration
            return null;
        }
    }
}
