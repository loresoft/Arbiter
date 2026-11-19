using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Options;
using Arbiter.Services.Extensions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Default implementation of <see cref="IBaseAddressResolver"/>.
/// Resolves the base address for service endpoints or API calls using <see cref="NavigationManager"/>, <see cref="IHttpContextAccessor"/>,
/// <see cref="EnvironmentOptions"/>, or <see cref="IConfiguration"/>.
/// </summary>
public sealed class BaseAddressResolver : IBaseAddressResolver
{
    /// <summary>
    /// The default configuration key for the base address.
    /// </summary>
    public const string BaseAddressKey = "BaseAddress";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IOptions<EnvironmentOptions> _environmentOptions;
    private readonly NavigationManager? _navigationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAddressResolver"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="configuration">Application configuration a custom base address key is read from as a fallback.</param>
    /// <param name="environmentOptions">The environment options the default base address is read from as a fallback.</param>
    /// <param name="navigationManager">Optional Blazor <see cref="NavigationManager"/> for resolving the base URI in WebAssembly scenarios.</param>
    /// <exception cref="ArgumentNullException">
    /// When <paramref name="httpContextAccessor"/>, <paramref name="configuration"/> or <paramref name="environmentOptions"/> is <see langword="null"/>.
    /// </exception>
    public BaseAddressResolver(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IOptions<EnvironmentOptions> environmentOptions,
        NavigationManager? navigationManager = null)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environmentOptions);

        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _environmentOptions = environmentOptions;
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Gets the base address for service endpoints or API calls.
    /// Resolution order: <see cref="NavigationManager"/> (if available), HTTP context, then <see cref="EnvironmentOptions.BaseAddress"/>
    /// for the default key or configuration for a custom key.
    /// </summary>
    /// <param name="configurationKey">
    /// The configuration key used to look up the base address. Defaults to <c>"BaseAddress"</c>, which resolves
    /// <see cref="EnvironmentOptions.BaseAddress"/>.
    /// </param>
    /// <returns>
    /// The resolved base address string if available; otherwise, <see langword="null"/>.
    /// </returns>
    public string? GetBaseAddress(string? configurationKey = BaseAddressKey)
    {
        // Use NavigationManager if available
        string? httpAddress = GetNavigationManagerAddress();
        if (httpAddress.HasValue())
            return httpAddress;

        // Use HttpContext if available
        httpAddress = GetHttpContextAddress();
        if (httpAddress.HasValue())
            return httpAddress;

        if (configurationKey == null || string.Equals(configurationKey, BaseAddressKey, StringComparison.OrdinalIgnoreCase))
            return _environmentOptions.Value.BaseAddress;

        // custom keys are not part of the environment options
        return _configuration[configurationKey];
    }

    private string? GetHttpContextAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        var request = httpContext.Request;
        if (request == null)
            return null;

        return UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase);
    }

    private string? GetNavigationManagerAddress()
    {
        try
        {
            return _navigationManager?.BaseUri;
        }
        catch (InvalidOperationException)
        {
            // NavigationManager has not been initialized
            return null;
        }
    }
}
