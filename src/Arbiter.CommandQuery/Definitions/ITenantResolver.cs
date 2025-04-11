using System.Security.Principal;

namespace Arbiter.CommandQuery.Definitions;

/// <summary>
///   <see langword="interface"/> for extracting tenant key from <see cref="IPrincipal" /> instance
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public interface ITenantResolver<TKey>
{
    /// <summary>
    /// Gets the tenant identifier from the specified <paramref name="principal"/>.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns>The tenant identifier for the specified principal.</returns>
    ValueTask<TKey?> GetTenantId(IPrincipal? principal);
}
