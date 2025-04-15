using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A query for selecting entities based on an <see cref="EntitySelect"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public record EntitySelectQuery<TReadModel> : CacheableQueryBase<IReadOnlyCollection<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    public EntitySelectQuery(ClaimsPrincipal? principal)
        : this(principal, new EntitySelect())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="filter">The <see cref="EntityFilter"/> to create an <see cref="EntitySelect"/> from</param>
    public EntitySelectQuery(ClaimsPrincipal? principal, EntityFilter filter)
        : this(principal, new EntitySelect(filter))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="filter">The <see cref="EntityFilter"/> to create an <see cref="EntitySelect"/> from</param>
    /// <param name="sort">The <see cref="EntitySort"/> to create an <see cref="EntitySelect"/> from</param>
    public EntitySelectQuery(ClaimsPrincipal? principal, EntityFilter filter, EntitySort sort)
        : this(principal, filter, [sort])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="filter">The <see cref="EntityFilter"/> to create an <see cref="EntitySelect"/> from</param>
    /// <param name="sort">The list of <see cref="EntitySort"/> to create an <see cref="EntitySelect"/> from</param>
    public EntitySelectQuery(ClaimsPrincipal? principal, EntityFilter filter, IEnumerable<EntitySort> sort)
        : this(principal, new EntitySelect(filter, sort))
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="select">The <see cref="EntitySelect"/> for this query</param>
    [JsonConstructor]
    public EntitySelectQuery(ClaimsPrincipal? principal, EntitySelect? select)
        : base(principal)
    {
        Select = select ?? new EntitySelect();
    }

    /// <summary>
    /// The <see cref="EntitySelect"/> for this query.
    /// </summary>
    [JsonPropertyName("select")]
    public EntitySelect Select { get; }


    /// <inheritdoc/>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.List, Select.GetHashCode());

    /// <inheritdoc/>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
