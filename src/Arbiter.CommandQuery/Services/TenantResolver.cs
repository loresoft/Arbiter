using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// Resolves the tenant identifier from a <see cref="ClaimsPrincipal"/> instance.
/// </summary>
/// <typeparam name="TKey">The type of the tenant identifier.</typeparam>
/// <remarks>
/// This class uses an <see cref="IPrincipalReader"/> to extract the tenant identifier claim from the provided
/// <see cref="ClaimsPrincipal"/> and converts it to the specified type <typeparamref name="TKey"/>.
/// </remarks>
public class TenantResolver<TKey>(IPrincipalReader principalReader) : ITenantResolver<TKey>
    where TKey : IParsable<TKey>
{
    private readonly IPrincipalReader _principalReader = principalReader
        ?? throw new ArgumentNullException(nameof(principalReader));

    /// <summary>
    /// Gets the tenant identifier from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal to extract the tenant identifier from.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the tenant identifier as <typeparamref name="TKey"/> if found;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public ValueTask<TKey?> GetTenantId(ClaimsPrincipal? principal)
    {
        if (principal is null)
            return ValueTask.FromResult<TKey?>(default);

        var tenantId = _principalReader.GetTenantId<TKey>(principal);

        return ValueTask.FromResult(tenantId);
    }
}
