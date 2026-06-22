namespace Arbiter.Functions;

/// <summary>
/// Configures how <see cref="FunctionClient"/> resolves its base address and master key.
/// </summary>
public sealed class FunctionClientOptions
{
    /// <summary>
    /// Gets or sets the configuration key used to resolve the function base address.
    /// </summary>
    public string BaseAddressConfigurationKey { get; set; } = "FunctionBaseAddress";

    /// <summary>
    /// Gets or sets the configuration key used to resolve the function master key.
    /// </summary>
    public string MasterKeyConfigurationKey { get; set; } = "FunctionMasterKey";

    /// <summary>
    /// Gets or sets the function base address value directly.
    /// </summary>
    public string? BaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the function master key value directly.
    /// </summary>
    public string? MasterKey { get; set; }
}
