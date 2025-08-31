using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace Arbiter.CommandQuery.Mvc;

/// <summary>
/// Default implementation of <see cref="IBaseAddressResolver"/>.
/// Resolves the base address for service endpoints or API calls using <see cref="NavigationManager"/>, <see cref="IHttpContextAccessor"/>, or <see cref="IConfiguration"/>.
/// </summary>
public sealed class BaseAddressResolver : IBaseAddressResolver
{
    /// <summary>
    /// The default configuration key for the base address.
    /// </summary>
    public const string BaseAddressKey = "BaseAddress";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly NavigationManager? _navigationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAddressResolver"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="configuration">Application configuration for retrieving the base address.</param>
    /// <param name="navigationManager">Optional Blazor <see cref="NavigationManager"/> for resolving the base URI in WebAssembly scenarios.</param>
    public BaseAddressResolver(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        NavigationManager? navigationManager = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Gets the base address for service endpoints or API calls.
    /// Resolution order: <see cref="NavigationManager"/> (if available), HTTP context, then configuration.
    /// </summary>
    /// <param name="configurationKey">
    /// The configuration key used to look up the base address. Defaults to <c>"BaseAddress"</c>.
    /// </param>
    /// <returns>
    /// The resolved base address string if available; otherwise, <see langword="null"/>.
    /// </returns>
    public string? GetBaseAddress(string? configurationKey = "BaseAddress")
    {
        // Use NavigationManager if available
        if (_navigationManager?.BaseUri != null)
            return _navigationManager.BaseUri;

        // Use HttpContext if available
        var httpAddress = GetHttpContextAddress();
        if (httpAddress.HasValue())
            return httpAddress;

        // fallback to configuration
        return _configuration[configurationKey ?? BaseAddressKey];
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
}
