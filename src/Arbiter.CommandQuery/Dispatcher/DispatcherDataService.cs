using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Dispatcher;

/// <summary>
/// Provides a data service for dispatching common data requests to a data store.
/// </summary>
/// <remarks>
/// This service acts as an abstraction for sending queries and commands to a dispatcher, enabling operations
/// such as retrieving, creating, updating, and deleting entities in a consistent manner.
/// </remarks>
public class DispatcherDataService : IDispatcherDataService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DispatcherDataService"/> class.
    /// </summary>
    /// <param name="dispatcher">The dispatcher used to send requests.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dispatcher"/> is <see langword="null"/>.</exception>
    public DispatcherDataService(IDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        Dispatcher = dispatcher;
    }

    /// <inheritdoc/>
    public IDispatcher Dispatcher { get; }

    /// <inheritdoc/>
    public async ValueTask<TModel?> Get<TKey, TModel>(
        TKey id,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityIdentifierQuery<TKey, TModel>(user, id);
        command.Cache(cacheTime);

        return await Dispatcher
            .Send<EntityIdentifierQuery<TKey, TModel>, TModel>(command, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<TModel>> Get<TKey, TModel>(
        IEnumerable<TKey> ids,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityIdentifiersQuery<TKey, TModel>(user, [.. ids]);
        command.Cache(cacheTime);

        var result = await Dispatcher
            .Send<EntityIdentifiersQuery<TKey, TModel>, IReadOnlyList<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? [];
    }

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<TModel>> All<TModel>(
        string? sortField = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var query = new EntityQuery();
        var sort = EntitySort.Parse(sortField);
        if (sort is not null)
            query.Sort = [sort];

        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityPagedQuery<TModel>(user, query);
        command.Cache(cacheTime);

        var result = await Dispatcher
            .Send<EntityPagedQuery<TModel>, EntityPagedResult<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result?.Data ?? [];
    }

    /// <inheritdoc/>
    public async ValueTask<EntityPagedResult<TModel>> Page<TModel>(
        EntityQuery? entityQuery = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityPagedQuery<TModel>(user, entityQuery);
        command.Cache(cacheTime);

        var result = await Dispatcher
            .Send<EntityPagedQuery<TModel>, EntityPagedResult<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? EntityPagedResult<TModel>.Empty;
    }

    /// <inheritdoc/>
    public async ValueTask<EntityPagedResult<TModel>> Search<TModel>(
        string searchText,
        EntityQuery? entityQuery = null,
        CancellationToken cancellationToken = default)
        where TModel : class, ISupportSearch
    {
        entityQuery ??= new EntityQuery();

        var searchFilter = EntityFilterBuilder.CreateSearchFilter(TModel.SearchFields(), searchText);
        entityQuery.Filter = EntityFilterBuilder.CreateGroup(entityQuery.Filter, searchFilter);

        var sort = new EntitySort { Name = TModel.SortField() };
        entityQuery.Sort ??= [];
        entityQuery.Sort.Insert(0, sort);

        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityPagedQuery<TModel>(user, entityQuery);

        var result = await Dispatcher
            .Send<EntityPagedQuery<TModel>, EntityPagedResult<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? EntityPagedResult<TModel>.Empty;
    }

    /// <inheritdoc/>
    public async ValueTask<TReadModel?> Save<TKey, TUpdateModel, TReadModel>(
        TKey id,
        TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
        where TReadModel : class
        where TUpdateModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityUpdateCommand<TKey, TUpdateModel, TReadModel>(user, id, updateModel, upsert: true);

        return await Dispatcher
            .Send<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>(command, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<TReadModel?> Create<TCreateModel, TReadModel>(
        TCreateModel createModel,
        CancellationToken cancellationToken = default)
        where TReadModel : class
        where TCreateModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityCreateCommand<TCreateModel, TReadModel>(user, createModel);

        return await Dispatcher
            .Send<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>(command, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<TReadModel?> Update<TKey, TUpdateModel, TReadModel>(
        TKey id,
        TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
        where TReadModel : class
        where TUpdateModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityUpdateCommand<TKey, TUpdateModel, TReadModel>(user, id, updateModel);

        return await Dispatcher
            .Send<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>(command, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<TReadModel?> Delete<TKey, TReadModel>(
        TKey id,
        CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);
        var command = new EntityDeleteCommand<TKey, TReadModel>(user, id);

        return await Dispatcher
            .Send<EntityDeleteCommand<TKey, TReadModel>, TReadModel>(command, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ClaimsPrincipal?> GetUser(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<ClaimsPrincipal?>(default);
    }
}
