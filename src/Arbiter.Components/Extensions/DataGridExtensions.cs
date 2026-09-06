using Arbiter.CommandQuery.Queries;

using LoreSoft.Blazor.Controls;

namespace Arbiter.Components.Extensions;

/// <summary>
/// Extension methods converting <see cref="DataGrid{TItem}"/> requests to command query requests.
/// </summary>
public static class DataGridExtensions
{
    /// <summary>
    /// Converts a data grid request to an <see cref="EntityQuery"/>.
    /// </summary>
    /// <param name="request">The paging, sorting and filtering options requested by the data grid</param>
    /// <returns>The equivalent <see cref="EntityQuery"/></returns>
    /// <exception cref="ArgumentNullException">When <paramref name="request"/> is <see langword="null"/></exception>
    public static EntityQuery ToQuery(this DataRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new EntityQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            ContinuationToken = request.ContinuationToken,
            Sort = request.Sorts.ToSort(),
            Filter = request.Query.ToFilter(),
        };
    }


    /// <summary>
    /// Converts the sorts requested by the data grid to <see cref="EntitySort"/> instances.
    /// </summary>
    /// <param name="sorts">The sorts requested by the data grid</param>
    /// <returns>
    /// The equivalent sorts, or <see langword="null"/> when <paramref name="sorts"/> is <see langword="null"/>
    /// </returns>
    public static IList<EntitySort>? ToSort(this IEnumerable<DataSort>? sorts)
    {
        if (sorts == null)
            return null;

        return sorts
            .Select(s => new EntitySort
            {
                Name = s.Property,
                Direction = s.Descending ? SortDirections.Descending : SortDirections.Ascending,
            })
            .ToList();
    }


    /// <summary>
    /// Converts a query rule to an <see cref="EntityFilter"/>.
    /// </summary>
    /// <param name="queryRule">The query rule built from the data grid state</param>
    /// <returns>
    /// The equivalent filter, or <see langword="null"/> when the rule is <see langword="null"/> or is not a
    /// <see cref="QueryGroup"/> or <see cref="QueryFilter"/>
    /// </returns>
    public static EntityFilter? ToFilter(this QueryRule? queryRule)
    {
        if (queryRule is QueryGroup group)
            return group.ToFilter();

        if (queryRule is QueryFilter filter)
            return filter.ToFilter();

        return null;
    }

    /// <summary>
    /// Converts a query group to an <see cref="EntityFilter"/> containing the nested filters.
    /// </summary>
    /// <param name="queryGroup">The query group built from the data grid state</param>
    /// <returns>
    /// The equivalent filter, or <see langword="null"/> when the group is <see langword="null"/> or empty
    /// </returns>
    public static EntityFilter? ToFilter(this QueryGroup? queryGroup)
    {
        if (queryGroup == null || queryGroup.Filters.Count == 0)
            return null;

        var filter = new EntityFilter
        {
            Logic = queryGroup.Logic.ToLogic(),
        };

        foreach (var rule in queryGroup.Filters)
        {
            var ruleFilter = rule.ToFilter();
            if (ruleFilter == null)
                continue;

            filter.Filters ??= [];
            filter.Filters.Add(ruleFilter);
        }

        return filter;
    }

    /// <summary>
    /// Converts a query filter to an <see cref="EntityFilter"/>.
    /// </summary>
    /// <param name="queryFilter">The query filter built from the data grid state</param>
    /// <returns>
    /// The equivalent filter, or <see langword="null"/> when <paramref name="queryFilter"/> is <see langword="null"/>
    /// </returns>
    public static EntityFilter? ToFilter(this QueryFilter? queryFilter)
    {
        if (queryFilter == null)
            return null;

        return new EntityFilter
        {
            Name = queryFilter.Field,
            Key = queryFilter.Key,
            Operator = queryFilter.Operator.ToOperator(),
            Value = queryFilter.Value,
        };
    }


    /// <summary>
    /// Converts a paged query result to a data grid result.
    /// </summary>
    /// <typeparam name="T">The type of the items in the result</typeparam>
    /// <param name="pagedResult">The paged result returned by the data store</param>
    /// <returns>The equivalent <see cref="DataResult{T}"/></returns>
    /// <exception cref="ArgumentNullException">When <paramref name="pagedResult"/> is <see langword="null"/></exception>
    /// <remarks>
    /// The total is clamped to the range of <see cref="int"/> because the data grid counts rows with an
    /// <see cref="int"/>; a larger total would otherwise overflow to a negative page count.
    /// </remarks>
    public static DataResult<T> ToResult<T>(this EntityPagedResult<T> pagedResult)
    {
        ArgumentNullException.ThrowIfNull(pagedResult);

        return new DataResult<T>(
            items: pagedResult.Data ?? [],
            total: (int)Math.Clamp(pagedResult.Total ?? 0, 0, int.MaxValue),
            continuationToken: pagedResult.ContinuationToken
        );
    }


    /// <summary>
    /// Translates a <see cref="QueryOperators"/> value to the matching <see cref="FilterOperators"/> value.
    /// </summary>
    /// <param name="value">The query operator used by the data grid</param>
    /// <returns>
    /// The matching filter operator, or <see langword="null"/> when the value is empty or not recognized
    /// </returns>
    public static FilterOperators? ToOperator(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;

        return value switch
        {
            QueryOperators.Equal => FilterOperators.Equal,
            QueryOperators.NotEqual => FilterOperators.NotEqual,
            QueryOperators.Contains => FilterOperators.Contains,
            QueryOperators.NotContains => FilterOperators.NotContains,
            QueryOperators.StartsWith => FilterOperators.StartsWith,
            QueryOperators.NotStartsWith => FilterOperators.NotStartsWith,
            QueryOperators.EndsWith => FilterOperators.EndsWith,
            QueryOperators.NotEndsWith => FilterOperators.NotEndsWith,
            QueryOperators.GreaterThan => FilterOperators.GreaterThan,
            QueryOperators.GreaterThanOrEqual => FilterOperators.GreaterThanOrEqual,
            QueryOperators.LessThan => FilterOperators.LessThan,
            QueryOperators.LessThanOrEqual => FilterOperators.LessThanOrEqual,
            QueryOperators.IsNull => FilterOperators.IsNull,
            QueryOperators.IsNotNull => FilterOperators.IsNotNull,
            _ => (FilterOperators?)null,
        };
    }

    /// <summary>
    /// Translates a <see cref="QueryLogic"/> value to the matching <see cref="FilterLogic"/> value.
    /// </summary>
    /// <param name="value">The query logic used by the data grid</param>
    /// <returns>
    /// The matching filter logic, or <see langword="null"/> when the value is empty or not recognized
    /// </returns>
    public static FilterLogic? ToLogic(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;

        return value switch
        {
            QueryLogic.And => FilterLogic.And,
            QueryLogic.Or => FilterLogic.Or,
            _ => (FilterLogic?)null,
        };
    }
}
