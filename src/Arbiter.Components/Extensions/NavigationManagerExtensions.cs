using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Extension methods for <see cref="NavigationManager"/>.
/// </summary>
/// <remarks>
/// Some members are conditionally compiled for target frameworks earlier than .NET 10, where the equivalent
/// functionality is provided by the framework. Code that calls them therefore compiles unchanged on every
/// supported target.
/// </remarks>
public static class NavigationManagerExtensions
{
#if !NET10_0_OR_GREATER
    /// <summary>
    /// The query string parameter name used by <see cref="NotFound(NavigationManager)"/> to signal that the
    /// requested resource was not found.
    /// </summary>
    public const string NotFoundParameterName = "NotFound";

    /// <summary>
    /// Indicates that the requested resource was not found.
    /// </summary>
    /// <param name="navigationManager">The navigation manager used to perform the navigation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="navigationManager"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is a substitute for the <c>NotFound</c> method introduced in .NET 10, which renders the not found
    /// content in place without changing the address. Because that behavior cannot be reproduced on earlier
    /// frameworks, this implementation navigates to the application root with the
    /// <see cref="NotFoundParameterName"/> query string value instead, replacing the current history entry so the
    /// missing resource is not restored by the browser back button.
    /// </para>
    /// <para>
    /// The hosting application is responsible for recognizing that query string value and rendering a suitable
    /// not found page. Note that the address changes and the current component is replaced, which differs from
    /// the .NET 10 behavior.
    /// </para>
    /// </remarks>
    public static void NotFound(this NavigationManager navigationManager)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);

        var uri = $"{navigationManager.BaseUri}?{NotFoundParameterName}=true";
        navigationManager.NavigateTo(uri, forceLoad: false, replace: true);
    }
#endif

    /// <summary>
    /// Navigates to the specified <paramref name="url"/> only when it resolves to a location within the
    /// application; otherwise navigates to <see cref="NavigationManager.BaseUri"/>.
    /// </summary>
    /// <param name="navigationManager">The navigation manager used to perform the navigation.</param>
    /// <param name="url">The relative or absolute URL to navigate to.</param>
    /// <param name="forceLoad">
    /// <see langword="true"/> to bypass client-side routing and force the browser to load the new page from the
    /// server; otherwise <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="navigationManager"/> or <paramref name="url"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This guards against open redirect attacks by rejecting any URL whose scheme, host, port or path falls
    /// outside <see cref="NavigationManager.BaseUri"/>. Relative URLs are resolved against the base address, and
    /// URLs that cannot be parsed are treated as external.
    /// </para>
    /// <para>
    /// Use this when the destination originates from untrusted input, such as a <c>returnUrl</c> query string value.
    /// </para>
    /// </remarks>
    public static void NavigateLocalOnly(
        this NavigationManager navigationManager,
        [StringSyntax(StringSyntaxAttribute.Uri)] string url,
        bool forceLoad = false)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);
        ArgumentNullException.ThrowIfNull(url);

        var baseUri = new Uri(navigationManager.BaseUri);

        // resolve relative to the base address; malformed input is treated as external
        if (!Uri.TryCreate(baseUri, url, out var targetUri) || !baseUri.IsBaseOf(targetUri))
        {
            navigationManager.NavigateTo(navigationManager.BaseUri, forceLoad);
            return;
        }

        navigationManager.NavigateTo(targetUri.ToString(), forceLoad);
    }
}
