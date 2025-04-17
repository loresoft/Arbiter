// Ignore Spelling: Upsert

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles the upsert operation for an entity in the MongoDB repository.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
/// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public class EntityUpsertCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpsertCommandHandler{TRepository, TEntity, TKey, TUpdateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The repository.</param>
    /// <param name="mapper">The mapper.</param>
    public EntityUpsertCommandHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the upsert command for the entity.
    /// </summary>
    /// <param name="request">The upsert command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The read model of the upserted entity.</returns>
    protected override async ValueTask<TReadModel?> Process(EntityUpsertCommand<TKey, TUpdateModel, TReadModel> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await Repository
            .FindAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

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
