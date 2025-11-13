using System.Security.Principal;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// Handles queries that retrieve entities from Entity Framework by their globally unique alternate key.
/// </summary>
/// <typeparam name="TContext">The type of <see cref="DbContext"/> used to access the database.</typeparam>
/// <typeparam name="TEntity">The type of entity to retrieve, which must implement <see cref="IHaveKey"/>.</typeparam>
/// <typeparam name="TReadModel">The type of read model to project the entity into.</typeparam>
/// <remarks>
/// This handler processes <see cref="EntityKeyQuery{TReadModel}"/> requests by querying the database
/// for an entity matching the specified <see cref="Guid"/> alternate key and projecting it to the read model type.
/// The query is executed with no tracking for improved performance in read-only scenarios.
/// </remarks>
public class EntityKeyQueryHandler<TContext, TEntity, TReadModel>
    : DataContextHandlerBase<TContext, EntityKeyQuery<TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveKey, new()
{
    /// <summary>
    /// Represents the name of the entity type associated with <typeparamref name="TEntity"/>.
    /// </summary>
    protected static readonly string EntityName = typeof(TEntity).Name;

    /// <summary>
    /// Represents the name of the read model type associated with <typeparamref name="TReadModel"/>.
    /// </summary>
    protected static readonly string ModelName = typeof(TReadModel).Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityKeyQueryHandler{TContext, TEntity, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <see cref="DbContext"/> used to access the database.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to project entities to read models.</param>
    /// <param name="pipeline">The optional <see cref="IQueryPipeline"/> to apply to the query.</param>
    public EntityKeyQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <summary>
    /// Processes the query to retrieve an entity by its globally unique alternate key.
    /// </summary>
    /// <param name="request">The <see cref="EntityKeyQuery{TReadModel}"/> containing the alternate key to search for.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
    /// containing the projected read model if found; otherwise, <see langword="null"/>.
    /// </returns>
    protected override async ValueTask<TReadModel?> Process(
        EntityKeyQuery<TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        var key = request.Key;
        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"Process(); Context:{ContextName}, Entity:{EntityName}, Model:{ModelName}")
            .TagWithCallSite()
            .Where(p => p.Key == key);

        // apply query pipeline modifiers
        query = await query
            .ApplyPipeline(Pipeline, DataContext, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(query);

        return await projected
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
