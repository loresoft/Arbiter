using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a sort expression for an entity.
/// </summary>
public class EntitySort
{
    /// <summary>
    /// The name of the property to sort by.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The direction of the sort (e.g., "asc" or "desc").
    /// </summary>
    [JsonPropertyName("direction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Direction { get; set; }

    /// <summary>
    /// Parses a string representation of an entity sort.
    /// </summary>
    /// <param name="sortString">The sort expression</param>
    /// <returns>An instance of <see cref="EntitySort"/> for the parsed sort expression</returns>
    public static EntitySort? Parse(string? sortString)
    {
        if (string.IsNullOrEmpty(sortString))
            return null;

        var parts = sortString.Split([':'], StringSplitOptions.RemoveEmptyEntries);
        if (parts is null || parts.Length == 0)
            return null;

        var sort = new EntitySort();
        sort.Name = parts[0].Trim();

        if (parts.Length >= 2)
            sort.Direction = parts[1]?.Trim();

        return sort;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Direction);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Direction))
            return Name;

        return $"{Name}:{Direction}";
    }
}
