using System.Text.Json.Serialization;

namespace Arbiter.Functions;

/// <summary>
/// Represents an Azure Function configuration entry returned by the Functions admin API.
/// </summary>
public record FunctionConfiguration
{
    /// <summary>
    /// Gets the function name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the fully qualified entry point method for the function.
    /// </summary>
    [JsonPropertyName("entryPoint")]
    public string? EntryPoint { get; init; }

    /// <summary>
    /// Gets the script file path for the function.
    /// </summary>
    [JsonPropertyName("scriptFile")]
    public string? ScriptFile { get; init; }

    /// <summary>
    /// Gets the function language.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>
    /// Gets the function directory path.
    /// </summary>
    [JsonPropertyName("functionDirectory")]
    public string? FunctionDirectory { get; init; }

    /// <summary>
    /// Gets the configured bindings for the function.
    /// </summary>
    [JsonPropertyName("bindings")]
    public IReadOnlyList<FunctionBinding>? Bindings { get; init; }
}
