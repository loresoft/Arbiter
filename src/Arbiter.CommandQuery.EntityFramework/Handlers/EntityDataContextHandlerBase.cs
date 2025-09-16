using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

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
{

    private static readonly string _contextName = typeof(TContext).Name;
    private static readonly string _entityName = typeof(TEntity).Name;
    private static readonly string _modelName = typeof(TReadModel).Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDataContextHandlerBase{TContext, TEntity, TKey, TReadModel, TRequest, TResponse}"/> class.
    /// </summary>
    /// <inheritdoc/>
    protected EntityDataContextHandlerBase(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }

    /// <summary>
    /// Reads the entity from the data context.
    /// </summary>
    /// <param name="key">The entity key to read</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TReadModel"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null</exception>
    protected virtual async ValueTask<TReadModel?> Read([NotNull] TKey key, CancellationToken cancellationToken = default)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"Read(); Context:{_contextName}, Entity:{_entityName}, Model:{_modelName}")
            .TagWithCallSite()
            .Where(p => Equals(p.Id, key));

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(query);

        return await projected
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
