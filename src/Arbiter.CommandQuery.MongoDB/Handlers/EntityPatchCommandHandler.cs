using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

#if NET10_0_OR_GREATER
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations;
#else
using SystemTextJsonPatch;
using SystemTextJsonPatch.Operations;
#endif

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles the patching of an entity in the repository.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public class EntityPatchCommandHandler<TRepository, TEntity, TKey, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntityPatchCommand<TKey, TReadModel>, TReadModel>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPatchCommandHandler{TRepository, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The repository.</param>
    /// <param name="mapper">The mapper.</param>
    public EntityPatchCommandHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the patch command to update an entity.
    /// </summary>
    /// <param name="request">The patch command request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated read model or null if the entity was not found.</returns>
    protected override async ValueTask<TReadModel?> Process(EntityPatchCommand<TKey, TReadModel> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await Repository
            .FindAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
            return default;

        // apply json patch to entity
        var patchOperations = request.Patch;

        // convert to JsonPatchDocument
        var jsonPatch = new JsonPatchDocument();
        foreach (var operation in patchOperations)
        {
            Operation<TEntity> patchOperation = new(operation.Operation, operation.Path, operation.From, operation.Value);
            jsonPatch.Operations.Add(patchOperation);
        }

        jsonPatch.ApplyTo(entity);

        // apply update metadata
        if (entity is ITrackUpdated updateEntity)
        {
            updateEntity.Updated = request.Activated;
            updateEntity.UpdatedBy = request.ActivatedBy;
        }

        var savedEntity = await Repository
            .UpdateAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        // return read model
        return Mapper.Map<TEntity, TReadModel>(savedEntity);
    }
}
