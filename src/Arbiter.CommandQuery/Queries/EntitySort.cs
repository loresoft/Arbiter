using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a sort expression for an entity, specifying the property to sort by and the sort direction.
/// </summary>
/// <remarks>
/// This class is typically used in queries to define sorting criteria for entities. The sort direction can be
/// ascending ("asc") or descending ("desc").
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntitySort"/> class:
/// <code>
/// var sort = new EntitySort
/// {
///     Name = "Name",
///     Direction = "asc"
/// };
///
/// Console.WriteLine(sort.ToString()); // Output: "Name:asc"
/// </code>
/// </example>
public class EntitySort
{
    /// <summary>
    /// Gets or sets the name of the property to sort by.
    /// </summary>
    /// <value>
    /// The name of the property to sort by.
    /// </value>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the direction of the sort (e.g., "asc" for ascending or "desc" for descending).
    /// </summary>
    /// <value>
    /// The direction of the sort. If <see langword="null"/>, the default sort direction is used.
    /// </value>
    [JsonPropertyName("direction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SortDirections? Direction { get; set; }

    /// <summary>
    /// Parses a string representation of a sort expression into an <see cref="EntitySort"/> instance.
    /// </summary>
    /// <param name="sortString">The sort expression in the format "PropertyName:Direction".</param>
    /// <returns>
    /// An instance of <see cref="EntitySort"/> for the parsed sort expression, or <see langword="null"/> if the input is invalid.
    /// </returns>
    /// <example>
    /// The following example demonstrates how to parse a sort string:
    /// <code>
    /// var sort = EntitySort.Parse("Name:desc");
    /// Console.WriteLine(sort?.Name); // Output: "Name"
    /// Console.WriteLine(sort?.Direction); // Output: "desc"
    /// </code>
    /// </example>
    public static EntitySort? Parse(string? sortString)
    {
        if (string.IsNullOrEmpty(sortString))
            return null;

        var parts = sortString.Split([':'], StringSplitOptions.RemoveEmptyEntries);
        if (parts is null || parts.Length == 0)
            return null;

        var sort = new EntitySort();
        sort.Name = parts[0].Trim();

        if (parts.Length < 2)
            return sort;

        var direction = parts[1]?.Trim().ToLowerInvariant();

        sort.Direction = direction switch
        {
            "asc" => SortDirections.Ascending,
            "ascending" => SortDirections.Ascending,
            "desc" => SortDirections.Descending,
            "descending" => SortDirections.Descending,
            _ => SortDirections.Ascending,
        };

        return sort;
    }

    /// <summary>
    /// Computes the hash code for the current <see cref="EntitySort"/> instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="EntitySort"/> instance.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Direction);
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="EntitySort"/> instance.
    /// </summary>
    /// <returns>
    /// A string representation of the sort expression in the format "PropertyName:Direction".
    /// If the direction is not specified, only the property name is returned.
    /// </returns>
    public override string ToString()
    {
        if (Direction == null)
            return Name;

        if (Direction == SortDirections.Descending)
            return $"{Name} desc";

        return $"{Name} asc";
    }
}
