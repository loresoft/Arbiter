using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Filters;

/// <summary>
/// Provides static helper methods for building common entity filters, queries, and sort expressions.
/// </summary>
/// <remarks>
/// This builder simplifies the creation of <see cref="EntityFilter"/>, <see cref="EntityQuery"/>, <see cref="EntitySelect"/>, and <see cref="EntitySort"/> instances
/// for use in data-driven Blazor and WebAssembly applications.
/// </remarks>
public static class EntityFilterBuilder
{
    /// <summary>
    /// Creates a search query for the specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model. Must implement <see cref="ISupportSearch"/>.</typeparam>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="page">The page number for the query. The default page is 1.</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20.</param>
    /// <returns>
    /// An instance of <see cref="EntityQuery"/> configured for the search text, page, and page size,
    /// or <see langword="null"/> if the search text is invalid.
    /// </returns>
    public static EntityQuery? CreateSearchQuery<TModel>(string searchText, int page = 1, int pageSize = 20)
        where TModel : class, ISupportSearch
    {
        var filter = CreateSearchFilter<TModel>(searchText);
        var sort = CreateSort<TModel>();

        return new EntityQuery(filter, sort, page, pageSize);
    }

    /// <summary>
    /// Creates a search select query for the specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model. Must implement <see cref="ISupportSearch"/>.</typeparam>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>
    /// An instance of <see cref="EntitySelect"/> configured for the search text,
    /// or <see langword="null"/> if the search text is invalid.
    /// </returns>
    public static EntitySelect? CreateSearchSelect<TModel>(string searchText)
        where TModel : class, ISupportSearch
    {
        var filter = CreateSearchFilter<TModel>(searchText);
        var sort = CreateSort<TModel>();

        return new EntitySelect(filter, sort);
    }

    /// <summary>
    /// Creates a search filter for the specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model. Must implement <see cref="ISupportSearch"/>.</typeparam>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>
    /// An instance of <see cref="EntityFilter"/> for the search text,
    /// or <see langword="null"/> if the search text is invalid.
    /// </returns>
    public static EntityFilter? CreateSearchFilter<TModel>(string searchText)
        where TModel : class, ISupportSearch
    {
        return CreateSearchFilter(TModel.SearchFields(), searchText);
    }

    /// <summary>
    /// Creates a sort expression for the specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model. Must implement <see cref="ISupportSearch"/>.</typeparam>
    /// <returns>
    /// An instance of <see cref="EntitySort"/> for the model type.
    /// </returns>
    public static EntitySort? CreateSort<TModel>()
        where TModel : class, ISupportSearch
    {
        return new EntitySort { Name = TModel.SortField() };
    }

    /// <summary>
    /// Creates a sort expression for the specified field and direction.
    /// </summary>
    /// <param name="field">The field or property name to sort on.</param>
    /// <param name="direction">The sort direction (e.g., "asc" or "desc").</param>
    /// <returns>
    /// An instance of <see cref="EntitySort"/> for the specified field and direction.
    /// </returns>
    public static EntitySort CreateSort(string field, string? direction = null)
        => new() { Name = field, Direction = direction };

    /// <summary>
    /// Creates a search filter for the specified fields and search text.
    /// </summary>
    /// <param name="fields">The list of fields or property names to search on.</param>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>
    /// An instance of <see cref="EntityFilter"/> for the search text,
    /// or <see langword="null"/> if the fields or search text are invalid.
    /// </returns>
    public static EntityFilter? CreateSearchFilter(IEnumerable<string> fields, string searchText)
    {
        if (fields is null || string.IsNullOrWhiteSpace(searchText))
            return null;

        var groupFilter = new EntityFilter
        {
            Logic = FilterLogic.Or,
            Filters = [],
        };

        foreach (var field in fields)
        {
            var filter = new EntityFilter
            {
                Name = field,
                Value = searchText,
                Operator = FilterOperators.Contains,
            };
            groupFilter.Filters.Add(filter);
        }

        return groupFilter;
    }

    /// <summary>
    /// Creates a filter for the specified field, value, and operator.
    /// </summary>
    /// <param name="field">The field or property name to filter on.</param>
    /// <param name="value">The value to filter for.</param>
    /// <param name="operator">The operator to use for the filter (e.g., "eq", "contains").</param>
    /// <returns>
    /// An instance of <see cref="EntityFilter"/> for the specified field, value, and operator.
    /// </returns>
    public static EntityFilter CreateFilter(string field, object? value, FilterOperators? @operator = null)
        => new() { Name = field, Value = value, Operator = @operator };

    /// <summary>
    /// Creates a filter group for the specified filters using the "and" logic operator.
    /// </summary>
    /// <param name="filters">The list of filters to group.</param>
    /// <returns>
    /// An instance of <see cref="EntityFilter"/> representing the group,
    /// or <see langword="null"/> if no valid filters are provided.
    /// </returns>
    /// <remarks>
    /// Any invalid filters will be removed from the group.
    /// </remarks>
    public static EntityFilter? CreateGroup(params IEnumerable<EntityFilter?> filters)
        => CreateGroup(FilterLogic.And, filters);

    /// <summary>
    /// Creates a filter group for the specified logic and filters.
    /// </summary>
    /// <param name="logic">The group logic operator (e.g., "and" or "or").</param>
    /// <param name="filters">The list of filters to group.</param>
    /// <returns>
    /// An instance of <see cref="EntityFilter"/> representing the group,
    /// or <see langword="null"/> if no valid filters are provided.
    /// </returns>
    /// <remarks>
    /// Any invalid filters will be removed from the group.
    /// </remarks>
    public static EntityFilter? CreateGroup(FilterLogic logic, params IEnumerable<EntityFilter?> filters)
    {
        // check for any valid filters
        if (!filters.Any(f => f?.IsValid() == true))
            return null;

        var groupFilters = filters
            .Where(f => f?.IsValid() == true)
            .Select(f => f!)
            .ToList();

        // no need for group if only one filter
        if (groupFilters.Count == 1)
            return groupFilters[0];

        return new EntityFilter
        {
            Logic = logic,
            Filters = groupFilters,
        };
    }
}
