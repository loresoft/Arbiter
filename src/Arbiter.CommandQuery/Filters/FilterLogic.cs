using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Filters;

/// <summary>
/// Defines logical operators used in query groups for data-bound components.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FilterLogic>))]
public enum FilterLogic
{
    /// <summary>
    /// Represents the logical "AND" operator for combining query rules.
    /// </summary>
    And = 0,

    /// <summary>
    /// Represents the logical "OR" operator for combining query rules.
    /// </summary>
    Or = 1
}
