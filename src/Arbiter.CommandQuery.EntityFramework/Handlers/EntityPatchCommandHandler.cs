using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler for a request that patches an entity in the specified <see cref="DbContext"/>.
/// </summary>
/// <inheritdoc/>
public class EntityPatchCommandHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityPatchCommand<TKey, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPatchCommandHandler{TContext, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <inheritdoc/>
    public EntityPatchCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }

    /// <inheritdoc/>
    protected override async ValueTask<TReadModel?> Process(
        EntityPatchCommand<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dbSet = DataContext
            .Set<TEntity>();

        var keyValue = new object[] { request.Id };

        // find entity to update by message id, not model id
        var entity = await dbSet
            .FindAsync(keyValue, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
            return default!;

        // apply json patch to entity
        var jsonPatch = request.Patch;
        jsonPatch.ApplyTo(entity);

        // apply update metadata
        if (entity is ITrackUpdated updateEntity)
        {
            updateEntity.Updated = request.Activated;
            updateEntity.UpdatedBy = request.ActivatedBy;
        }

        await DataContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        // return read model
        return await Read(entity.Id, cancellationToken).ConfigureAwait(false);
    }
}
