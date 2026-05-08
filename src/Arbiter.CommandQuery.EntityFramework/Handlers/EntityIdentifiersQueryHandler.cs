using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.Mapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler that processes an <see cref="EntityIdentifiersQuery{TKey, TReadModel}"/> by querying the
/// specified <typeparamref name="TContext"/> for all <typeparamref name="TEntity"/> instances whose primary
/// key is contained in the request's identifier list, and projecting the results to
/// <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TContext">The <see cref="DbContext"/> used to access the data store.</typeparam>
/// <typeparam name="TEntity">The entity type being queried. Must implement <see cref="IHaveIdentifier{TKey}"/> and have a parameterless constructor.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TReadModel">The output model type returned for each matched entity.</typeparam>
public class EntityIdentifiersQueryHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyList<TReadModel>>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifiersQueryHandler{TContext, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to access the data store.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to project <typeparamref name="TEntity"/> to <typeparamref name="TReadModel"/>.</param>
    /// <param name="pipeline">An optional <see cref="IQueryPipeline"/> applied to queries executed by this handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="dataContext"/>, or <paramref name="mapper"/> is <see langword="null"/>.</exception>
    public EntityIdentifiersQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Applies any optional query pipeline modifiers, then filters the <see cref="DbSet{TEntity}"/>
    /// to entities whose <c>Id</c> is contained in <c>request.Ids</c>. The matched entities are
    /// projected to <typeparamref name="TReadModel"/> and returned as an
    /// <see cref="IReadOnlyList{T}"/>. Returns an empty list when no identifiers match;
    /// returns <see langword="null"/> only if the underlying query returns <see langword="null"/>.
    /// </remarks>
    protected override async ValueTask<IReadOnlyList<TReadModel>?> Process(
        EntityIdentifiersQuery<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"EntityIdentifiersQueryHandler; Context:{ContextName}, Entity:{EntityName}, Model:{ModelName}");

        // apply query pipeline modifiers
        query = await query
            .ApplyPipeline(Pipeline, DataContext, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);

        query = query
            .TagWithCallSite()
            .Where(p => request.Ids.Contains(p.Id));

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(query);

        return await projected
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
