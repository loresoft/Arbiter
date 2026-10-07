// Ignore Spelling: Typeahead

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Options;
using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Services;
using Arbiter.Dispatcher;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Arbiter.Components;

/// <summary>
/// A typeahead component that searches, loads, and looks up entities using an <see cref="IDispatcherDataService"/>.
/// </summary>
/// <typeparam name="TItem">The type of entity displayed in the typeahead.</typeparam>
/// <typeparam name="TValue">The type of the bound value; either the entity or its identifier.</typeparam>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
public class EntityTypeahead<TItem, TValue, TKey> : Typeahead<TItem, TValue>
     where TItem : class, IHaveIdentifier<TKey>, ISupportSearch
{
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTypeahead{TItem, TValue, TKey}"/> class.
    /// </summary>
    public EntityTypeahead()
    {
        SearchMethod = SearchQuery;
        ItemLoader = LoadItems;
        LookupMethod = LookupQuery;

        if (typeof(TValue) == typeof(TKey) || Nullable.GetUnderlyingType(typeof(TValue)) == typeof(TKey))
            ConvertMethod = ConvertIdentifier;
    }

    /// <summary>
    /// Gets or sets the notification service used to display errors.
    /// </summary>
    [Inject]
    public required INotificationService Notification { get; set; }

    /// <summary>
    /// Gets or sets the data service used to query entities.
    /// </summary>
    [Inject]
    public required IDispatcherDataService DataService { get; set; }

    /// <summary>
    /// Gets or sets the environment options used to provide the default <see cref="CacheTime"/>.
    /// </summary>
    [Inject]
    public required IOptions<EnvironmentOptions> EnvironmentOptions { get; set; }

    /// <summary>
    /// Gets or sets the sort expression used when loading items. Defaults to <c>TItem.SortField()</c> when not set.
    /// </summary>
    [Parameter]
    public string? Sort { get; set; }

    /// <summary>
    /// Gets or sets the filter applied when loading and searching items.
    /// </summary>
    [Parameter]
    public EntityFilter? Filter { get; set; }

    /// <summary>
    /// Gets or sets the duration to cache query results. Set to <see langword="null"/> to disable caching.
    /// </summary>
    /// <value>Defaults to <see cref="EnvironmentOptions.DefaultCacheTime"/>.</value>
    [Parameter]
    public TimeSpan? CacheTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether items should be preloaded. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool Preload { get; set; } = true;

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {
        // apply the default before parameters are set so an explicit value, including null, takes precedence
        if (!_initialized)
        {
            _initialized = true;
            CacheTime = EnvironmentOptions.Value.DefaultCacheTime;
        }

        return base.SetParametersAsync(parameters);
    }

    /// <summary>
    /// Loads the initial list of items using the configured <see cref="Filter"/> and <see cref="Sort"/>.
    /// </summary>
    /// <returns>The loaded items, or an empty collection if loading is disabled or fails.</returns>
    protected async Task<IEnumerable<TItem>> LoadItems()
    {
        if (!Preload)
            return [];

        try
        {
            var query = new EntityQuery { Filter = Filter };
            query.AddSort(Sort ?? TItem.SortField());

            var result = await DataService.Page<TItem>(query, CacheTime);

            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
            return [];
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Looks up the entity for the specified bound value.
    /// </summary>
    /// <param name="value">The bound value; either an entity or its identifier.</param>
    /// <returns>The matching entity, or <see langword="null"/> if not found.</returns>
    protected async Task<TItem?> LookupQuery(TValue? value)
    {
        if (value is null)
            return null;

        if (value is TItem item)
            return item;

        if (value is not TKey id)
            return default;

        if (EqualityComparer<TKey>.Default.Equals(id, default))
            return default;

        try
        {
            return await DataService.Get<TKey, TItem>(id, CacheTime);
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
            return default;
        }
    }

    /// <summary>
    /// Searches for entities matching the specified text.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>The matching items, or an empty collection if the search fails.</returns>
    protected async Task<IEnumerable<TItem>> SearchQuery(string searchText)
    {
        try
        {
            var query = new EntityQuery { Filter = Filter };
            var result = await DataService.Search<TItem>(searchText, query);

            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
            return [];
        }
    }

    /// <summary>
    /// Converts the specified entity to its identifier value.
    /// </summary>
    /// <param name="item">The entity to convert.</param>
    /// <returns>The entity identifier, or <see langword="default"/> if <paramref name="item"/> is <see langword="null"/>.</returns>
    protected static TValue? ConvertIdentifier(TItem? item)
    {
        if (item is null)
            return default;

        if (item is IHaveIdentifier<TKey> identifiable)
            return (TValue)(object)identifiable.Id;

        return default;
    }
}
