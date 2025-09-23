using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Provides static helper methods for building common entity filters, queries, and sort expressions.
/// </summary>
public static class EntityFilterBuilder
{
    /// <summary>
    /// Creates a query for the specified entity with optional raw query text, sorting, and pagination.
    /// </summary>
    /// <param name="query">The raw query expression to search for entities. Can be <see langword="null"/> or empty for no raw query.</param>
    /// <param name="sort">The sort expression in string format (e.g., "Name asc" or "Date desc"). Can be <see langword="null"/> for no sorting.</param>
    /// <param name="page">The page number for pagination. Must be greater than 0. Defaults to 1 if invalid value is provided.</param>
    /// <param name="pageSize">The number of items per page. Must be greater than 0. Defaults to 20 if invalid value is provided.</param>
    /// <returns>
    /// An <see cref="EntityQuery"/> instance configured with the specified parameters.
    /// </returns>
    public static EntityQuery? CreateQuery(string? query = null, string? sort = null, int page = 1, int pageSize = 20)
    {
        var entityQuery = new EntityQuery
        {
            Query = query,
            Page = page > 0 ? page : 1,
            PageSize = pageSize > 0 ? pageSize : 20,
        };
        entityQuery.AddSort(sort);

        return entityQuery;
    }

    /// <summary>
    /// Creates a search query for the specified model type using the model's configured search fields and sort field.
    /// </summary>
    /// <typeparam name="TModel">The type of the model. Must implement <see cref="ISupportSearch"/> to provide search and sort configuration.</typeparam>
    /// <param name="searchText">The text to search for across all configured search fields of the model.</param>
    /// <param name="page">The page number for pagination. Must be greater than 0. Defaults to 1 if invalid value is provided.</param>
    /// <param name="pageSize">The number of items per page. Must be greater than 0. Defaults to 20 if invalid value is provided.</param>
    /// <returns>
    /// An <see cref="EntityQuery"/> instance configured with a search filter for the specified text and default sorting,
    /// or <see langword="null"/> if the search text is invalid or empty.
    /// </returns>
    public static EntityQuery? CreateSearchQuery<TModel>(string searchText, int page = 1, int pageSize = 20)
        where TModel : class, ISupportSearch
    {
        var filter = CreateSearchFilter<TModel>(searchText);
        var sort = CreateSort<TModel>();

        return new EntityQuery
        {
            Filter = filter,
            Sort = sort is not null ? [sort] : null,
            Page = page > 0 ? page : 1,
            PageSize = pageSize > 0 ? pageSize : 20,
        };
    }

    /// <summary>
    /// Creates a search filter for the specified model type using the model's configured search fields.
    /// </summary>
    /// <typeparam name="TModel">The type of the model. Must implement <see cref="ISupportSearch"/> to provide search field configuration.</typeparam>
    /// <param name="searchText">The text to search for across all configured search fields of the model.</param>
    /// <returns>
    /// An <see cref="EntityFilter"/> instance that searches for the specified text across all search fields using the Contains operator,
    /// or <see langword="null"/> if the search text is <see langword="null"/>, empty, or whitespace-only.
    /// </returns>
    public static EntityFilter? CreateSearchFilter<TModel>(string searchText)
        where TModel : class, ISupportSearch
    {
        return CreateSearchFilter(TModel.SearchFields(), searchText);
    }

    /// <summary>
    /// Creates a sort expression for the specified model type using the model's configured default sort field.
    /// </summary>
    /// <typeparam name="TModel">The type of the model. Must implement <see cref="ISupportSearch"/> to provide sort field configuration.</typeparam>
    /// <returns>
    /// An <see cref="EntitySort"/> instance configured with the model's default sort field,
    /// or <see langword="null"/> if the model doesn't define a sort field.
    /// </returns>
    public static EntitySort? CreateSort<TModel>()
        where TModel : class, ISupportSearch
    {
        return new EntitySort { Name = TModel.SortField() };
    }

    /// <summary>
    /// Creates a sort expression for the specified field and direction.
    /// </summary>
    /// <param name="field">The name of the field or property to sort by. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="direction">The sort direction. If <see langword="null"/>, uses the default direction (ascending).</param>
    /// <returns>
    /// An <see cref="EntitySort"/> instance configured with the specified field and direction.
    /// </returns>
    public static EntitySort CreateSort(string field, SortDirections? direction = null)
        => new() { Name = field, Direction = direction };

    /// <summary>
    /// Creates a search filter for the specified fields and search text using the Contains operator.
    /// </summary>
    /// <param name="fields">The collection of field or property names to search across. Cannot be <see langword="null"/>.</param>
    /// <param name="searchText">The text to search for in the specified fields.</param>
    /// <returns>
    /// An <see cref="EntityFilter"/> instance that creates an OR group filter searching for the text across all specified fields,
    /// or <see langword="null"/> if the fields collection is <see langword="null"/> or the search text is <see langword="null"/>, empty, or whitespace-only.
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
    /// <param name="field">The name of the field or property to filter on. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="value">The value to filter against. Can be <see langword="null"/> depending on the operator used.</param>
    /// <param name="operator">The comparison operator to use for filtering. If <see langword="null"/>, uses the default operator (Equal).</param>
    /// <returns>
    /// An <see cref="EntityFilter"/> instance configured with the specified field, value, and operator.
    /// </returns>
    public static EntityFilter CreateFilter(string field, object? value, FilterOperators? @operator = null)
        => new() { Name = field, Value = value, Operator = @operator };

    /// <summary>
    /// Creates a filter group for the specified filters using the AND logic operator.
    /// </summary>
    /// <param name="filters">The collection of filters to group together. Invalid filters are automatically removed.</param>
    /// <returns>
    /// An <see cref="EntityFilter"/> instance representing the group with AND logic,
    /// the single filter if only one valid filter is provided,
    /// or <see langword="null"/> if no valid filters are provided.
    /// </returns>
    public static EntityFilter? CreateGroup(params IEnumerable<EntityFilter?> filters)
        => CreateGroup(FilterLogic.And, filters);

    /// <summary>
    /// Creates a filter group for the specified logic operator and filters.
    /// </summary>
    /// <param name="logic">The logical operator to use for combining the filters (AND or OR).</param>
    /// <param name="filters">The collection of filters to group together. Invalid filters are automatically removed.</param>
    /// <returns>
    /// An <see cref="EntityFilter"/> instance representing the group with the specified logic,
    /// the single filter if only one valid filter is provided,
    /// or <see langword="null"/> if no valid filters are provided.
    /// </returns>
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
