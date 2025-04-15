using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A builder for creating common entity filters.
/// </summary>
public static class EntityFilterBuilder
{
    /// <summary>
    /// Creates a search query for the specified model type. <typeparamref name="TModel"/> must implement <see cref="ISupportSearch"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="searchText">The text to search for</param>
    /// <param name="page">The page number for the query. The default page is 1</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20</param>
    /// <returns>An instance of <see cref="EntityQuery"/> for the search text, page and page size</returns>
    public static EntityQuery? CreateSearchQuery<TModel>(string searchText, int page = 1, int pageSize = 20)
        where TModel : class, ISupportSearch
    {
        var filter = CreateSearchFilter<TModel>(searchText);
        var sort = CreateSort<TModel>();

        return new EntityQuery(filter, sort, page, pageSize);
    }

    /// <summary>
    /// Creates a search query for the specified model type. <typeparamref name="TModel"/> must implement <see cref="ISupportSearch"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="searchText">The text to search for</param>
    /// <returns>An instance of <see cref="EntitySelect"/> for the search text</returns>
    public static EntitySelect? CreateSearchSelect<TModel>(string searchText)
        where TModel : class, ISupportSearch
    {
        var filter = CreateSearchFilter<TModel>(searchText);
        var sort = CreateSort<TModel>();

        return new EntitySelect(filter, sort);
    }

    /// <summary>
    /// Creates a search filter for the specified model type. <typeparamref name="TModel"/> must implement <see cref="ISupportSearch"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="searchText">The text to search for</param>
    /// <returns>An instance of <see cref="EntityFilter"/> for the search text</returns>
    public static EntityFilter? CreateSearchFilter<TModel>(string searchText)
        where TModel : class, ISupportSearch
    {
        return CreateSearchFilter(TModel.SearchFields(), searchText);
    }

    /// <summary>
    /// Creates a sort expression for specified model type. <typeparamref name="TModel"/> must implement <see cref="ISupportSearch"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <returns>An instance of <see cref="EntitySort"/> for the model type</returns>
    public static EntitySort? CreateSort<TModel>()
        where TModel : class, ISupportSearch
    {
        return new EntitySort { Name = TModel.SortField() };
    }

    /// <summary>
    /// Creates a sort expression for the specified field and direction.
    /// </summary>
    /// <param name="field">The field or property name to sort on</param>
    /// <param name="direction">The sort direction (e.g., "asc" or "desc")</param>
    /// <returns>An instance of <see cref="EntitySort"/> for the specified field and direction</returns>
    public static EntitySort CreateSort(string field, string? direction = null)
        => new() { Name = field, Direction = direction };

    /// <summary>
    /// Creates a search filter for the specified fields and search text.
    /// </summary>
    /// <param name="fields">The list of fields or property names to search on</param>
    /// <param name="searchText">The text to search for</param>
    /// <returns>An instance of <see cref="EntityFilter"/> for the search text</returns>
    public static EntityFilter? CreateSearchFilter(IEnumerable<string> fields, string searchText)
    {
        if (fields is null || string.IsNullOrWhiteSpace(searchText))
            return null;

        var groupFilter = new EntityFilter
        {
            Logic = EntityFilterLogic.Or,
            Filters = [],
        };

        foreach (var field in fields)
        {
            var filter = new EntityFilter
            {
                Name = field,
                Value = searchText,
                Operator = EntityFilterOperators.Contains,
            };
            groupFilter.Filters.Add(filter);
        }

        return groupFilter;
    }

    /// <summary>
    /// Creates a filter for the specified field, value and operator.
    /// </summary>
    /// <param name="field">The field or property name to search on</param>
    /// <param name="value">The value to search for</param>
    /// <param name="operator">The operator to use for the filter</param>
    /// <returns>An instance of <see cref="EntityFilter"/> for the specified field, value and operator</returns>
    public static EntityFilter CreateFilter(string field, object? value, string? @operator = null)
        => new() { Name = field, Value = value, Operator = @operator };

    /// <summary>
    /// Creates a filter group for the specified filters. The logic for the group is set to "and".
    /// </summary>
    /// <param name="filters">The list of filters to create a group from</param>
    /// <returns>An instance of <see cref="EntityFilter"/> for the group</returns>
    /// <remarks>Any invalid filters will be removed</remarks>
    public static EntityFilter? CreateGroup(params IEnumerable<EntityFilter?> filters)
        => CreateGroup(EntityFilterLogic.And, filters);

    /// <summary>
    /// Creates a filter group for the specified logic and filters
    /// </summary>
    /// <param name="logic">The group logic operator.</param>
    /// <param name="filters">The list of filters to create a group from</param>
    /// <returns>An instance of <see cref="EntityFilter"/> for the group</returns>
    /// <remarks>Any invalid filters will be removed</remarks>
    public static EntityFilter? CreateGroup(string logic, params IEnumerable<EntityFilter?> filters)
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
