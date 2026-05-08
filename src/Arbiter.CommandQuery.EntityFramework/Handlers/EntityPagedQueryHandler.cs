using System.Linq.Dynamic.Core;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;
using Arbiter.Mapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler that processes an <see cref="EntityPagedQuery{TReadModel}"/> by applying filters, sorting,
/// and pagination to a <see cref="DbSet{TEntity}"/> within the specified <typeparamref name="TContext"/>,
/// and returns the matching entities projected to <typeparamref name="TReadModel"/> as an
/// <see cref="EntityPagedResult{TReadModel}"/>.
/// </summary>
/// <typeparam name="TContext">The <see cref="DbContext"/> used to access the data store.</typeparam>
/// <typeparam name="TEntity">The entity type being queried. Must be a reference type.</typeparam>
/// <typeparam name="TReadModel">The output model type returned for each matched entity.</typeparam>
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
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to access the data store.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to project <typeparamref name="TEntity"/> to <typeparamref name="TReadModel"/>.</param>
    /// <param name="pipeline">An optional <see cref="IQueryPipeline"/> applied to queries executed by this handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="dataContext"/>, or <paramref name="mapper"/> is <see langword="null"/>.</exception>
    public EntityPagedQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }


    /// <inheritdoc/>
    /// <remarks>
    /// Applies optional query pipeline modifiers, builds the filter/sort query via <see cref="BuildQuery"/>,
    /// retrieves the total count via <see cref="QueryTotal"/>, and — when the total is greater than zero —
    /// fetches and projects the current page via <see cref="QueryPaged"/>.
    /// Returns an empty <see cref="EntityPagedResult{TReadModel}"/> immediately when the total is zero.
    /// </remarks>
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
    /// Applies filter and raw query predicates from <paramref name="request"/> to <paramref name="query"/>.
    /// </summary>
    /// <param name="request">The paged query request containing the <see cref="EntityQuery"/> filter and raw query predicates.</param>
    /// <param name="query">The <see cref="IQueryable{T}"/> to apply the predicates to.</param>
    /// <returns>The <see cref="IQueryable{T}"/> with all applicable predicates applied.</returns>
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
    /// Returns the total number of entities that match the filtered <paramref name="query"/> before pagination is applied.
    /// </summary>
    /// <param name="request">The paged query request (reserved for override use).</param>
    /// <param name="query">The filtered <see cref="IQueryable{T}"/> to count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total count of matching entities.</returns>
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
    /// Applies sorting and pagination to <paramref name="query"/>, projects the results to
    /// <typeparamref name="TReadModel"/>, and returns them as an <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <param name="request">The paged query request containing the sort and pagination criteria.</param>
    /// <param name="query">The filtered <see cref="IQueryable{T}"/> to sort, page, and project.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of <typeparamref name="TReadModel"/> for the current page.</returns>
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
