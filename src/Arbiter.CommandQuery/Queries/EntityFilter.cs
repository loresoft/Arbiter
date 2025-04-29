using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Converters;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a filter for selecting entities based on specific criteria.
/// </summary>
/// <remarks>
/// This class is typically used in queries to define filtering criteria for entities. Filters can be combined using logical operators
/// such as "and" or "or" and can include nested filters for complex queries.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityFilter"/> class as a basic filter:
/// <code>
/// var filter = new EntityFilter
/// {
///     Name = "Status",
///     Operator = "eq",
///     Value = "Active"
/// };
/// </code>
/// The following example demonstrates how to use the <see cref="EntityFilter"/> class as group filter:
/// <code>
/// var filter = new EntityFilter
/// {
///     Logic = "and",
///     Filters = new List&lt;EntityFilter&gt;
///     {
///         new EntityFilter { Name = "Priority", Operator = "gt", Value = 1 },
///         new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" }
///     }
/// };
/// </code>
/// </example>
[JsonConverter(typeof(EntityFilterConverter))]
public class EntityFilter
{
    /// <summary>
    /// Gets or sets the name of the field or property to filter on.
    /// </summary>
    /// <value>
    /// The name of the field or property to filter on.
    /// </value>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the operator to use for the filter. This can be "eq" (equals), "ne" (not equals), "gt" (greater than),
    /// "lt" (less than), "ge" (greater than or equal), "le" (less than or equal), "contains", "startswith", or "endswith".
    /// </summary>
    /// <value>
    /// The operator to use for the filter.
    /// </value>
    /// <seealso cref="EntityFilterOperators"/>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    /// <summary>
    /// Gets or sets the value to filter on.
    /// </summary>
    /// <value>
    /// The value to filter on.
    /// </value>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the logical operator to use for combining filters. This can be "and" or "or".
    /// </summary>
    /// <value>
    /// The logical operator to use for combining filters.
    /// </value>
    /// <seealso cref="EntityFilterLogic"/>
    [JsonPropertyName("logic")]
    public string? Logic { get; set; }

    /// <summary>
    /// Gets or sets the list of nested filters to apply to the query. The logic for these filters is defined by the <see cref="Logic"/> property.
    /// </summary>
    /// <value>
    /// The list of nested filters to apply to the query.
    /// </value>
    [JsonPropertyName("filters")]
    public IList<EntityFilter>? Filters { get; set; }

    /// <summary>
    /// Determines whether this filter is valid. A filter is considered valid if it has a name or if it contains a list of valid nested filters.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if the filter is valid; otherwise, <see langword="false" />.
    /// </returns>
    public bool IsValid() => Filters?.Any(f => f.IsValid()) == true || Name is not null;

    /// <summary>
    /// Computes the hash code for the current <see cref="EntityFilter"/> instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="EntityFilter"/> instance.
    /// </returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Operator);
        hash.Add(Value);
        hash.Add(Logic);

        if (Filters == null)
            return hash.ToHashCode();

        foreach (var filter in Filters)
            hash.Add(filter.GetHashCode());

        return hash.ToHashCode();
    }
}
