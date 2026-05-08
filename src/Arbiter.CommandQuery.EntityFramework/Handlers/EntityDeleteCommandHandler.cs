using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.Mapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler that processes an <see cref="EntityDeleteCommand{TKey, TReadModel}"/> by locating the
/// <typeparamref name="TEntity"/> in the specified <typeparamref name="TContext"/>, reading it back as a
/// <typeparamref name="TReadModel"/>, then performing a soft delete (via <see cref="ITrackDeleted"/>) or a
/// hard delete, and finally saving changes.
/// </summary>
/// <typeparam name="TContext">The <see cref="DbContext"/> used to access the data store.</typeparam>
/// <typeparam name="TEntity">The entity type being deleted. Must implement <see cref="IHaveIdentifier{TKey}"/> and have a parameterless constructor.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TReadModel">The output model type returned after the entity is deleted.</typeparam>
public class EntityDeleteCommandHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityDeleteCommand<TKey, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDeleteCommandHandler{TContext, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to access the data store.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to project <typeparamref name="TEntity"/> to <typeparamref name="TReadModel"/>.</param>
    /// <param name="pipeline">An optional <see cref="IQueryPipeline"/> applied to queries executed by this handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="dataContext"/>, or <paramref name="mapper"/> is <see langword="null"/>.</exception>
    public EntityDeleteCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Queries the <see cref="DbSet{TEntity}"/> for the entity matching <c>request.Id</c>, applying any
    /// optional query pipeline modifiers. If no entity is found, returns <see langword="null"/>.
    /// </para>
    /// <para>
    /// Before deletion, the entity is read back and projected to <typeparamref name="TReadModel"/> so the
    /// caller receives the state of the entity prior to deletion.
    /// </para>
    /// <para>
    /// The delete strategy is determined by the entity's implemented interfaces:
    /// <list type="bullet">
    /// <item><description>If the entity implements <see cref="ITrackDeleted"/>, a soft delete is performed by setting <c>IsDeleted = true</c>.</description></item>
    /// <item><description>Otherwise, the entity is hard-deleted via <see cref="DbSet{TEntity}.Remove"/>. If the entity also implements <see cref="ITrackHistory"/> and <see cref="ITrackUpdated"/>, update metadata is saved first to preserve history.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
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
