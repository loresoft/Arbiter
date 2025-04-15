using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Dispatcher;

/// <summary>
/// A data service for dispatching common data requests to a data store.
/// </summary>
public class DispatcherDataService : IDispatcherDataService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DispatcherDataService"/> class.
    /// </summary>
    /// <param name="dispatcher">The dispatcher used to send requests</param>
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
    public async ValueTask<IReadOnlyCollection<TModel>> Get<TKey, TModel>(
        IEnumerable<TKey> ids,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityIdentifiersQuery<TKey, TModel>(user, [.. ids]);
        command.Cache(cacheTime);

        var result = await Dispatcher
            .Send<EntityIdentifiersQuery<TKey, TModel>, IReadOnlyCollection<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? [];
    }

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyCollection<TModel>> All<TModel>(
        string? sortField = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var filter = new EntityFilter();
        var sort = EntitySort.Parse(sortField);

        var select = new EntitySelect(filter, sort);

        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntitySelectQuery<TModel>(user, select);
        command.Cache(cacheTime);

        var result = await Dispatcher
            .Send<EntitySelectQuery<TModel>, IReadOnlyCollection<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? [];
    }

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyCollection<TModel>> Select<TModel>(
        EntitySelect? entitySelect = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntitySelectQuery<TModel>(user, entitySelect);
        command.Cache(cacheTime);

        var result = await Dispatcher
            .Send<EntitySelectQuery<TModel>, IReadOnlyCollection<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? [];
    }

    /// <inheritdoc/>
    public async ValueTask<EntityPagedResult<TModel>> Page<TModel>(
        EntityQuery? entityQuery = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntityPagedQuery<TModel>(user, entityQuery);

        var result = await Dispatcher
            .Send<EntityPagedQuery<TModel>, EntityPagedResult<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? new EntityPagedResult<TModel>();
    }


    /// <inheritdoc/>
    public async ValueTask<IEnumerable<TModel>> Search<TModel>(
        string searchText,
        EntityFilter? entityFilter = null,
        CancellationToken cancellationToken = default)
        where TModel : class, ISupportSearch
    {
        var searchFilter = EntityFilterBuilder.CreateSearchFilter(TModel.SearchFields(), searchText);
        var sort = new EntitySort { Name = TModel.SortField() };

        var groupFilter = EntityFilterBuilder.CreateGroup(entityFilter, searchFilter);

        var select = new EntitySelect(groupFilter, sort);

        var user = await GetUser(cancellationToken).ConfigureAwait(false);

        var command = new EntitySelectQuery<TModel>(user, select);

        var result = await Dispatcher
            .Send<EntitySelectQuery<TModel>, IReadOnlyCollection<TModel>>(command, cancellationToken)
            .ConfigureAwait(false);

        return result ?? [];
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

        var command = new EntityUpsertCommand<TKey, TUpdateModel, TReadModel>(user, id, updateModel);

        return await Dispatcher
            .Send<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>(command, cancellationToken)
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
