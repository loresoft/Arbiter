using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a sort expression for an entity, specifying the property to sort by and the sort direction.
/// </summary>
/// <remarks>
/// <para>
/// This class is used in query operations to define sorting criteria for entities. It supports both ascending and descending sort directions
/// and can be serialized to JSON for use in web APIs and data transfer scenarios.
/// </para>
/// <para>
/// The class provides parsing capabilities to convert string representations of sort expressions into strongly-typed <see cref="EntitySort"/> instances.
/// It supports multiple string formats including "PropertyName", "PropertyName:asc", "PropertyName desc", etc.
/// </para>
/// <para>
/// This class is typically used in conjunction with <see cref="EntityQuery"/> and <see cref="EntityFilter"/> to build complex data queries.
/// </para>
/// </remarks>
/// <example>
/// <para>The following example demonstrates how to create and use the <see cref="EntitySort"/> class:</para>
/// <code>
/// // Create a sort expression programmatically
/// var sort = new EntitySort
/// {
///     Name = "LastName",
///     Direction = SortDirections.Ascending
/// };
///
/// // Parse from string representation
/// var parsedSort = EntitySort.Parse("FirstName:desc");
///
/// // Use in query scenarios
/// var query = new EntityQuery
/// {
///     Sort = new[] { sort, parsedSort }
/// };
///
/// Console.WriteLine(sort.ToString()); // Output: "LastName asc"
/// Console.WriteLine(parsedSort?.ToString()); // Output: "FirstName desc"
/// </code>
/// </example>
/// <seealso cref="SortDirections"/>
/// <seealso cref="EntityQuery"/>
/// <seealso cref="EntityFilter"/>
public class EntitySort
{
    /// <summary>
    /// Gets or sets the name of the property to sort by.
    /// </summary>
    /// <value>
    /// The name of the property or field to use for sorting. This should correspond to a valid property name on the entity being queried.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property specifies which field or property of the entity should be used for sorting. The value should match
    /// the exact property name as it exists on the entity class, including proper casing.
    /// </para>
    /// <para>
    /// When used with Entity Framework or similar ORMs, this name is typically mapped directly to database column names
    /// or entity property expressions.
    /// </para>
    /// </remarks>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the direction of the sort operation.
    /// </summary>
    /// <value>
    /// The direction of the sort using the <see cref="SortDirections"/> enumeration.
    /// If <see langword="null"/>, the default sort direction (ascending) is typically used by the consuming system.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property determines whether the sort operation should be performed in ascending or descending order.
    /// When serialized to JSON, this property uses string enum conversion and is omitted when the value is <see langword="null"/>.
    /// </para>
    /// <para>
    /// The JSON representation uses "asc" for ascending and "desc" for descending, making it compatible with
    /// common web API conventions and query string parameters.
    /// </para>
    /// </remarks>
    /// <seealso cref="SortDirections"/>
    [JsonPropertyName("direction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(JsonStringEnumConverter<SortDirections>))]
    public SortDirections? Direction { get; set; }

    /// <summary>
    /// Parses a string representation of a sort expression into an <see cref="EntitySort"/> instance.
    /// </summary>
    /// <param name="sortString">
    /// The sort string to parse. Supports multiple formats including:
    /// <list type="bullet">
    /// <item><description>"PropertyName" - Property name only (defaults to ascending)</description></item>
    /// <item><description>"PropertyName asc" or "PropertyName:asc" - Explicit ascending sort</description></item>
    /// <item><description>"PropertyName desc" or "PropertyName:desc" - Explicit descending sort</description></item>
    /// <item><description>"PropertyName ascending" or "PropertyName:ascending" - Full word ascending</description></item>
    /// <item><description>"PropertyName descending" or "PropertyName:descending" - Full word descending</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// An <see cref="EntitySort"/> instance representing the parsed sort expression,
    /// or <see langword="null"/> if the input string is <see langword="null"/>, empty, or cannot be parsed.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides flexible parsing of sort expressions from string format, supporting both colon (:) and space
    /// separators between the property name and direction. The parsing is case-insensitive for the direction component.
    /// </para>
    /// <para>
    /// If no direction is specified in the input string, the method returns an <see cref="EntitySort"/> instance with
    /// only the <see cref="Name"/> property set, leaving <see cref="Direction"/> as <see langword="null"/>.
    /// </para>
    /// <para>
    /// Invalid direction values are treated as ascending by default, ensuring that malformed input doesn't cause parsing failures.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">This method does not throw exceptions for invalid input; it returns <see langword="null"/> instead.</exception>
    public static EntitySort? Parse(string? sortString)
    {
        if (string.IsNullOrEmpty(sortString))
            return null;

        // support "Name desc" or "Name:desc"
        var parts = sortString.Split([':', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
    /// A hash code value that represents the current <see cref="EntitySort"/> instance,
    /// computed from both the <see cref="Name"/> and <see cref="Direction"/> properties.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Direction);
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="EntitySort"/> instance.
    /// </summary>
    /// <returns>
    /// A string representation of the sort expression in a human-readable format.
    /// <list type="bullet">
    /// <item><description>If <see cref="Direction"/> is <see langword="null"/>, returns only the property name.</description></item>
    /// <item><description>If <see cref="Direction"/> is <see cref="SortDirections.Ascending"/>, returns "PropertyName asc".</description></item>
    /// <item><description>If <see cref="Direction"/> is <see cref="SortDirections.Descending"/>, returns "PropertyName desc".</description></item>
    /// </list>
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
