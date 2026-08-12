namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Represents a configuration entry with its resolved value and source provider.
/// </summary>
public sealed class ConfigurationValue
{
    /// <summary>
    /// Gets or sets the configuration key path.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the resolved configuration value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the configuration provider that supplied the value.
    /// </summary>
    public string? Provider { get; set; }
}
