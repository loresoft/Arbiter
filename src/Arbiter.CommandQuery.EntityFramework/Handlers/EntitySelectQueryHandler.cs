using System.Linq.Dynamic.Core;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler for a request that reads a collection of entities in the specified <see cref="DbContext"/>.
/// </summary>
/// <inheritdoc/>
public class EntitySelectQueryHandler<TContext, TEntity, TReadModel>
    : DataContextHandlerBase<TContext, EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>
    where TContext : DbContext
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQueryHandler{TContext, TEntity, TReadModel}"/> class.
    /// </summary>
    /// <inheritdoc/>
    public EntitySelectQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }


    /// <inheritdoc/>
    protected override async ValueTask<IReadOnlyCollection<TReadModel>?> Process(
        EntitySelectQuery<TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"EntitySelectQueryHandler; Context:{typeof(TContext).Name}, Entity:{typeof(TEntity).Name}, Model:{typeof(TReadModel).Name}");

        // build query from filter
        query = BuildQuery(request, query);

        // page the query and convert to read model
        return await QueryList(request, query, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the query from the request.
    /// </summary>
    /// <param name="request">The request to build the query from.</param>
    /// <param name="query">The IQueryable to apply the entity query to</param>
    /// <returns>An IQueryable with the entity query applied</returns>
    protected virtual IQueryable<TEntity> BuildQuery(
        EntitySelectQuery<TReadModel> request,
        IQueryable<TEntity> query)
    {
        var entitySelect = request?.Select;

        // build query from filter
        if (entitySelect?.Filter != null)
            query = query.Filter(entitySelect.Filter);

        // add raw query
        if (!string.IsNullOrEmpty(entitySelect?.Query))
            query = query.Where(entitySelect.Query);

        return query;
    }

    /// <summary>
    /// Queries the data for the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="request">The request to build the query from.</param>
    /// <param name="query">The IQueryable to apply the entity query to</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A list entities for the specified query</returns>
    protected virtual async ValueTask<IReadOnlyCollection<TReadModel>> QueryList(
        EntitySelectQuery<TReadModel> request,
        IQueryable<TEntity> query,
        CancellationToken cancellationToken)
    {
        var queryable = query
            .TagWithCallSite()
            .Sort(request?.Select?.Sort);

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(queryable);

        return await projected
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
