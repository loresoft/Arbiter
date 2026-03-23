using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;

using Microsoft.AspNetCore.Http;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Default implementation of <see cref="IPrincipalResolver"/> that retrieves the current principal from the HTTP context.
/// </summary>
/// <remarks>
/// This implementation uses the <see cref="IHttpContextAccessor"/> to access the current
/// </remarks>
public sealed class PrincipalResolver : IPrincipalResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrincipalResolver"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    public PrincipalResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current <see cref="ClaimsPrincipal"/> from the HTTP context.
    /// </summary>
    /// <returns>The current <see cref="ClaimsPrincipal"/> if available; otherwise, <c>null</c>.</returns>
    public ClaimsPrincipal? GetCurrent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        return httpContext.User;
    }
}
