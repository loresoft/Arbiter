using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbiter.Functions;

/// <summary>
/// Represents a binding entry in an Azure Functions <c>function.json</c> definition.
/// </summary>
public record FunctionBinding
{
    /// <summary>
    /// Gets the binding name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the binding type.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the binding direction (for example, <c>in</c> or <c>out</c>).
    /// </summary>
    [JsonPropertyName("direction")]
    public string? Direction { get; init; }

    /// <summary>
    /// Gets the authorization level for HTTP trigger bindings.
    /// </summary>
    [JsonPropertyName("authLevel")]
    public string? AuthorizationLevel { get; init; }

    /// <summary>
    /// Gets the allowed HTTP methods for HTTP trigger bindings.
    /// </summary>
    [JsonPropertyName("methods")]
    public IReadOnlyList<string>? Methods { get; init; }

    /// <summary>
    /// Gets the route template for HTTP trigger bindings.
    /// </summary>
    [JsonPropertyName("route")]
    public string? Route { get; init; }

    /// <summary>
    /// Gets the CRON schedule expression for timer trigger bindings.
    /// </summary>
    [JsonPropertyName("schedule")]
    public string? Schedule { get; init; }

    /// <summary>
    /// Gets additional binding-specific properties.
    /// </summary>
    [JsonPropertyName("properties")]
    public JsonElement? Properties { get; init; }

    /// <summary>
    /// Gets the queue name for queue bindings.
    /// </summary>
    [JsonPropertyName("queueName")]
    public string? QueueName { get; init; }

    /// <summary>
    /// Gets the topic name for Service Bus topic bindings.
    /// </summary>
    [JsonPropertyName("topicName")]
    public string? TopicName { get; init; }

    /// <summary>
    /// Gets the subscription name for Service Bus subscription bindings.
    /// </summary>
    [JsonPropertyName("subscriptionName")]
    public string? SubscriptionName { get; init; }

    /// <summary>
    /// Gets the application setting name that contains the binding connection string.
    /// </summary>
    [JsonPropertyName("connection")]
    public string? Connection { get; init; }

    /// <summary>
    /// Gets the trigger cardinality setting.
    /// </summary>
    [JsonPropertyName("cardinality")]
    public string? Cardinality { get; init; }

    /// <summary>
    /// Gets a value indicating whether messages are automatically completed for Service Bus triggers.
    /// </summary>
    [JsonPropertyName("autoCompleteMessages")]
    public bool? AutoCompleteMessages { get; init; }
}
