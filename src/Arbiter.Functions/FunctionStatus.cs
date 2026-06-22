using System.Text.Json.Serialization;

namespace Arbiter.Functions;

/// <summary>
/// Represents Azure Functions host runtime status information.
/// </summary>
public record FunctionStatus
{
    /// <summary>
    /// Gets the host identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current host state.
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Functions runtime version.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>
    /// Gets detailed Functions runtime version information.
    /// </summary>
    [JsonPropertyName("versionDetails")]
    public string? VersionDetails { get; init; }

    /// <summary>
    /// Gets the platform version.
    /// </summary>
    [JsonPropertyName("platformVersion")]
    public string? PlatformVersion { get; init; }

    /// <summary>
    /// Gets the host instance identifier.
    /// </summary>
    [JsonPropertyName("instanceId")]
    public string? InstanceId { get; init; }

    /// <summary>
    /// Gets the machine name hosting the Functions process.
    /// </summary>
    [JsonPropertyName("computerName")]
    public string? ComputerName { get; init; }

    /// <summary>
    /// Gets the process uptime in seconds.
    /// </summary>
    [JsonPropertyName("processUptime")]
    public int? ProcessUptime { get; init; }

    /// <summary>
    /// Gets the function app content editing state.
    /// </summary>
    [JsonPropertyName("functionAppContentEditingState")]
    public string? FunctionEditingState { get; init; }
}
