using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// A class that represents the result of a validation.
/// </summary>
[MessagePackObject]
public partial class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation was successful.
    /// </summary>
    [Key(0)]
    [JsonPropertyName("isValid")]
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets or sets the validation errors.  The dictionary key is the property name, and the value is an array of error messages.
    /// </summary>
    [Key(1)]
    [JsonPropertyName("errors")]
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>(StringComparer.Ordinal);

    /// <summary>
    /// Generates a string representation of the error messages separated by new lines.
    /// </summary>
    /// <returns>A string representation of the error message</returns>
    public override string ToString()
    {
        return ToString(Environment.NewLine);
    }

    /// <summary>
    /// Generates a string representation of the error messages separated by the specified character.
    /// </summary>
    /// <param name="separator">The character to separate the error messages.</param>
    /// <returns>A string representation of the error message</returns>
    public string ToString(string separator)
    {
        if (Errors.Count == 0)
            return string.Empty;

        return string.Join(separator, Errors.SelectMany(kvp => kvp.Value));
    }

    /// <summary>
    /// Gets a <see cref="ValidationResult"/> instance that represents a successful validation result.
    /// </summary>
    /// <remarks>
    /// This property provides a predefined instance of <see cref="ValidationResult"/> to represent
    /// success,  avoiding the need to create a new instance for successful validation scenarios.
    /// </remarks>
    public static ValidationResult Success { get; } = new();
}
