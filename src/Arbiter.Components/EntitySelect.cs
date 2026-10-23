using System.Globalization;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Options;
using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Services;
using Arbiter.Dispatcher;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Options;

namespace Arbiter.Components;

/// <summary>
/// A select input component that loads its options from entities queried through <see cref="IDispatcherDataService"/>.
/// </summary>
/// <typeparam name="TModel">The entity model type used to populate the options.</typeparam>
/// <typeparam name="TValue">The type of the bound value.</typeparam>
public class EntitySelect<TModel, TValue> : InputSelect<TValue>
     where TModel : class, IHaveIdentifier, ISupportSearch
{
    private EntityFilter? _loadedFilter;
    private string? _loadedSort;
    private bool _hasLoaded;
    private int _loadVersion;

    private bool? _hasPermission;
    private Task? _permissionTask;

    private IReadOnlyDictionary<string, object>? _sourceAttributes;
    private IReadOnlyDictionary<string, object>? _mergedAttributes;

    /// <summary>
    /// Gets or sets the environment options used to provide the default <see cref="CacheTime"/>.
    /// </summary>
    [Inject]
    public required IOptions<EnvironmentOptions> EnvironmentOptions { get; set; }

    /// <summary>
    /// Gets or sets the service used to display error notifications.
    /// </summary>
    [Inject]
    public required INotificationService Notification { get; set; }

    /// <summary>
    /// Gets or sets the data service used to load the entities.
    /// </summary>
    [Inject]
    public required IDispatcherDataService DataService { get; set; }

    /// <summary>
    /// Gets or sets the service used to check permission when <see cref="PermissionResolver"/> is not set.
    /// </summary>
    [Inject]
    public required IPermissionResolver PermissionService { get; set; }

    /// <summary>
    /// Gets or sets the function used to get the option value for an entity.
    /// When not set, the entity identifier is formatted using the current culture.
    /// </summary>
    [Parameter]
    public Func<TModel, string>? ValueSelector { get; set; }

    /// <summary>
    /// Gets or sets the function used to get the option display text for an entity.
    /// When not set, <see cref="object.ToString"/> is used.
    /// </summary>
    [Parameter]
    public Func<TModel, string>? TextSelector { get; set; }

    /// <summary>
    /// Gets or sets the sort expression for the entities.
    /// When not set, the model's default sort field is used.
    /// </summary>
    [Parameter]
    public string? Sort { get; set; }

    /// <summary>
    /// Gets or sets the text displayed for the empty option.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "- select -";

    /// <summary>
    /// Gets or sets the filter applied when loading entities. Changing the filter reloads the options.
    /// </summary>
    [Parameter]
    public EntityFilter? Filter { get; set; }

    /// <summary>
    /// Gets or sets how long the loaded entities are cached. Set to <see langword="null"/> to disable caching.
    /// </summary>
    /// <value>Defaults to <see cref="EnvironmentOptions.DefaultCacheTime"/>.</value>
    [Parameter]
    public TimeSpan? CacheTime { get; set; }

    /// <summary>
    /// Gets or sets the text displayed in the empty option while entities are loading.
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Loading...";

    /// <summary>
    /// Gets or sets a value indicating whether the select is disabled.
    /// A value of <see langword="true"/>, or a truthy <c>disabled</c> attribute, takes priority over <see cref="PermissionResolver"/>.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the function used to determine whether the current user has permission to use the select.
    /// When it returns <see langword="false"/>, the select is disabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The function is evaluated once and the result is cached for the lifetime of the component. The select is
    /// disabled while the function is pending, and remains disabled if it throws. When the select is explicitly
    /// disabled through <see cref="Disabled"/> or a <c>disabled</c> attribute, the check is skipped.
    /// </para>
    /// <para>
    /// When not set, the injected <see cref="IPermissionResolver"/> is used with the <see cref="Action"/>,
    /// <see cref="Subject"/> and <see cref="Field"/> values.
    /// </para>
    /// </remarks>
    [Parameter]
    public Func<Task<bool>>? PermissionResolver { get; set; }

    /// <summary>
    /// Gets or sets the subject passed to the registered <see cref="IPermissionResolver"/>.
    /// Defaults to the type name of the cascading <see cref="EditContext"/> model.
    /// </summary>
    /// <remarks>
    /// When there is no subject, the permission check is skipped and access is granted.
    /// </remarks>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the field passed to the registered <see cref="IPermissionResolver"/>.
    /// Defaults to the name of the bound field.
    /// </summary>
    [Parameter]
    public string? Field { get; set; }

    /// <summary>
    /// Gets or sets the action passed to the registered <see cref="IPermissionResolver"/>.
    /// Defaults to <c>update</c>.
    /// </summary>
    [Parameter]
    public string? Action { get; set; }


    /// <summary>
    /// Gets or sets the loaded entities used to render the options.
    /// </summary>
    protected IReadOnlyCollection<TModel> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether entities are currently loading.
    /// </summary>
    protected bool IsLoading { get; set; }

    /// <summary>
    /// Gets a value indicating whether the select is disabled, either explicitly through <see cref="Disabled"/> or a
    /// <c>disabled</c> attribute, or because the permission check has not granted access.
    /// </summary>
    protected bool IsDisabled => IsExplicitlyDisabled || _hasPermission != true;

    private bool IsExplicitlyDisabled => Disabled || IsDisabledAttribute(_sourceAttributes);


    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ChildContent ??= RenderOptions;

        // a new dictionary means the caller supplied the attributes; otherwise keep the last caller supplied source
        if (!ReferenceEquals(AdditionalAttributes, _mergedAttributes))
            _sourceAttributes = AdditionalAttributes;

        ApplyDisabledAttribute();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // start loading first so the data load and permission check run concurrently
        var loadTask = LoadItems();

        // an explicit disable wins, and a resolved permission is cached for the component lifetime
        if (!IsExplicitlyDisabled && !_hasPermission.HasValue)
        {
            // reuse the in-flight check so parameter updates while the resolver is pending don't invoke it again
            _permissionTask ??= ResolvePermission();
            await _permissionTask;

            // apply the permission as soon as it resolves rather than waiting for the load to finish
            ApplyDisabledAttribute();
            StateHasChanged();
        }

        await loadTask;
    }

    /// <summary>
    /// Determines whether the current user has permission to use the select.
    /// </summary>
    /// <returns>
    /// The result of <see cref="PermissionResolver"/> when set; otherwise the result of the injected
    /// <see cref="IPermissionResolver"/>. Returns <see langword="true"/> without calling the resolver when no
    /// <see cref="Subject"/> can be determined.
    /// </returns>
    protected virtual async Task<bool> HasPermission()
    {
        if (PermissionResolver is not null)
            return await PermissionResolver();

        var subject = Subject ?? EditContext?.Model.GetType().Name;
        if (string.IsNullOrWhiteSpace(subject))
            return true;

        var action = Action ?? "update";
        var field = Field;

        if (field is null && !FieldIdentifier.Equals(default))
            field = FieldIdentifier.FieldName;

        return await PermissionService.HasPermission(action, subject, field);
    }

    private async Task LoadItems()
    {
        var changed = !_hasLoaded
            || !Equals(_loadedFilter, Filter)
            || !string.Equals(_loadedSort, Sort, StringComparison.Ordinal);

        if (!changed)
            return;

        _hasLoaded = true;
        _loadedFilter = Filter;
        _loadedSort = Sort;

        var version = ++_loadVersion;
        IsLoading = true;

        var items = await LoadData();

        if (version != _loadVersion)
            return;

        Items = items;
        IsLoading = false;
    }

    /// <summary>
    /// Loads the entities using the current <see cref="Filter"/> and <see cref="Sort"/>.
    /// Errors are reported through <see cref="Notification"/> and an empty collection is returned.
    /// </summary>
    /// <returns>The loaded entities.</returns>
    protected async Task<IReadOnlyCollection<TModel>> LoadData()
    {
        try
        {
            var query = new EntityQuery { Filter = Filter };
            query.AddSort(Sort ?? TModel.SortField());

            var cacheTime = CacheTime ?? EnvironmentOptions.Value.DefaultCacheTime;
            var result = await DataService.Page<TModel>(query, cacheTime);

            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
            return [];
        }
    }

    /// <summary>
    /// Renders the empty option followed by an option for each loaded entity.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> to render to.</param>
    protected void RenderOptions(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "option");
        builder.AddAttribute(1, "value", string.Empty);
        builder.AddContent(2, IsLoading ? LoadingText : Placeholder);
        builder.CloseElement();


        foreach (var item in Items)
        {
            var id = item.GetIdentifier();
            var value = ConvertValue(item, id);
            var text = ConvertText(item);

            builder.OpenElement(3, "option");

            builder.SetKey(id);
            builder.AddAttribute(4, "value", value);
            builder.AddContent(5, text);

            builder.CloseElement();
        }
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

    private void ApplyDisabledAttribute()
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (_sourceAttributes != null)
        {
            foreach (var attribute in _sourceAttributes)
            {
                if (!string.Equals(attribute.Key, "disabled", StringComparison.OrdinalIgnoreCase))
                    attributes[attribute.Key] = attribute.Value;
            }
        }

        if (IsDisabled)
            attributes["disabled"] = true;

        _mergedAttributes = attributes;
        AdditionalAttributes = attributes;
    }

    private static bool IsDisabledAttribute(IReadOnlyDictionary<string, object>? attributes)
    {
        if (attributes is null)
            return false;

        foreach (var attribute in attributes)
        {
            if (!string.Equals(attribute.Key, "disabled", StringComparison.OrdinalIgnoreCase))
                continue;

            return attribute.Value switch
            {
                null => false,
                bool value => value,
                string value => !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase),
                _ => true,
            };
        }

        return false;
    }


    private string ConvertText(TModel model)
    {
        if (TextSelector != null)
            return TextSelector(model);

        return model.ToString() ?? string.Empty;
    }

    private string ConvertValue(TModel model, object id)
    {
        if (ValueSelector != null)
            return ValueSelector(model);

        return Convert.ToString(id, CultureInfo.CurrentCulture) ?? string.Empty;
    }
}
