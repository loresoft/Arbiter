using System.Security.Claims;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Provides methods for resolving the current principal.
/// </summary>
public interface IPrincipalResolver
{
    /// <summary>
    /// Resolves the current principal.
    /// </summary>
    /// <returns>The resolved <see cref="ClaimsPrincipal"/> instance, or <c>null</c> if no principal is available.</returns>
    ClaimsPrincipal? GetCurrent();
}
