using System.Linq.Dynamic.Core;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler for a request that reads a paged collection of entities in the specified <see cref="DbContext"/>.
/// </summary>
/// <inheritdoc/>
public class EntityPagedQueryHandler<TContext, TEntity, TReadModel>
    : DataContextHandlerBase<TContext, EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>
    where TContext : DbContext
    where TEntity : class
{
    /// <summary>
    /// Represents the name of the entity type associated with <typeparamref name="TEntity"/>.
    /// </summary>
    protected static readonly string EntityName = typeof(TEntity).Name;

    /// <summary>
    /// Represents the name of the read model type associated with <typeparamref name="TReadModel"/>.
    /// </summary>
    protected static readonly string ModelName = typeof(TReadModel).Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPagedQueryHandler{TContext, TEntity, TReadModel}"/> class.
    /// </summary>
    /// <inheritdoc/>
    public EntityPagedQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }


    /// <inheritdoc/>
    protected override async ValueTask<EntityPagedResult<TReadModel>?> Process(
        EntityPagedQuery<TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"EntityPagedQueryHandler; Context:{ContextName}, Entity:{EntityName}, Model:{ModelName}");

        // apply query pipeline modifiers
        query = await query
            .ApplyPipeline(Pipeline, DataContext, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);

        // build query from filter
        query = await BuildQuery(request, query)
            .ConfigureAwait(false);

        // get total for query
        int total = await QueryTotal(request, query, cancellationToken)
            .ConfigureAwait(false);

        // short circuit if total is zero
        if (total == 0)
            return new EntityPagedResult<TReadModel> { Data = [] };

        // page the query and convert to read model
        var result = await QueryPaged(request, query, cancellationToken)
            .ConfigureAwait(false);

        return new EntityPagedResult<TReadModel>
        {
            Total = total,
            Data = result,
        };
    }

    /// <summary>
    /// Builds the query from the request.
    /// </summary>
    /// <param name="request">The request to build the query from.</param>
    /// <param name="query">The IQueryable to apply the entity query to</param>
    /// <returns>A ValueTask containing an IQueryable with the entity query applied</returns>
    protected virtual ValueTask<IQueryable<TEntity>> BuildQuery(EntityPagedQuery<TReadModel> request, IQueryable<TEntity> query)
    {
        var entityQuery = request.Query;

        // build query from filter
        if (entityQuery?.Filter != null)
            query = query.Filter(entityQuery.Filter);

        // add raw query
        if (!string.IsNullOrEmpty(entityQuery?.Query))
            query = query.Where(entityQuery.Query);

        return ValueTask.FromResult(query);
    }

    /// <summary>
    /// Queries the total number of items for the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="request">The request to build the query from.</param>
    /// <param name="query">The IQueryable to apply the entity query to</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The total number of items for the <paramref name="query"/></returns>
    protected virtual async ValueTask<int> QueryTotal(
        EntityPagedQuery<TReadModel> request,
        IQueryable<TEntity> query,
        CancellationToken cancellationToken)
    {
        return await query
            .TagWithCallSite()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Queries the paged data for the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="request">The request to build the query from.</param>
    /// <param name="query">The IQueryable to apply the entity query to</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A list entities for the specified query</returns>
    protected virtual async ValueTask<IReadOnlyList<TReadModel>> QueryPaged(
        EntityPagedQuery<TReadModel> request,
        IQueryable<TEntity> query,
        CancellationToken cancellationToken)
    {
        var entityQuery = request.Query;

        var queryable = query
            .Sort(entityQuery.Sort);

        if (entityQuery.Page > 0 && entityQuery.PageSize > 0)
            queryable = queryable.Page(entityQuery.Page.Value, entityQuery.PageSize.Value);

        queryable = queryable.TagWithCallSite();

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(queryable);

        return await projected
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
