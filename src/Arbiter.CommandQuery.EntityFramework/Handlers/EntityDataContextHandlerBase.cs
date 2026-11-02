using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Notifications;
using Arbiter.Mapping;
using Arbiter.Mediation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A base handler for a request that requires the specified <see cref="DbContext"/>.
/// </summary>
/// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TEntity">The type of entity being operated on by the <see cref="DbContext"/></typeparam>
/// <typeparam name="TKey">The key type for the data context entity</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public abstract class EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, TRequest, TResponse>
    : DataContextHandlerBase<TContext, TRequest, TResponse>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
    where TRequest : IRequest<TResponse>
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
    /// Initializes a new instance of the <see cref="EntityDataContextHandlerBase{TContext, TEntity, TKey, TReadModel, TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to access the data store.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to map between entity and model types.</param>
    /// <param name="pipeline">An optional <see cref="IQueryPipeline"/> applied to queries executed by this handler and to entities before changes are saved.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="dataContext"/>, or <paramref name="mapper"/> is <see langword="null"/>.</exception>
    protected EntityDataContextHandlerBase(
        ILoggerFactory loggerFactory,
        TContext dataContext,
        IMapper mapper,
        IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <summary>
    /// Applies the registered <see cref="IEntityModifier{TEntity}"/> instances to <paramref name="entity"/>
    /// using the <see cref="DataContextHandlerBase{TContext, TRequest, TResponse}.Pipeline"/>.
    /// </summary>
    /// <param name="entity">The entity being saved.</param>
    /// <param name="operation">The change operation being performed on the entity.</param>
    /// <param name="pipelineName">The optional name of the pipeline used to resolve entity modifiers.</param>
    /// <param name="principal">The optional <see cref="IRequestPrincipal"/> representing the user executing the command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Called by the command handlers before changes are saved so that modifier changes are persisted in the same
    /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> call. Does nothing when no query pipeline is registered.
    /// When <paramref name="pipelineName"/> is specified, only modifiers registered for that named pipeline are applied.
    /// </remarks>
    protected virtual async ValueTask ApplyModifiers(
        TEntity entity,
        EntityChangeOperation operation,
        string? pipelineName = null,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (Pipeline is null)
            return;

        if (pipelineName.HasValue())
        {
            await Pipeline
                .ApplyModifiers(entity, DataContext, operation, pipelineName, principal, cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        await Pipeline
            .ApplyModifiers(entity, DataContext, operation, principal, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the <typeparamref name="TEntity"/> identified by <paramref name="key"/> from the data context
    /// and projects it to <typeparamref name="TReadModel"/>.
    /// </summary>
    /// <param name="key">The primary key of the entity to read.</param>
    /// <param name="pipelineName">The optional name of the query pipeline to apply when querying the entity.</param>
    /// <param name="principal">The optional <see cref="ClaimsPrincipal"/> used by the query pipeline for authorization or filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <typeparamref name="TReadModel"/> for the entity, or <see langword="null"/> if no matching entity is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    protected virtual async ValueTask<TReadModel?> Read(
        [NotNull] TKey key,
        string? pipelineName = null,
        ClaimsPrincipal? principal = null,
        CancellationToken cancellationToken = default)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"Read(); Context:{ContextName}, Entity:{EntityName}, Model:{ModelName}")
            .TagWithCallSite()
            .Where(p => Equals(p.Id, key));

        // apply query pipeline modifiers
        query = await query
            .ApplyPipeline(Pipeline, DataContext, pipelineName, principal, cancellationToken)
            .ConfigureAwait(false);

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(query);

        return await projected
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
