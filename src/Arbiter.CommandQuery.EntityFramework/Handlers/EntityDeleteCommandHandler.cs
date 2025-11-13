using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler for a request that deletes an entity in the specified <see cref="DbContext"/>.
/// </summary>
/// <inheritdoc/>
public class EntityDeleteCommandHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityDeleteCommand<TKey, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDeleteCommandHandler{TContext, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <inheritdoc/>
    public EntityDeleteCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <inheritdoc/>
    protected override async ValueTask<TReadModel?> Process(
        EntityDeleteCommand<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dbSet = DataContext
            .Set<TEntity>();

        var query = dbSet
            .TagWith($"EntityDeleteCommandHandler; Context:{ContextName}, Entity:{EntityName}, Model:{ModelName}")
            .TagWithCallSite();

        // apply query pipeline modifiers
        query = await query
            .ApplyPipeline(Pipeline, DataContext, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);

        var entity = await query
            .FirstOrDefaultAsync(x => Equals(x.Id, request.Id), cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
            return default;

        // read the entity before deleting it
        var readModel = await Read(request.Id, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);

        // apply update metadata
        if (entity is ITrackUpdated updateEntity)
        {
            updateEntity.UpdatedBy = request.ActivatedBy;
            updateEntity.Updated = request.Activated;
        }

        // entity supports soft delete
        if (entity is ITrackDeleted deleteEntity)
        {
            deleteEntity.IsDeleted = true;
        }
        else
        {
            // when history is tracked, need to update the entity with update metadata before deleting
            if (entity is ITrackHistory and ITrackUpdated)
            {
                await DataContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            // delete the entity
            dbSet.Remove(entity);
        }

        await DataContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        // convert deleted entity to read model
        return readModel;
    }

}
