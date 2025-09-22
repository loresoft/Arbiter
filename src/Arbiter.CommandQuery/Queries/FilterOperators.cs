using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Defines supported query operators used in filtering and searching within data-bound components.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FilterOperators>))]
public enum FilterOperators
{
    /// <summary>
    /// Represents the "equal" operator for comparing values.
    /// </summary>
    Equal = 0,

    /// <summary>
    /// Represents the "not equal" operator for comparing values.
    /// </summary>
    NotEqual = 1,

    /// <summary>
    /// Represents the "contains" operator for substring or collection membership checks.
    /// </summary>
    Contains = 2,

    /// <summary>
    /// Represents the "not contains" operator for substring or collection membership checks.
    /// </summary>
    NotContains = 3,

    /// <summary>
    /// Represents the "starts with" operator for prefix matching.
    /// </summary>
    StartsWith = 4,

    /// <summary>
    /// Represents the "not starts with" operator for prefix matching.
    /// </summary>
    NotStartsWith = 5,

    /// <summary>
    /// Represents the "ends with" operator for suffix matching.
    /// </summary>
    EndsWith = 6,

    /// <summary>
    /// Represents the "not ends with" operator for suffix matching.
    /// </summary>
    NotEndsWith = 7,

    /// <summary>
    /// Represents the "greater than" operator for numeric or date comparisons.
    /// </summary>
    GreaterThan = 8,

    /// <summary>
    /// Represents the "greater than or equal" operator for numeric or date comparisons.
    /// </summary>
    GreaterThanOrEqual = 9,

    /// <summary>
    /// Represents the "less than" operator for numeric or date comparisons.
    /// </summary>
    LessThan = 10,

    /// <summary>
    /// Represents the "less than or equal" operator for numeric or date comparisons.
    /// </summary>
    LessThanOrEqual = 11,

    /// <summary>
    /// Represents the "is null" operator for checking if a value is null.
    /// </summary>
    IsNull = 12,

    /// <summary>
    /// Represents the "is not null" operator for checking if a value is not null.
    /// </summary>
    IsNotNull = 13,

    /// <summary>
    /// Represents the "in" operator for checking if a value is within a set of values.
    /// </summary>
    In = 14,

    /// <summary>
    /// Represents the "not in" operator for checking if a value is not within a set of values.
    /// </summary>
    NotIn = 15,

    /// <summary>
    /// Represents a custom expression operator for advanced querying scenarios.
    /// </summary>
    Expression = 16
}
