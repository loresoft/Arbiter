using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.Mapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A handler that processes an <see cref="EntityIdentifierQuery{TKey, TReadModel}"/> by reading a single
/// <typeparamref name="TEntity"/> by its primary key from the specified <typeparamref name="TContext"/>
/// and projecting it to a <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TContext">The <see cref="DbContext"/> used to access the data store.</typeparam>
/// <typeparam name="TEntity">The entity type being queried. Must implement <see cref="IHaveIdentifier{TKey}"/> and have a parameterless constructor.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TReadModel">The output model type returned after the entity is read.</typeparam>
public class EntityIdentifierQueryHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityIdentifierQuery<TKey, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifierQueryHandler{TContext, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> used to access the data store.</param>
    /// <param name="mapper">The <see cref="IMapper"/> used to project <typeparamref name="TEntity"/> to <typeparamref name="TReadModel"/>.</param>
    /// <param name="pipeline">An optional <see cref="IQueryPipeline"/> applied to queries executed by this handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="dataContext"/>, or <paramref name="mapper"/> is <see langword="null"/>.</exception>
    public EntityIdentifierQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper, IQueryPipeline? pipeline = null)
        : base(loggerFactory, dataContext, mapper, pipeline)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Delegates to <see cref="EntityDataContextHandlerBase{TContext, TEntity, TKey, TReadModel, TRequest, TResponse}.Read"/>
    /// using <c>request.Id</c>, <c>request.FilterName</c>, and <c>request.Principal</c>.
    /// Returns <see langword="null"/> if no matching entity is found.
    /// </remarks>
    protected override async ValueTask<TReadModel?> Process(
        EntityIdentifierQuery<TKey, TReadModel> request,
        CancellationToken cancellationToken = default)
    {
        return await Read(request.Id, request.FilterName, request.Principal, cancellationToken)
            .ConfigureAwait(false);
    }
}
