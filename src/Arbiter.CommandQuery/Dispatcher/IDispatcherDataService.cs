using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Filters;
using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Dispatcher;

/// <summary>
/// A data service for dispatching common data requests to a data store.
/// </summary>
public interface IDispatcherDataService
{
    /// <summary>
    /// Gets the dispatcher used to send requests.
    /// </summary>
    IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the model for the specified identifier key from the data store.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="id">The identifier key to get</param>
    /// <param name="cacheTime">Optional time to cache the results</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the <typeparamref name="TModel"/> for the specified identifier key</returns>
    ValueTask<TModel?> Get<TKey, TModel>(
        TKey id,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    /// <summary>
    /// Gets a list of models for the specified identifier keys from the data store.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="ids">The identifier keys to get</param>
    /// <param name="cacheTime">Optional time to cache the results</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning a list <typeparamref name="TModel"/> for the specified identifier keys</returns>
    ValueTask<IReadOnlyCollection<TModel>> Get<TKey, TModel>(
        IEnumerable<TKey> ids,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    /// <summary>
    /// Gets all models of the specified type from the data store.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="sortField">The field or property name to sort by</param>
    /// <param name="cacheTime">Optional time to cache the results</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning a list <typeparamref name="TModel"/></returns>
    ValueTask<IReadOnlyCollection<TModel>> All<TModel>(
        string? sortField = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    /// <summary>
    /// Gets models based on the specified <see cref="EntitySelect"/> query from the data store.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="entitySelect"></param>
    /// <param name="cacheTime">Optional time to cache the results</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning a list <typeparamref name="TModel"/></returns>
    ValueTask<IReadOnlyCollection<TModel>> Select<TModel>(
        EntitySelect? entitySelect = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    /// <summary>
    /// Gets page of models based on the specified <see cref="EntityQuery"/> query.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="entityQuery"></param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning a paged result for <typeparamref name="TModel"/></returns>
    ValueTask<EntityPagedResult<TModel>> Page<TModel>(
        EntityQuery? entityQuery = null,
        CancellationToken cancellationToken = default)
        where TModel : class;

    /// <summary>
    /// Searches the data store for models based on the specified search text and optional filter. <typeparamref name="TModel"/> must implement <see cref="ISupportSearch"/>
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="searchText">The search text to search for</param>
    /// <param name="entityFilter">Optional additional filter for search</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning a list <typeparamref name="TModel"/></returns>
    ValueTask<IEnumerable<TModel>> Search<TModel>(
        string searchText,
        EntityFilter? entityFilter = null,
        CancellationToken cancellationToken = default)
        where TModel : class, ISupportSearch;


    /// <summary>
    /// Saves the specified <paramref name="updateModel"/> in the data store with the specified <paramref name="id"/>.
    /// If the model already exists, it will be updated; otherwise it will be created.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="id">The identifier to apply the update to.</param>
    /// <param name="updateModel">The update model to apply.</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning <typeparamref name="TReadModel"/></returns>
    ValueTask<TReadModel?> Save<TKey, TUpdateModel, TReadModel>(
        TKey id,
        TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
        where TUpdateModel : class
        where TReadModel : class;

    /// <summary>
    /// Creates the specified <paramref name="createModel"/> in the data store.
    /// </summary>
    /// <typeparam name="TCreateModel">The type of the create model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="createModel">The model being created</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning <typeparamref name="TReadModel"/></returns>
    ValueTask<TReadModel?> Create<TCreateModel, TReadModel>(
        TCreateModel createModel,
        CancellationToken cancellationToken = default)
        where TCreateModel : class
        where TReadModel : class;

    /// <summary>
    /// Updates the specified <paramref name="updateModel"/> in the data store with the specified <paramref name="id"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TUpdateModel">The type of the update model.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="id">The identifier to apply the update to.</param>
    /// <param name="updateModel">The model being updated</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning <typeparamref name="TReadModel"/></returns>
    ValueTask<TReadModel?> Update<TKey, TUpdateModel, TReadModel>(
        TKey id,
        TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
        where TUpdateModel : class
        where TReadModel : class;

    /// <summary>
    /// Deletes the model with the specified <paramref name="id"/> from the data store.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    /// <param name="id">The identifier to delete.</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning <typeparamref name="TReadModel"/></returns>
    ValueTask<TReadModel?> Delete<TKey, TReadModel>(
        TKey id,
        CancellationToken cancellationToken = default) where TReadModel : class;

    /// <summary>
    /// Gets the current user from the request.
    /// </summary>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns></returns>
    ValueTask<ClaimsPrincipal?> GetUser(CancellationToken cancellationToken = default);
}
