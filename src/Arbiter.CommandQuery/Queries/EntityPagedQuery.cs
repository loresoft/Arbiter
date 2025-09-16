using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a query for retrieving paged entities based on an <see cref="EntityQuery"/>.
/// The result of the query will be of type <see cref="EntityPagedResult{TReadModel}"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the query.</typeparam>
/// <remarks>
/// This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve entities
/// in a paginated format. The <see cref="EntityQuery"/> allows filtering, sorting, and pagination criteria to be specified.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityPagedQuery{TReadModel}"/>:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var entityQuery = new EntityQuery
/// {
///     Filter = new EntityFilter { Name = "Status", Operator = "eq", Value = "Active" },
///     Sort = new List&lt;EntitySort&gt; { new EntitySort { Name = "Name", Direction = "asc" } },
///     Page = 1,
///     PageSize = 20
/// };
///
/// var query = new EntityPagedQuery&lt;ProductReadModel&gt;(principal, entityQuery);
///
/// // Send the query to the mediator instance
/// var result = await mediator.Send(query);
/// Console.WriteLine($"Total Results: {result?.Total}");
/// </code>
/// </example>
public record EntityPagedQuery<TReadModel> : CacheableQueryBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPagedQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="query">The <see cref="EntityQuery"/> defining the filter, sort, and pagination criteria for the query.</param>
    public EntityPagedQuery(ClaimsPrincipal? principal, EntityQuery? query)
        : base(principal)
    {
        Query = query ?? new EntityQuery();
    }

    /// <summary>
    /// Gets the <see cref="EntityQuery"/> defining the filter, sort, and pagination criteria for the query.
    /// </summary>
    [JsonPropertyName("query")]
    public EntityQuery Query { get; }

    /// <summary>
    /// Generates a cache key for the query based on the <see cref="EntityQuery"/>.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query.
    /// </returns>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.Paged, Query.GetHashCode());

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/>.
    /// </summary>
    /// <returns>
    /// The cache tag for the <typeparamref name="TReadModel"/>, or <see langword="null"/> if no tag is available.
    /// </returns>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
