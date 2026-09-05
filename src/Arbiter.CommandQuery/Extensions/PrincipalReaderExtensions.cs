using System.Globalization;
using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides strongly typed claim reading extensions for <see cref="IPrincipalReader"/>.
/// </summary>
public static class PrincipalReaderExtensions
{
    /// <summary>
    /// Gets the user identifier claim from the specified <paramref name="principal"/> as
    /// the key type <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the user identifier, such as <see cref="int"/>, <see cref="long"/>, <see cref="Guid"/> or <see cref="string"/>.</typeparam>
    /// <param name="principalReader">The principal reader used to read the claim.</param>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The user identifier claim as <typeparamref name="TKey"/> if found and convertible;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principalReader"/> is <see langword="null"/>.</exception>
    public static TKey? GetUserId<TKey>(this IPrincipalReader principalReader, ClaimsPrincipal? principal)
        where TKey : IParsable<TKey>
    {
        ArgumentNullException.ThrowIfNull(principalReader);

        var userId = principalReader.GetUserId(principal);

        return TKey.TryParse(userId, CultureInfo.InvariantCulture, out var key) ? key : default;
    }

    /// <summary>
    /// Gets the tenant identifier claim from the specified <paramref name="principal"/> as
    /// the key type <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the tenant identifier, such as <see cref="int"/>, <see cref="long"/>, <see cref="Guid"/> or <see cref="string"/>.</typeparam>
    /// <param name="principalReader">The principal reader used to read the claim.</param>
    /// <param name="principal">The principal to read the claim from.</param>
    /// <returns>
    /// The tenant identifier claim as <typeparamref name="TKey"/> if found and convertible;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principalReader"/> is <see langword="null"/>.</exception>
    public static TKey? GetTenantId<TKey>(this IPrincipalReader principalReader, ClaimsPrincipal? principal)
        where TKey : IParsable<TKey>
    {
        ArgumentNullException.ThrowIfNull(principalReader);

        var tenantId = principalReader.GetTenantId(principal);

        return TKey.TryParse(tenantId, CultureInfo.InvariantCulture, out var key) ? key : default;
    }
}
