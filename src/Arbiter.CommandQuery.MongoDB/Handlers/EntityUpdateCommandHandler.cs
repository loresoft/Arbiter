using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles the update command for an entity in a MongoDB repository.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public class EntityUpdateCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateCommandHandler{TRepository, TEntity, TKey, TUpdateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The MongoDB repository.</param>
    /// <param name="mapper">The mapper for transforming models.</param>
    public EntityUpdateCommandHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the update command for the specified entity.
    /// </summary>
    /// <param name="request">The update command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated entity as a read model, or null if the entity was not found.</returns>
    protected override async ValueTask<TReadModel?> Process(EntityUpdateCommand<TKey, TUpdateModel, TReadModel> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await Repository
            .FindAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null && !request.Upsert)
            return default!;

        // create entity if not found
        if (entity == null)
        {
            entity = new TEntity { Id = request.Id };

            // apply create metadata
            if (entity is ITrackCreated createdModel)
            {
                createdModel.Created = request.Activated;
                createdModel.CreatedBy = request.ActivatedBy;
            }
        }

        // copy updates from model to entity
        Mapper.Map(request.Model, entity);

        // apply update metadata
        if (entity is ITrackUpdated updateEntity)
        {
            updateEntity.Updated = request.Activated;
            updateEntity.UpdatedBy = request.ActivatedBy;
        }

        // save updates
        var savedEntity = await Repository
            .UpsertAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        // return read model
        return Mapper.Map<TEntity, TReadModel>(savedEntity);
    }
}
