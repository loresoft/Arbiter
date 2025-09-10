using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// Represents a single JSON Patch operation as defined in RFC 6902.
/// JSON Patch is a format for describing changes to a JSON document.
/// </summary>
public record JsonPatchOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPatchOperation"/> record with the specified parameters.
    /// </summary>
    /// <param name="operation">The operation to be performed.</param>
    /// <param name="path">The JSON pointer that identifies the location within the target document.</param>
    /// <param name="value">The value to be used within the operation.</param>
    /// <param name="from">The source location for "move" and "copy" operations.</param>
    [JsonConstructor]
    public JsonPatchOperation(string operation, string path, object? value = null, string? from = null)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Value = value;
        From = from;
    }

    /// <summary>
    /// Gets the operation to be performed.
    /// </summary>
    /// <value>
    /// The operation type. Valid values are: "add", "remove", "replace", "move", "copy", "test".
    /// This property is required and must be provided during initialization.
    /// </value>
    [JsonPropertyName("op")]
    public string Operation { get; }

    /// <summary>
    /// Gets the JSON pointer that identifies the location within the target document where the operation is performed.
    /// </summary>
    /// <value>
    /// A JSON pointer string (RFC 6901) that references a location in the target document.
    /// This property is required and must be provided during initialization.
    /// </value>
    [JsonPropertyName("path")]
    public string Path { get; }

    /// <summary>
    /// Gets the value to be used within the operation.
    /// </summary>
    /// <value>
    /// The value for the operation. Used by "add", "replace", and "test" operations.
    /// This property is ignored during serialization when null.
    /// </value>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Value { get; }

    /// <summary>
    /// Gets the source location for "move" and "copy" operations.
    /// </summary>
    /// <value>
    /// A JSON pointer string (RFC 6901) that references the source location for "move" and "copy" operations.
    /// This property is ignored during serialization when null and when the operation is not "move" or "copy".
    /// </value>
    [JsonPropertyName("from")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? From { get; }

    /// <summary>
    /// Determines whether the <see cref="From"/> property should be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the operation is "move" or "copy" (case-insensitive); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method is used by the JSON serializer to conditionally include the "from" property
    /// only for operations that require it ("move" and "copy").
    /// </remarks>
    public bool ShouldSerializeFrom()
    {
        return (string.Equals(Operation, "move", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Operation, "copy", StringComparison.OrdinalIgnoreCase));
    }
}
