using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Dispatcher;

public interface IDispatcherDataService
{
    IDispatcher Dispatcher { get; }

    ValueTask<TModel?> Get<TKey, TModel>(
        TKey id,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    ValueTask<IReadOnlyCollection<TModel>> Get<TKey, TModel>(
        IEnumerable<TKey> ids,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    ValueTask<IReadOnlyCollection<TModel>> All<TModel>(
        string? sortField = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    ValueTask<IReadOnlyCollection<TModel>> Select<TModel>(
        EntitySelect? entitySelect = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    ValueTask<EntityPagedResult<TModel>> Page<TModel>(
        EntityQuery? entityQuery = null,
        CancellationToken cancellationToken = default)
        where TModel : class;


    ValueTask<IEnumerable<TModel>> Search<TModel>(
        string searchText,
        EntityFilter? entityFilter = null,
        CancellationToken cancellationToken = default)
        where TModel : class, ISupportSearch;



    ValueTask<TReadModel?> Save<TKey, TUpdateModel, TReadModel>(
        TKey id,
        TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
        where TUpdateModel : class
        where TReadModel : class;

    ValueTask<TReadModel?> Create<TCreateModel, TReadModel>(
        TCreateModel createModel,
        CancellationToken cancellationToken = default)
        where TCreateModel : class
        where TReadModel : class;

    ValueTask<TReadModel?> Update<TKey, TUpdateModel, TReadModel>(
        TKey id,
        TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
        where TUpdateModel : class
        where TReadModel : class;

    ValueTask<TReadModel?> Delete<TKey, TReadModel>(
        TKey id,
        CancellationToken cancellationToken = default) where TReadModel : class;


    ValueTask<ClaimsPrincipal?> GetUser(CancellationToken cancellationToken = default);
}
