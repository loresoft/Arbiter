using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles queries for retrieving entity identifiers and mapping them to a read model.
/// </summary>
/// <typeparam name="TRepository">The type of the repository used to access the data store.</typeparam>
/// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
/// <typeparam name="TReadModel">The type of the read model to map the entity to.</typeparam>
public class EntityIdentifiersQueryHandler<TRepository, TEntity, TKey, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifiersQueryHandler{TRepository, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for logging.</param>
    /// <param name="repository">The repository used to access the data store.</param>
    /// <param name="mapper">The mapper used to map entities to read models.</param>
    public EntityIdentifiersQueryHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the query to retrieve entities by their identifiers and maps them to the specified read model.
    /// </summary>
    /// <param name="request">The query containing the identifiers of the entities to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of read models corresponding to the retrieved entities.</returns>
    protected override async ValueTask<IReadOnlyCollection<TReadModel>?> Process(EntityIdentifiersQuery<TKey, TReadModel> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var keys = new HashSet<TKey>(request.Ids);

        return await Repository.Collection
            .AsQueryable()
            .Where(p => keys.Contains(p.Id))
            .ProjectTo<TEntity, TReadModel>(Mapper)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
