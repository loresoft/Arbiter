using System.Text.Json.Serialization;

namespace Arbiter.Functions;

/// <summary>
/// Represents an Azure Function definition returned by the Functions admin API.
/// </summary>
public record FunctionDefinition
{
    /// <summary>
    /// Gets the function name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resource URI for the function definition.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; init; }

    /// <summary>
    /// Gets the invoke URL template for the function.
    /// </summary>
    [JsonPropertyName("invoke_url_template")]
    public string? InvokeUrlTemplate { get; init; }

    /// <summary>
    /// Gets the function language.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>
    /// Gets the function configuration details.
    /// </summary>
    [JsonPropertyName("config")]
    public FunctionConfiguration? Configuration { get; init; }

    /// <summary>
    /// Gets a value indicating whether the function is disabled.
    /// </summary>
    [JsonPropertyName("isDisabled")]
    public bool? IsDisabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether the function is a direct function.
    /// </summary>
    [JsonPropertyName("isDirect")]
    public bool? IsDirect { get; init; }

    /// <summary>
    /// Gets a value indicating whether the function is a proxy function.
    /// </summary>
    [JsonPropertyName("isProxy")]
    public bool? IsProxy { get; init; }
}
