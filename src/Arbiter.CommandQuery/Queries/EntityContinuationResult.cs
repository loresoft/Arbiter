using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents the result of an entity query that supports continuation for paging scenarios.
/// </summary>
/// <typeparam name="TReadModel">
/// The type of the read model returned in the query results.
/// </typeparam>
/// <remarks>
/// This class is typically used to return a page of data along with a continuation token
/// that can be used to retrieve the next page in paginated queries, such as those in Blazor WebAssembly applications.
/// </remarks>
public class EntityContinuationResult<TReadModel>
{
    /// <summary>
    /// Gets or sets the continuation token for retrieving the next page of results.
    /// </summary>
    /// <value>
    /// A string token that can be used in subsequent queries to fetch the next set of results,
    /// or <see langword="null"/> if there are no more results.
    /// </value>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Gets or sets the current page of data returned by the query.
    /// </summary>
    /// <value>
    /// A read-only collection of <typeparamref name="TReadModel"/> representing the data for the current page,
    /// or <see langword="null"/> if no data is available.
    /// </value>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<TReadModel>? Data { get; set; }
}
