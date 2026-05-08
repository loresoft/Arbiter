using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.Mapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler that processes an <see cref="EntityCreateCommand{TCreateModel, TReadModel}"/> by mapping the
/// create model to a new <typeparamref name="TEntity"/>, applying audit metadata, persisting the entity
/// via the specified <typeparamref name="TContext"/>, and returning the created entity as a <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TContext">The <see cref="DbContext"/> used to persist the entity.</typeparam>
/// <typeparam name="TEntity">The entity type being created. Must implement <see cref="IHaveIdentifier{TKey}"/> and have a parameterless constructor.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TCreateModel">The input model type containing the data for the new entity.</typeparam>
/// <typeparam name="TReadModel">The output model type returned after the entity is created.</typeparam>
public class EntityCreateCommandHandler<TContext, TEntity, TKey, TCreateModel, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateCommandHandler{TContext, TEntity, TKey, TCreateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to persist the entity.</param>
    /// <param name="mapper">The mapper used to convert between <typeparamref name="TCreateModel"/> and <typeparamref name="TEntity"/>, and to project to <typeparamref name="TReadModel"/>.</param>
    /// <param name="pipeline">An optional query pipeline applied when reading back the created entity.</param>
    public EntityCreateCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Maps <paramref name="request"/>.<c>Model</c> to a new <typeparamref name="TEntity"/>, then sets
    /// <see cref="ITrackCreated"/> and <see cref="ITrackUpdated"/> audit fields when supported.
    /// The entity is added to the <see cref="DbSet{TEntity}"/> and saved. The newly created entity
    /// is then read back and projected to <typeparamref name="TReadModel"/> before being returned.
    /// </remarks>
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

        var dbSet = DataContext
            .Set<TEntity>();

        // add to data set, id should be generated
        await dbSet
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        // save to database
        await DataContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        // convert to read model
        return await Read(entity.Id, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);
    }
}
