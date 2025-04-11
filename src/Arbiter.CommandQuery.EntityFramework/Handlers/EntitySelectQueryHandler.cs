using System.Linq.Dynamic.Core;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

public class EntitySelectQueryHandler<TContext, TEntity, TReadModel>
    : DataContextHandlerBase<TContext, EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>
    where TContext : DbContext
    where TEntity : class
{
    public EntitySelectQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }


    protected override async ValueTask<IReadOnlyCollection<TReadModel>?> Process(EntitySelectQuery<TReadModel> request, CancellationToken cancellationToken = default)
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


    protected virtual IQueryable<TEntity> BuildQuery(EntitySelectQuery<TReadModel> request, IQueryable<TEntity> query)
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

    protected virtual async ValueTask<IReadOnlyCollection<TReadModel>> QueryList(EntitySelectQuery<TReadModel> request, IQueryable<TEntity> query, CancellationToken cancellationToken)
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
