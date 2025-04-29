using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a query for retrieving paged entities using a continuation token based on an <see cref="EntitySelect"/>.
/// The result of the query will be of type <see cref="EntityContinuationResult{TReadModel}"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the query.</typeparam>
/// <remarks>
/// This query is typically used in scenarios where data needs to be retrieved in pages, with each page being identified
/// by a continuation token. The <see cref="EntitySelect"/> allows filtering and sorting of the data.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityContinuationQuery{TReadModel}"/>:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var entitySelect = new EntitySelect
/// {
///     Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
///     Sort = new List&lt;EntitySort&gt; { new EntitySort { Name = "Name", Direction = "asc" } }
/// };
///
/// var query = new EntityContinuationQuery&lt;ProductReadModel&gt;(principal, entitySelect, pageSize: 20, continuationToken: "abc123");
///
/// // Send the query to the mediator instance
/// var result = await mediator.Send(query);
/// Console.WriteLine($"Continuation Token: {result?.ContinuationToken}");
/// </code>
/// </example>
public record EntityContinuationQuery<TReadModel> : CacheableQueryBase<EntityContinuationResult<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityContinuationQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="query">The <see cref="EntitySelect"/> defining the filter and sort criteria for the query.</param>
    /// <param name="pageSize">The number of items to retrieve per page.</param>
    /// <param name="continuationToken">The continuation token for retrieving the next page of results.</param>
    public EntityContinuationQuery(ClaimsPrincipal? principal, EntitySelect? query, int pageSize = 10, string? continuationToken = null)
        : base(principal)
    {
        Query = query ?? new EntitySelect();
        PageSize = pageSize;
        ContinuationToken = continuationToken;
    }

    /// <summary>
    /// Gets the <see cref="EntitySelect"/> defining the filter and sort criteria for the query.
    /// </summary>
    [JsonPropertyName("query")]
    public EntitySelect Query { get; }

    /// <summary>
    /// Gets the number of items to retrieve per page.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; }

    /// <summary>
    /// Gets the continuation token for retrieving the next page of results.
    /// </summary>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; }

    /// <inheritdoc/>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>
        (
            bucket: CacheTagger.Buckets.Continuation,
            value: HashCode.Combine(Query.GetHashCode(), PageSize, ContinuationToken)
        );

    /// <inheritdoc/>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
