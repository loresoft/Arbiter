using System.Linq.Dynamic.Core;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles paged queries for entities in a MongoDB repository.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public class EntityPagedQueryHandler<TRepository, TEntity, TKey, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPagedQueryHandler{TRepository, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The MongoDB repository.</param>
    /// <param name="mapper">The mapper for converting entities to read models.</param>
    public EntityPagedQueryHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the paged query request.
    /// </summary>
    /// <param name="request">The paged query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the paged query.</returns>
    protected override async ValueTask<EntityPagedResult<TReadModel>?> Process(EntityPagedQuery<TReadModel> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = Repository.All();

        // Build query from filter
        query = await BuildQuery(request, query).ConfigureAwait(false);

        // Get total for query
        int total = await QueryTotal(request, query, cancellationToken).ConfigureAwait(false);

        // Short circuit if total is zero
        if (total == 0)
            return new EntityPagedResult<TReadModel> { Data = new List<TReadModel>() };

        // Page the query and convert to read model
        var result = await QueryPaged(request, query, cancellationToken).ConfigureAwait(false);

        return new EntityPagedResult<TReadModel>
        {
            Total = total,
            Data = result
        };
    }

    /// <summary>
    /// Builds the query based on the request's filter and query parameters.
    /// </summary>
    /// <param name="request">The paged query request.</param>
    /// <param name="query">The initial query.</param>
    /// <returns>The modified query as a ValueTask.</returns>
    protected virtual ValueTask<IQueryable<TEntity>> BuildQuery(EntityPagedQuery<TReadModel> request, IQueryable<TEntity> query)
    {
        var entityQuery = request.Query;

        // Build query from filter
        if (entityQuery?.Filter != null)
            query = query.Filter(entityQuery.Filter);

        // Add raw query
        if (!string.IsNullOrEmpty(entityQuery?.Query))
            query = query.Where(entityQuery.Query);

        return ValueTask.FromResult(query);
    }

    /// <summary>
    /// Calculates the total number of entities matching the query.
    /// </summary>
    /// <param name="request">The paged query request.</param>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total count of matching entities.</returns>
    protected virtual async ValueTask<int> QueryTotal(EntityPagedQuery<TReadModel> request, IQueryable<TEntity> query, CancellationToken cancellationToken)
    {
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a paged collection of entities and maps them to the read model.
    /// </summary>
    /// <param name="request">The paged query request.</param>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of the read model.</returns>
    protected virtual async ValueTask<IReadOnlyCollection<TReadModel>> QueryPaged(EntityPagedQuery<TReadModel> request, IQueryable<TEntity> query, CancellationToken cancellationToken)
    {
        var entityQuery = request.Query;

        var queryable = query
            .Sort(entityQuery.Sort);

        if (entityQuery.Page > 0 && entityQuery.PageSize > 0)
            queryable = queryable.Page(entityQuery.Page, entityQuery.PageSize);

        return await queryable
            .ProjectTo<TEntity, TReadModel>(Mapper)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
