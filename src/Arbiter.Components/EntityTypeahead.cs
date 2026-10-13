// Ignore Spelling: Typeahead

using System.Reflection;

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
public class EntityTypeahead<TItem, TValue> : Typeahead<TItem, TValue>
     where TItem : class, ISupportSearch
{
    private static readonly bool _isIdentifierValue = IsIdentifierValue();

    // only built when the bound value is the entity identifier; otherwise lookups never query
    private static readonly Func<IDispatcherDataService, TValue, TimeSpan?, ValueTask<TItem?>>? _identifierLookup
        = _isIdentifierValue ? CreateIdentifierLookup() : null;

    private bool _disabledParameter;
    private bool? _hasPermission;
    private Task? _permissionTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTypeahead{TItem, TValue}"/> class.
    /// </summary>
    public EntityTypeahead()
    {
        SearchMethod = SearchQuery;
        ItemLoader = LoadItems;
        LookupMethod = LookupQuery;

        if (_isIdentifierValue)
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

    /// <summary>
    /// Gets or sets the function used to determine whether the current user has permission to use the control.
    /// When it returns <see langword="false"/>, the control is disabled.
    /// </summary>
    /// <remarks>
    /// The function is evaluated once and the result is cached for the lifetime of the component. The control is
    /// disabled while the function is pending, and remains disabled if it throws. An explicit
    /// <see cref="Typeahead{TItem, TValue}.Disabled"/> value of <see langword="true"/> takes priority and skips the check.
    /// </remarks>
    [Parameter]
    public Func<Task<bool>>? PermissionResolver { get; set; }

    /// <summary>
    /// Gets a value indicating whether the control is disabled, either explicitly through
    /// <see cref="Typeahead{TItem, TValue}.Disabled"/> or because the permission check has not granted access.
    /// </summary>
    protected bool IsDisabled => _disabledParameter || _hasPermission != true;

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {
        // capture the caller supplied value; the base Disabled property is overwritten with the effective state
        if (parameters.TryGetValue<bool>(nameof(Disabled), out var disabled))
            _disabledParameter = disabled;

        return base.SetParametersAsync(parameters);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // default the Disabled property based on the current permission state
        Disabled = IsDisabled;
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // an explicit Disabled="true" wins, and a resolved permission is cached for the component lifetime
        if (_disabledParameter || _hasPermission.HasValue)
            return;

        // reuse the in-flight check so parameter updates while the resolver is pending don't invoke it again
        _permissionTask ??= ResolvePermission();
        await _permissionTask;

        // apply the resolved permission; OnParametersSet ran before it completed and left the control disabled
        Disabled = IsDisabled;
    }

    /// <summary>
    /// Determines whether the current user has permission to use the control.
    /// </summary>
    /// <returns>
    /// The result of <see cref="PermissionResolver"/>, or <see langword="true"/> when no resolver is set.
    /// </returns>
    protected virtual Task<bool> HasPermission()
    {
        if (PermissionResolver is null)
            return Task.FromResult(true);

        return PermissionResolver();
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

            var cacheTime = CacheTime ?? EnvironmentOptions.Value.DefaultCacheTime;
            var result = await DataService.Page<TItem>(query, cacheTime);

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
    /// <remarks>
    /// A query is only sent when <typeparamref name="TValue"/> matches the key type of <typeparamref name="TItem"/>.
    /// Nullable identifier values are queried using their underlying key type.
    /// </remarks>
    protected async Task<TItem?> LookupQuery(TValue? value)
    {
        // nothing bound, including a null nullable identifier
        if (value is null)
            return null;

        // bound directly to the entity, no lookup needed
        if (value is TItem item)
            return item;

        // only query when the value is the entity identifier
        if (_identifierLookup is null)
            return default;

        try
        {
            var cacheTime = CacheTime ?? EnvironmentOptions.Value.DefaultCacheTime;

            // queries with the underlying key type so nullable values match the registered handler
            return await _identifierLookup(DataService, value, cacheTime);
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

        if (item is IHaveIdentifier<TValue> identifiableValue)
            return identifiableValue.Id;

        if (item is IHaveIdentifier identifiable)
            return (TValue)identifiable.GetIdentifier();

        return default;
    }

    private async Task ResolvePermission()
    {
        try
        {
            _hasPermission = await HasPermission();
        }
        catch (Exception ex)
        {
            _hasPermission = false;
            Notification.ShowError(ex);
        }
    }

    private static bool IsIdentifierValue()
    {
        // compare against the underlying type so int? matches an IHaveIdentifier<int> entity
        var valueType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        // the value is the identifier only when TItem implements IHaveIdentifier<T> with a matching key type;
        // otherwise the bound value may be something else entirely and must not be treated as a key
        return typeof(TItem)
            .GetInterfaces()
            .Any(i => i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IHaveIdentifier<>)
                && i.GetGenericArguments()[0] == valueType);
    }

    private static Func<IDispatcherDataService, TValue, TimeSpan?, ValueTask<TItem?>> CreateIdentifierLookup()
    {
        // non-nullable identifiers, such as int or Guid, already match the registered handler key type
        var keyType = Nullable.GetUnderlyingType(typeof(TValue));
        if (keyType is null)
            return LookupIdentifier<TValue>;

        // nullable identifiers, such as int?, must query with the underlying key type (int);
        // the key type is only known at runtime, so close LookupIdentifier<TKey> over it with reflection
        var method = typeof(EntityTypeahead<TItem, TValue>)
            .GetMethod(nameof(LookupIdentifier), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(keyType);

        // compile to a delegate once per closed type so each lookup avoids reflection invoke costs
        return method.CreateDelegate<Func<IDispatcherDataService, TValue, TimeSpan?, ValueTask<TItem?>>>();
    }

    private static ValueTask<TItem?> LookupIdentifier<TKey>(
        IDispatcherDataService dataService,
        TValue id,
        TimeSpan? cacheTime)
    {
        // A non-null Nullable<T> boxes as T, allowing the query to use the registered key type.
        var key = (TKey)(object)id!;

        // skip unset identifiers such as 0 or Guid.Empty; checked on the key type so int? 0 is skipped like int 0
        if (EqualityComparer<TKey>.Default.Equals(key, default))
            return ValueTask.FromResult<TItem?>(default);

        return dataService.Get<TKey, TItem>(key, cacheTime);
    }
}
