using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Converters;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A filter for selecting entities.
/// </summary>
[JsonConverter(typeof(EntityFilterConverter))]
public class EntityFilter
{
    /// <summary>
    /// The name of the field or property to filter on.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The operator to use for the filter. This can be "eq", "ne", "gt", "lt", "ge", "le", "contains", "startswith", or "endswith".
    /// </summary>
    /// <seealso cref="EntityFilterOperators"/>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    /// <summary>
    /// The value to filter on.
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// The logic to use for the filter.  This can be "and" or "or". Used with <see cref="Filters"/> property.
    /// </summary>
    /// <seealso cref="EntityFilterLogic"/>
    [JsonPropertyName("logic")]
    public string? Logic { get; set; }

    /// <summary>
    /// The list filters to apply to the query.  The logic for these filters is defined by the <see cref="Logic"/> property.
    /// </summary>
    [JsonPropertyName("filters")]
    public IList<EntityFilter>? Filters { get; set; }

    /// <summary>
    /// Check if this filter is valid.  A filter is valid if it has a name or if it has a list of filters.
    /// </summary>
    /// <returns><see langword="true" /> if filter is valid; otherwise <see langword="false" /></returns>
    public bool IsValid() => Filters?.Any(f => f.IsValid()) == true || Name is not null;

    /// <inheritdoc />
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
