using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;
using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Client;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// An <see cref="IDispatcherDataService"/> that returns preconfigured results and records the calls it received.
/// </summary>
internal sealed class FakeDataService : IDispatcherDataService
{
    /// <summary>
    /// Gets or sets the result returned by <see cref="Page{TModel}"/>.
    /// </summary>
    public object? PageResult { get; set; }

    /// <summary>
    /// Gets or sets the exception thrown by <see cref="Page{TModel}"/> instead of returning a result.
    /// </summary>
    public Exception? PageException { get; set; }

    /// <summary>
    /// Gets the queries passed to <see cref="Page{TModel}"/>.
    /// </summary>
    public List<EntityQuery?> PageQueries { get; } = [];

    /// <summary>
    /// Gets the identifiers passed to <see cref="Delete{TKey, TReadModel}"/>.
    /// </summary>
    public List<object?> DeletedIds { get; } = [];

    /// <summary>
    /// Gets or sets the model returned by <see cref="Get{TKey, TModel}(TKey, TimeSpan?, CancellationToken)"/>.
    /// </summary>
    public object? GetResult { get; set; }

    /// <summary>
    /// Gets or sets the exception thrown by <see cref="Get{TKey, TModel}(TKey, TimeSpan?, CancellationToken)"/>.
    /// </summary>
    public Exception? GetException { get; set; }

    /// <summary>
    /// Gets the identifiers passed to <see cref="Get{TKey, TModel}(TKey, TimeSpan?, CancellationToken)"/>.
    /// </summary>
    public List<object?> RequestedIds { get; } = [];

    /// <summary>
    /// Gets or sets the model returned by <see cref="Save{TKey, TUpdateModel, TReadModel}"/>.
    /// </summary>
    public object? SaveResult { get; set; }

    /// <summary>
    /// Gets or sets the exception thrown by <see cref="Save{TKey, TUpdateModel, TReadModel}"/>.
    /// </summary>
    public Exception? SaveException { get; set; }

    /// <summary>
    /// Gets the update models passed to <see cref="Save{TKey, TUpdateModel, TReadModel}"/>.
    /// </summary>
    public List<object?> SavedModels { get; } = [];

    public IDispatcher Dispatcher => throw new NotSupportedException();

    public ValueTask<EntityPagedResult<TModel>> Page<TModel>(
        EntityQuery? entityQuery = null,
        TimeSpan? cacheTime = null,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        PageQueries.Add(entityQuery);

        cancellationToken.ThrowIfCancellationRequested();

        if (PageException != null)
            throw PageException;

        var result = PageResult as EntityPagedResult<TModel> ?? new EntityPagedResult<TModel>();
        return ValueTask.FromResult(result);
    }

    public ValueTask<TReadModel?> Delete<TKey, TReadModel>(
        TKey id,
        CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        DeletedIds.Add(id);

        return ValueTask.FromResult<TReadModel?>(null);
    }

    public ValueTask<ClaimsPrincipal?> GetUser(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<ClaimsPrincipal?>(null);

    public ValueTask<TModel?> Get<TKey, TModel>(TKey id, TimeSpan? cacheTime = null, CancellationToken cancellationToken = default)
        where TModel : class
    {
        RequestedIds.Add(id);

        cancellationToken.ThrowIfCancellationRequested();

        if (GetException != null)
            throw GetException;

        return ValueTask.FromResult(GetResult as TModel);
    }

    public ValueTask<TModel?> GetKey<TModel>(Guid key, TimeSpan? cacheTime = null, CancellationToken cancellationToken = default)
        where TModel : class
        => throw new NotSupportedException();

    public ValueTask<IReadOnlyList<TModel>> Get<TKey, TModel>(IEnumerable<TKey> ids, TimeSpan? cacheTime = null, CancellationToken cancellationToken = default)
        where TModel : class
        => throw new NotSupportedException();

    public ValueTask<IReadOnlyList<TModel>> All<TModel>(string? sortField = null, TimeSpan? cacheTime = null, CancellationToken cancellationToken = default)
        where TModel : class
        => throw new NotSupportedException();

    public ValueTask<EntityPagedResult<TModel>> Search<TModel>(string searchText, EntityQuery? entityQuery = null, CancellationToken cancellationToken = default)
        where TModel : class, ISupportSearch
        => throw new NotSupportedException();

    public ValueTask<TReadModel?> Save<TKey, TUpdateModel, TReadModel>(TKey id, TUpdateModel updateModel, CancellationToken cancellationToken = default)
        where TUpdateModel : class
        where TReadModel : class
    {
        SavedModels.Add(updateModel);

        cancellationToken.ThrowIfCancellationRequested();

        if (SaveException != null)
            throw SaveException;

        return ValueTask.FromResult(SaveResult as TReadModel);
    }

    public ValueTask<TReadModel?> Create<TCreateModel, TReadModel>(TCreateModel createModel, CancellationToken cancellationToken = default)
        where TCreateModel : class
        where TReadModel : class
        => throw new NotSupportedException();

    public ValueTask<TReadModel?> Update<TKey, TUpdateModel, TReadModel>(TKey id, TUpdateModel updateModel, CancellationToken cancellationToken = default)
        where TUpdateModel : class
        where TReadModel : class
        => throw new NotSupportedException();
}
