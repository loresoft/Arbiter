using System.Security.Claims;
using System.Security.Principal;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Provides methods for reading claims from an <see cref="IPrincipal"/> or <see cref="ClaimsPrincipal"/>.
/// </summary>
/// <remarks>
/// This interface is designed to extract specific claims such as identifier, name, email, object ID, tenant ID,
/// and display name from the provided <see cref="IPrincipal"/> or <see cref="ClaimsPrincipal"/> instance.
/// </remarks>
public interface IPrincipalReader
{
    /// <summary>
    /// Gets the identifier claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The identifier claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    string? GetIdentifier(IPrincipal? principal);

    /// <summary>
    /// Gets the name claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The name claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    string? GetName(IPrincipal? principal);

    /// <summary>
    /// Gets the email claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The email claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    string? GetEmail(IPrincipal? principal);

    /// <summary>
    /// Gets the object identifier claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The object identifier claim as a <see cref="Guid"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Guid? GetObjectId(IPrincipal? principal);

    /// <summary>
    /// Gets the tenant identifier claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The tenant identifier claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    string? GetTenantId(IPrincipal? principal);

    /// <summary>
    /// Gets the display name claim from the specified <paramref name="claimsPrincipal"/>.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to read the claim from.</param>
    /// <returns>
    /// The display name claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    string? GetDisplayName(ClaimsPrincipal? claimsPrincipal);
}
