using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles queries that retrieve entities from MongoDB by their globally unique alternate key.
/// </summary>
/// <typeparam name="TRepository">The type of <see cref="IMongoRepository{TEntity, TKey}"/> used to access the MongoDB collection.</typeparam>
/// <typeparam name="TEntity">The type of entity to retrieve, which must implement both <see cref="IHaveIdentifier{TKey}"/> and <see cref="IHaveKey"/>.</typeparam>
/// <typeparam name="TKey">The type of the primary key for the entity.</typeparam>
/// <typeparam name="TReadModel">The type of read model to map the entity into.</typeparam>
/// <remarks>
/// This handler processes <see cref="EntityKeyQuery{TReadModel}"/> requests by querying the MongoDB repository
/// for an entity matching the specified <see cref="Guid"/> alternate key and mapping it to the read model type.
/// The alternate key is independent of the primary key and provides a globally unique identifier for the entity.
/// </remarks>
public class EntityKeyQueryHandler<TRepository, TEntity, TKey, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityKeyQuery<TReadModel>, TReadModel>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, IHaveKey, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityKeyQueryHandler{TRepository, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="repository">The <see cref="IMongoRepository{TEntity, TKey}"/> used to access the MongoDB collection.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to map entities to read models.</param>
    public EntityKeyQueryHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the query to retrieve an entity by its globally unique alternate key.
    /// </summary>
    /// <param name="request">The <see cref="EntityKeyQuery{TReadModel}"/> containing the alternate key to search for.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing the mapped read model if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    protected override async ValueTask<TReadModel?> Process(
        EntityKeyQuery<TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await Repository
            .FindOneAsync(q => q.Key == request.Key, cancellationToken)
            .ConfigureAwait(false);

        // convert entity to read model
        return Mapper.Map<TEntity, TReadModel>(entity);
    }
}
