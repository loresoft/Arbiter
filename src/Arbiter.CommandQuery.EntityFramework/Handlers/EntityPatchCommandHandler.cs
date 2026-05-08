using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.Mapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler that processes an <see cref="EntityPatchCommand{TKey, TReadModel}"/> by locating the
/// <typeparamref name="TEntity"/> in the specified <typeparamref name="TContext"/>, applying a
/// <c>JsonPatchDocument</c> to it, setting audit metadata, saving changes, and returning the updated
/// entity projected to <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TContext">The <see cref="DbContext"/> used to access the data store.</typeparam>
/// <typeparam name="TEntity">The entity type being patched. Must implement <see cref="IHaveIdentifier{TKey}"/> and have a parameterless constructor.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TReadModel">The output model type returned after the patch is applied.</typeparam>
public class EntityPatchCommandHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityPatchCommand<TKey, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPatchCommandHandler{TContext, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to access the data store.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to project <typeparamref name="TEntity"/> to <typeparamref name="TReadModel"/>.</param>
    /// <param name="pipeline">An optional <see cref="IQueryPipeline"/> applied to queries executed by this handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="dataContext"/>, or <paramref name="mapper"/> is <see langword="null"/>.</exception>
    public EntityPatchCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Queries the <see cref="DbSet{TEntity}"/> for the entity matching <c>request.Id</c>, applying any
    /// optional query pipeline modifiers. Returns <see langword="null"/> if no entity is found.
    /// The JSON patch document from <c>request.Patch</c> is applied directly to the entity, followed by
    /// <see cref="ITrackUpdated"/> audit metadata. Changes are persisted, then the entity is read back
    /// and projected to <typeparamref name="TReadModel"/> before being returned.
    /// </remarks>
    protected override async ValueTask<TReadModel?> Process(
        EntityPatchCommand<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dbSet = DataContext
            .Set<TEntity>();

        var query = dbSet
            .TagWith($"EntityPatchCommandHandler; Context:{ContextName}, Entity:{EntityName}, Model:{ModelName}")
            .TagWithCallSite();

        // apply query pipeline modifiers
        query = await query
            .ApplyPipeline(Pipeline, DataContext, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);

        var entity = await query
            .FirstOrDefaultAsync(x => Equals(x.Id, request.Id), cancellationToken)
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
        return await Read(request.Id, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);
    }
}
