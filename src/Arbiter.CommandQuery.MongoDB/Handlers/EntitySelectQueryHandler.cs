using System.Linq.Dynamic.Core;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;

using Microsoft.Extensions.Logging;

using MongoDB.Abstracts;
using MongoDB.Driver.Linq;

namespace Arbiter.CommandQuery.MongoDB.Handlers;

/// <summary>
/// Handles queries for selecting entities from a MongoDB repository.
/// </summary>
/// <typeparam name="TRepository">The type of the repository.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public class EntitySelectQueryHandler<TRepository, TEntity, TKey, TReadModel>
    : RepositoryHandlerBase<TRepository, TEntity, TKey, EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>
    where TRepository : IMongoRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelectQueryHandler{TRepository, TEntity, TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="repository">The MongoDB repository.</param>
    /// <param name="mapper">The mapper for converting entities to read models.</param>
    public EntitySelectQueryHandler(ILoggerFactory loggerFactory, TRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {
    }

    /// <summary>
    /// Processes the query to retrieve a collection of read models.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of read models.</returns>
    protected override async ValueTask<IReadOnlyCollection<TReadModel>?> Process(EntitySelectQuery<TReadModel> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = Repository.All();

        // Build query from filter
        query = await BuildQuery(request, query).ConfigureAwait(false);

        // Page the query and convert to read model
        return await QueryList(request, query, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the query based on the provided request.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="query">The initial query.</param>
    /// <returns>The modified query as a ValueTask.</returns>
    protected virtual ValueTask<IQueryable<TEntity>> BuildQuery(EntitySelectQuery<TReadModel> request, IQueryable<TEntity> query)
    {
        var entitySelect = request?.Select;

        // Build query from filter
        if (entitySelect?.Filter != null)
            query = query.Filter(entitySelect.Filter);

        // Add raw query
        if (!string.IsNullOrEmpty(entitySelect?.Query))
            query = query.Where(entitySelect.Query);

        return ValueTask.FromResult(query);
    }

    /// <summary>
    /// Executes the query and maps the results to read models.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of read models.</returns>
    protected virtual async ValueTask<IReadOnlyCollection<TReadModel>> QueryList(EntitySelectQuery<TReadModel> request, IQueryable<TEntity> query, CancellationToken cancellationToken)
    {
        return await query
            .Sort(request?.Select?.Sort)
            .ProjectTo<TEntity, TReadModel>(Mapper)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
