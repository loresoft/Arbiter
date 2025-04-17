using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles the deletion of an entity in a MongoDB repository.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned after deletion.</typeparam>
public class EntityDeleteCommandHandler<TRepository, TEntity, TKey, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityDeleteCommand<TKey, TReadModel>, TReadModel>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDeleteCommandHandler{TRepository, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The MongoDB repository.</param>
    /// <param name="mapper">The mapper for converting entities to read models.</param>
    public EntityDeleteCommandHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the delete command by finding the entity, applying metadata, and deleting it.
    /// </summary>
    /// <param name="request">The delete command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The read model of the deleted entity, or null if the entity was not found.</returns>
    protected override async ValueTask<TReadModel?> Process(
        EntityDeleteCommand<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await Repository
            .FindAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
            return default;

        // apply update metadata
        if (entity is ITrackUpdated updateEntity)
        {
            updateEntity.UpdatedBy = request.ActivatedBy;
            updateEntity.Updated = request.Activated;
        }

        TEntity savedEntity;

        // entity supports soft delete
        if (entity is ITrackDeleted deleteEntity)
        {
            deleteEntity.IsDeleted = true;

            savedEntity = await Repository
                .UpdateAsync(entity, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            // when history is tracked, need to update the entity with update metadata before deleting
            if (entity is ITrackHistory and ITrackUpdated)
            {
                savedEntity = await Repository
                   .UpdateAsync(entity, cancellationToken)
                   .ConfigureAwait(false);
            }
            else
            {
                savedEntity = entity;
            }

            await Repository
                .DeleteAsync(entity, cancellationToken)
                .ConfigureAwait(false);
        }

        // convert deleted entity to read model
        return Mapper.Map<TEntity, TReadModel>(savedEntity);
    }
}
