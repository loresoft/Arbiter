using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Handlers;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// A base handler for a request that requires the specified repository.
/// </summary>
/// <typeparam name="TRepository">The type of <see cref="IMongoRepository{TEntity, TKey}"/>.</typeparam>
/// <typeparam name="TEntity">The type of entity being operated on by the repository</typeparam>
/// <typeparam name="TKey">The type of the key for the entity</typeparam>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public abstract class RepositoryHandlerBase<TRepository, TEntity, TKey, TRequest, TResponse>
    : RequestHandlerBase<TRequest, TResponse>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryHandlerBase{TRepository, TEntity, TKey, TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="repository">The <see cref="IMongoRepository{TEntity, TKey}"/> for this handler.</param>
    /// <param name="mapper"> The <see cref="IMapper"/> for this handler.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="repository"/> or <paramref name="mapper"/> are null</exception>
    protected RepositoryHandlerBase(
        ILoggerFactory loggerFactory,
        TRepository repository,
        IMapper mapper) : base(loggerFactory)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Gets the <see cref="IMongoRepository{TEntity, TKey}"/> for this handler.
    /// </summary>
    protected TRepository Repository { get; }

    /// <summary>
    /// Gets the <see cref="IMapper"/> for this handler.
    /// </summary>
    protected IMapper Mapper { get; }
}
