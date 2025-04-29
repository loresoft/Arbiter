using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a query for selecting entities based on an <see cref="EntitySelect"/>.
/// The result of the query will be a collection of type <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the query.</typeparam>
/// <remarks>
/// This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve a collection of entities
/// based on filtering and sorting criteria defined in an <see cref="EntitySelect"/>.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntitySelectQuery{TReadModel}"/>:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var filter = new EntityFilter
/// {
///     Name = "Status",
///     Operator = "eq",
///     Value = "Active"
/// };
/// var sort = new EntitySort
/// {
///     Name = "Name",
///     Direction = "asc"
/// };
/// var query = new EntitySelectQuery&lt;ProductReadModel&gt;(principal, filter, sort);
///
/// // Send the query to the mediator instance
/// var result = await mediator.Send(query);
/// Console.WriteLine($"Retrieved {result?.Count} entities.");
/// </code>
/// </example>
public record EntitySelectQuery<TReadModel> : CacheableQueryBase<IReadOnlyCollection<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class with a default <see cref="EntitySelect"/>.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    public EntitySelectQuery(ClaimsPrincipal? principal)
        : this(principal, new EntitySelect())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class with a filter.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="filter">The <see cref="EntityFilter"/> to create an <see cref="EntitySelect"/> from.</param>
    public EntitySelectQuery(ClaimsPrincipal? principal, EntityFilter filter)
        : this(principal, new EntitySelect(filter))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class with a filter and a single sort expression.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="filter">The <see cref="EntityFilter"/> to create an <see cref="EntitySelect"/> from.</param>
    /// <param name="sort">The <see cref="EntitySort"/> to create an <see cref="EntitySelect"/> from.</param>
    public EntitySelectQuery(ClaimsPrincipal? principal, EntityFilter filter, EntitySort sort)
        : this(principal, filter, new[] { sort })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class with a filter and multiple sort expressions.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="filter">The <see cref="EntityFilter"/> to create an <see cref="EntitySelect"/> from.</param>
    /// <param name="sort">The list of <see cref="EntitySort"/> to create an <see cref="EntitySelect"/> from.</param>
    public EntitySelectQuery(ClaimsPrincipal? principal, EntityFilter filter, IEnumerable<EntitySort> sort)
        : this(principal, new EntitySelect(filter, sort))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class with a custom <see cref="EntitySelect"/>.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="select">The <see cref="EntitySelect"/> defining the filter and sort criteria for the query.</param>
    [JsonConstructor]
    public EntitySelectQuery(ClaimsPrincipal? principal, EntitySelect? select)
        : base(principal)
    {
        Select = select ?? new EntitySelect();
    }

    /// <summary>
    /// Gets the <see cref="EntitySelect"/> defining the filter and sort criteria for the query.
    /// </summary>
    [JsonPropertyName("select")]
    public EntitySelect Select { get; }

    /// <summary>
    /// Generates a cache key for the query based on the <see cref="EntitySelect"/>.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query.
    /// </returns>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.List, Select.GetHashCode());

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/>.
    /// </summary>
    /// <returns>
    /// The cache tag for the <typeparamref name="TReadModel"/>, or <see langword="null"/> if no tag is available.
    /// </returns>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
