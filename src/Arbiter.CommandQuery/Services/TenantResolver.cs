using System.Security.Principal;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// Resolves the tenant identifier from an <see cref="IPrincipal"/> instance.
/// </summary>
/// <typeparam name="TKey">The type of the tenant identifier.</typeparam>
/// <remarks>
/// This class uses an <see cref="IPrincipalReader"/> to extract the tenant identifier claim from the provided
/// <see cref="IPrincipal"/> and converts it to the specified type <typeparamref name="TKey"/>.
/// </remarks>
public class TenantResolver<TKey>(IPrincipalReader principalReader) : ITenantResolver<TKey>
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
    /// <exception cref="InvalidCastException">
    /// Thrown if the tenant identifier cannot be converted to the specified type <typeparamref name="TKey"/>.
    /// </exception>
    public ValueTask<TKey?> GetTenantId(IPrincipal? principal)
    {
        if (principal is null)
            return ValueTask.FromResult<TKey?>(default);

        var tenantId = _principalReader.GetTenantId(principal);
        if (tenantId is null)
            return ValueTask.FromResult<TKey?>(default);

        var result = Convert.ChangeType(tenantId, typeof(TKey));

        if (result is TKey key)
            return ValueTask.FromResult<TKey?>(key);

        return ValueTask.FromResult<TKey?>(default);
    }
}
