namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// Provides a mechanism to resolve the base address for service endpoints or API calls.
/// </summary>
public interface IBaseAddressResolver
{
    /// <summary>
    /// Gets the base address as a string, or <see langword="null"/> if it cannot be resolved.
    /// </summary>
    /// <param name="configurationKey">
    /// The configuration key used to look up the base address. Defaults to <c>"BaseAddress"</c>.
    /// </param>
    /// <returns>
    /// The base address string if available; otherwise, <see langword="null"/>.
    /// </returns>
    string? GetBaseAddress(string? configurationKey = "BaseAddress");
}
