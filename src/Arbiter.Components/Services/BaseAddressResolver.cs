using Arbiter.CommandQuery.Definitions;
using Arbiter.Components.Options;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Arbiter.Components.Services;

/// <summary>
/// An <see cref="IBaseAddressResolver"/> that resolves the base address from the Blazor
/// <see cref="NavigationManager"/> when one is available, falling back to <see cref="EnvironmentOptions"/>.
/// </summary>
/// <remarks>
/// A hosted Blazor application knows its own base address at runtime, so the navigation manager is preferred and
/// no configuration is required. <see cref="EnvironmentOptions.BaseAddress"/> is used when there is no navigation
/// manager, for example in a background service or a test host.
/// </remarks>
public class BaseAddressResolver : IBaseAddressResolver
{
    /// <summary>
    /// The default configuration key the base address is read from when the navigation manager is not available.
    /// </summary>
    public const string BaseAddressKey = "BaseAddress";

    private readonly IConfiguration _configuration;
    private readonly IOptions<EnvironmentOptions> _environmentOptions;
    private readonly NavigationManager? _navigationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAddressResolver"/> class.
    /// </summary>
    /// <param name="configuration">The configuration a custom base address key is read from as a fallback</param>
    /// <param name="environmentOptions">The environment options the default base address is read from as a fallback</param>
    /// <param name="navigationManager">
    /// The navigation manager the base address is preferred from, or <see langword="null"/> when the application is not
    /// rendering a component
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// When <paramref name="configuration"/> or <paramref name="environmentOptions"/> is <see langword="null"/>
    /// </exception>
    public BaseAddressResolver(
        IConfiguration configuration,
        IOptions<EnvironmentOptions> environmentOptions,
        NavigationManager? navigationManager = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environmentOptions);

        _configuration = configuration;
        _environmentOptions = environmentOptions;
        _navigationManager = navigationManager;
    }

    /// <inheritdoc />
    /// <remarks>
    /// A <see cref="NavigationManager"/> is preferred when available. Otherwise, a <see langword="null"/> key or
    /// <see cref="BaseAddressKey"/> resolves <see cref="EnvironmentOptions.BaseAddress"/>, which is bound from
    /// configuration and can be overridden in code. Any other key is read directly from configuration. The fallback
    /// is also used when the navigation manager has not been initialized yet, which happens outside of a Blazor
    /// rendering context.
    /// </remarks>
    public string? GetBaseAddress(string? configurationKey = BaseAddressKey)
    {
        var baseUri = ReadBaseUri();
        if (!string.IsNullOrEmpty(baseUri))
            return baseUri;

        if (configurationKey == null || string.Equals(configurationKey, BaseAddressKey, StringComparison.OrdinalIgnoreCase))
            return _environmentOptions.Value.BaseAddress;

        // custom keys are not part of the environment options
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
