using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler for a request that creates an entity in the specified <see cref="DbContext"/>.
/// </summary>
/// <inheritdoc/>
public class EntityCreateCommandHandler<TContext, TEntity, TKey, TCreateModel, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateCommandHandler{TContext, TEntity, TKey, TCreateModel, TReadModel}"/> class.
    /// </summary>
    /// <inheritdoc/>
    public EntityCreateCommandHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }

    /// <inheritdoc/>
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
        return await Read(entity.Id, cancellationToken)
            .ConfigureAwait(false);
    }
}
