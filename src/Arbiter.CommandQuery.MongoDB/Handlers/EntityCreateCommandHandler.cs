using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles the creation of an entity in the repository.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
/// <typeparam name="TCreateModel">The type of the model used to create the entity.</typeparam>
/// <typeparam name="TReadModel">The type of the model used to read the entity.</typeparam>
public class EntityCreateCommandHandler<TRepository, TEntity, TKey, TCreateModel, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateCommandHandler{TRepository, TEntity, TKey, TCreateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The repository instance.</param>
    /// <param name="mapper">The mapper instance.</param>
    public EntityCreateCommandHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the creation command by creating a new entity in the repository.
    /// </summary>
    /// <param name="request">The creation command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created entity mapped to the read model.</returns>
    protected override async ValueTask<TReadModel?> Process(
        EntityCreateCommand<TCreateModel, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // create new entity from model
        var entity = Mapper.Map<TCreateModel, TEntity>(request.Model);

        // apply create metadata
        if (entity is ITrackCreated createdModel)
        {
            createdModel.Created = request.Activated;
            createdModel.CreatedBy = request.ActivatedBy;
        }

        // apply update metadata
        if (entity is ITrackUpdated updateEntity)
        {
            updateEntity.Updated = request.Activated;
            updateEntity.UpdatedBy = request.ActivatedBy;
        }

        var result = await Repository
            .InsertAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        // convert to read model
        return Mapper.Map<TEntity, TReadModel>(result);
    }
}
