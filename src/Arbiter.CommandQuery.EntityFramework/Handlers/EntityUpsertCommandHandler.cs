using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler for a request to update or insert of an entity in the specified <see cref="DbContext"/>.
/// </summary>
/// <inheritdoc/>
public class EntityUpsertCommandHandler<TContext, TEntity, TKey, TUpdateModel, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpsertCommandHandler{TContext, TEntity, TKey, TUpdateModel, TReadModel}"/> class.
    /// </summary>
    /// <inheritdoc/>
    public EntityUpsertCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {

    }

    /// <inheritdoc/>
    protected override async ValueTask<TReadModel?> Process(
        EntityUpsertCommand<TKey, TUpdateModel, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dbSet = DataContext
            .Set<TEntity>();

        // don't query if default value
        var entity = !EqualityComparer<TKey>.Default.Equals(request.Id, default)
            ? await dbSet.FindAsync([request.Id], cancellationToken).ConfigureAwait(false)
            : default;

        // create entity if not found
        if (entity == null)
        {
            entity = new TEntity();
            entity.Id = request.Id;

            // apply create metadata
            if (entity is ITrackCreated createdModel)
            {
                createdModel.Created = request.Activated;
                createdModel.CreatedBy = request.ActivatedBy;
            }

            await dbSet
                .AddAsync(entity, cancellationToken)
                .ConfigureAwait(false);
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
        await DataContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        // return read model
        return await Read(entity.Id, cancellationToken).ConfigureAwait(false);
    }
}
