using System.Data;

using Arbiter.CommandQuery.Queries;

using LoreSoft.Blazor.Controls;

namespace Tracker.Client.Extensions;

public static class GridExtensions
{
    public static EntityQuery ToQuery(this DataRequest request)
    {
        return new EntityQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Sort = request.Sorts.ToSort(),
            Filter = request.Query.ToFilter()
        };
    }

    public static List<EntitySort>? ToSort(this IEnumerable<DataSort>? sorts)
    {
        if (sorts == null)
            return null;

        return sorts
            .Select(s => new EntitySort
            {
                Name = s.Property,
                Direction = s.Descending ? SortDirections.Descending : SortDirections.Ascending
            })
            .ToList();
    }

    public static EntityFilter? ToFilter(this QueryRule? queryRule)
    {
        if (queryRule is QueryGroup group)
            return group.ToFilter();
        else if (queryRule is QueryFilter filter)
            return filter.ToFilter();

        return null;
    }

    public static EntityFilter? ToFilter(this QueryGroup? queryGroup)
    {
        if (queryGroup == null || queryGroup.Filters.Count == 0)
            return null;

        var filter = new EntityFilter();
        filter.Logic = TranslateLogic(queryGroup.Logic);

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

    public static EntityFilter? ToFilter(this QueryFilter? queryFilter)
    {
        if (queryFilter == null)
            return null;

        return new EntityFilter
        {
            Name = queryFilter.Field,
            Operator = TranslateOperator(queryFilter.Operator),
            Value = queryFilter.Value
        };
    }

    public static DataResult<T> ToResult<T>(this EntityPagedResult<T> pagedResult)
    {
        return new DataResult<T>(
            total: (int)(pagedResult.Total ?? 0),
            items: pagedResult.Data ?? []
        );
    }

    private static FilterOperators? TranslateOperator(string? value)
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
            _ => default,
        };
    }

    private static FilterLogic? TranslateLogic(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;

        return value switch
        {
            "and" => FilterLogic.And,
            "or" => FilterLogic.Or,
            _ => default,
        };
    }
}
