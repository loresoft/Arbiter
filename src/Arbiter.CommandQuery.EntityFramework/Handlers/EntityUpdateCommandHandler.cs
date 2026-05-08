using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.Mapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler that processes an <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}"/> by locating
/// the <typeparamref name="TEntity"/> in the specified <typeparamref name="TContext"/>, mapping the update
/// model onto it, applying audit metadata, saving changes, and returning the updated entity projected to
/// <typeparamref name="TReadModel"/>. Supports upsert semantics when <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}.Upsert"/> is <see langword="true"/>.
/// </summary>
/// <typeparam name="TContext">The <see cref="DbContext"/> used to access the data store.</typeparam>
/// <typeparam name="TEntity">The entity type being updated. Must implement <see cref="IHaveIdentifier{TKey}"/> and have a parameterless constructor.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TUpdateModel">The input model type containing the data for the update operation.</typeparam>
/// <typeparam name="TReadModel">The output model type returned after the entity is updated.</typeparam>
public class EntityUpdateCommandHandler<TContext, TEntity, TKey, TUpdateModel, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateCommandHandler{TContext, TEntity, TKey, TUpdateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to access the data store.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to map <typeparamref name="TUpdateModel"/> onto <typeparamref name="TEntity"/> and project to <typeparamref name="TReadModel"/>.</param>
    /// <param name="pipeline">An optional <see cref="IQueryPipeline"/> applied to queries executed by this handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="dataContext"/>, or <paramref name="mapper"/> is <see langword="null"/>.</exception>
    public EntityUpdateCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {

    }

    /// <inheritdoc />
    /// <remarks>
    /// Queries the <see cref="DbSet{TEntity}"/> for the entity matching <c>request.Id</c>, applying any
    /// optional query pipeline modifiers.
    /// <list type="bullet">
    /// <item><description>If the entity is not found and <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}.Upsert"/> is <see langword="false"/>, returns <see langword="null"/>.</description></item>
    /// <item><description>If the entity is not found and <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}.Upsert"/> is <see langword="true"/>, a new <typeparamref name="TEntity"/> is created with <see cref="ITrackCreated"/> audit fields set.</description></item>
    /// </list>
    /// The update model is mapped onto the entity, <see cref="ITrackUpdated"/> audit metadata is applied,
    /// changes are saved, and the entity is read back and projected to <typeparamref name="TReadModel"/> before being returned.
    /// </remarks>
    protected override async ValueTask<TReadModel?> Process(
        EntityUpdateCommand<TKey, TUpdateModel, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dbSet = DataContext
            .Set<TEntity>();

        var query = dbSet
            .TagWith($"EntityUpdateCommandHandler; Context:{ContextName}, Entity:{EntityName}, Model:{ModelName}")
            .TagWithCallSite();

        // apply query pipeline modifiers
        query = await query
            .ApplyPipeline(Pipeline, DataContext, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);

        var entity = await query
            .FirstOrDefaultAsync(x => Equals(x.Id, request.Id), cancellationToken)
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
        return await Read(entity.Id, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);
    }
}
