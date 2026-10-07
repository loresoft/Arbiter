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
    private bool _initialized;

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
    /// Gets or sets the loaded entities used to render the options.
    /// </summary>
    protected IReadOnlyCollection<TModel> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether entities are currently loading.
    /// </summary>
    protected bool IsLoading { get; set; }


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

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ChildContent ??= RenderOptions;
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

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

            var result = await DataService.Page<TModel>(query, CacheTime);

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
