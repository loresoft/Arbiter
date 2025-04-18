using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler for a request that reads a collection of entities in the specified <see cref="DbContext"/>.
/// </summary>
/// <inheritdoc/>
public class EntityIdentifiersQueryHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifiersQueryHandler{TContext, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <inheritdoc/>
    public EntityIdentifiersQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }


    /// <inheritdoc/>
    protected override async ValueTask<IReadOnlyCollection<TReadModel>?> Process(
        EntityIdentifiersQuery<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"EntityIdentifiersQueryHandler; Context:{typeof(TContext).Name}, Entity:{typeof(TEntity).Name}, Model:{typeof(TReadModel).Name}")
            .TagWithCallSite()
            .Where(p => request.Ids.Contains(p.Id));

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(query);

        return await projected
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
