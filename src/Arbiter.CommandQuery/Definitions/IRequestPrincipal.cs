using System.Security.Claims;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Represents a request principal that provides access to the current user's claims and activation information.
/// </summary>
public interface IRequestPrincipal
{
    /// <summary>
    /// Gets the timestamp when the principal was activated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> indicating when the principal was activated.
    /// </value>
    DateTimeOffset Activated { get; }

    /// <summary>
    /// Gets the identifier of the user or system that activated the principal.
    /// </summary>
    /// <value>
    /// A string representing the activator's identifier, or <c>null</c> if not available.
    /// </value>
    string? ActivatedBy { get; }

    /// <summary>
    /// Gets the claims principal associated with the current request.
    /// </summary>
    /// <value>
    /// A <see cref="ClaimsPrincipal"/> containing the user's claims, or <c>null</c> if not authenticated.
    /// </value>
    ClaimsPrincipal? Principal { get; }

    /// <summary>
    /// Applies the specified claims principal to the current request.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to apply, or <c>null</c> to clear the principal.</param>
    void ApplyPrincipal(ClaimsPrincipal? principal);
}
