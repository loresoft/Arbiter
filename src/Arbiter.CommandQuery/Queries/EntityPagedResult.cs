using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A paged result for an entity query.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
[MessagePackObject]
public partial class EntityPagedResult<TReadModel>
{
    /// <summary>
    /// Gets an empty instance of the <see cref="EntityPagedResult{TReadModel}"/> class.
    /// </summary>
    [SuppressMessage("Design", "MA0018:Do not declare static members on generic types", Justification = "<Pending>")]
    public static EntityPagedResult<TReadModel> Empty { get; } = new();

    /// <summary>
    /// Gets or sets the continuation token for retrieving the next page of results.
    /// </summary>
    /// <value>
    /// A string token that can be used in subsequent queries to fetch the next set of results,
    /// or <see langword="null"/> if there are no more results.
    /// </value>
    [Key(0)]
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// The total number of the results for the query.
    /// </summary>
    [Key(1)]
    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Total { get; set; }

    /// <summary>
    /// The current page of data for the query.
    /// </summary>
    [Key(2)]
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<TReadModel>? Data { get; set; }
}
