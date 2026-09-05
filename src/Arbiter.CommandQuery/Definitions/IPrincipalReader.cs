using System.Security.Claims;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Provides methods for reading claims from a <see cref="ClaimsPrincipal"/>.
/// </summary>
/// <remarks>
/// This interface is designed to extract specific claims such as identifier, name, email, object ID, tenant ID,
/// and display name from the provided <see cref="ClaimsPrincipal"/> instance.
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
    /// <remarks>
    /// Reads the value of <c>ClaimsPrincipal.Identity.Name</c>.
    /// </remarks>
    string? GetIdentifier(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets the name claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The name claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Reads the first matching claim in order: <see cref="ClaimNames.NameClaim"/>, <see cref="ClaimTypes.Name"/>,
    /// <see cref="ClaimNames.Subject"/>; falling back to <c>ClaimsPrincipal.Identity.Name</c>.
    /// </remarks>
    string? GetName(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets the email claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The email claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Reads the first matching claim in order: <see cref="ClaimTypes.Email"/>, <see cref="ClaimNames.EmailClaim"/>,
    /// <see cref="ClaimNames.EmailsClaim"/>.
    /// </remarks>
    string? GetEmail(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets the object identifier claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The object identifier claim as a <see cref="Guid"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Reads the first matching claim in order: <see cref="ClaimNames.IdentifierClaim"/>,
    /// <see cref="ClaimNames.ObjectIdentifier"/>, <see cref="ClaimTypes.NameIdentifier"/>.
    /// </remarks>
    Guid? GetObjectId(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets the user identifier claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The user identifier claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Reads the <see cref="ClaimNames.UserId"/> claim.
    /// </remarks>
    string? GetUserId(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets the tenant identifier claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The tenant identifier claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Reads the <see cref="ClaimNames.TenantId"/> claim.
    /// </remarks>
    string? GetTenantId(ClaimsPrincipal? principal);

    /// <summary>
    /// Gets the display name claim from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The display name claim as a <see cref="string"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Reads the first matching claim in order: <see cref="ClaimNames.DisplayName"/>, <see cref="ClaimNames.NameClaim"/>,
    /// <see cref="ClaimTypes.Name"/>, <see cref="ClaimNames.PreferredUserName"/>, <see cref="ClaimNames.Subject"/>;
    /// falling back to <c>ClaimsPrincipal.Identity.Name</c>.
    /// </remarks>
    string? GetDisplayName(ClaimsPrincipal? principal);
}
