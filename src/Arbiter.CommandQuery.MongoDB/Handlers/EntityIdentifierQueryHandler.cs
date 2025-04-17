using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles queries to retrieve an entity by its identifier and map it to a read model.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public class EntityIdentifierQueryHandler<TRepository, TEntity, TKey, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityIdentifierQuery<TKey, TReadModel>, TReadModel>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifierQueryHandler{TRepository, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The repository instance.</param>
    /// <param name="mapper">The mapper instance.</param>
    public EntityIdentifierQueryHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper) : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the query to retrieve an entity by its identifier and map it to a read model.
    /// </summary>
    /// <param name="request">The query request containing the identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mapped read model or null if the entity is not found.</returns>
    protected override async ValueTask<TReadModel?> Process(
        EntityIdentifierQuery<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await Repository
            .FindAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        // convert entity to read model
        return Mapper.Map<TEntity, TReadModel>(entity);
    }
}
