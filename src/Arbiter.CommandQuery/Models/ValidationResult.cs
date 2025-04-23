using System.Text.Json.Serialization;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// A class that represents the result of a validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation was successful.
    /// </summary>
    [JsonPropertyName("isValid")]
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets or sets the validation errors.  The dictionary key is the property name, and the value is an array of error messages.
    /// </summary>
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
        return string.Join(separator, Errors.Select(failure => failure.Value));
    }
}
