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

    public static EntitySelect ToSelect(this DataRequest request)
    {
        return new EntitySelect
        {
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
                Direction = s.Descending ? "DESC" : "ASC"
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
        filter.Logic = queryGroup.Logic;

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
            Total: (int)pagedResult.Total,
            Items: pagedResult.Data ?? []
        );
    }

    public static DataResult<T> ToResult<T>(this EntityContinuationResult<T> pagedResult)
    {
        return new DataResult<T>(
            Total: pagedResult.Data?.Count ?? 0,
            Items: pagedResult.Data ?? []
        );
    }

    private static string? TranslateOperator(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return value switch
        {
            QueryOperators.Equal => EntityFilterOperators.Equal,
            QueryOperators.NotEqual => EntityFilterOperators.NotEqual,
            QueryOperators.Contains => EntityFilterOperators.Contains,
            QueryOperators.NotContains => $"!{EntityFilterOperators.Contains}",
            QueryOperators.StartsWith => EntityFilterOperators.StartsWith,
            QueryOperators.NotStartsWith => $"!{EntityFilterOperators.StartsWith}",
            QueryOperators.EndsWith => EntityFilterOperators.EndsWith,
            QueryOperators.NotEndsWith => $"!{EntityFilterOperators.EndsWith}",
            QueryOperators.GreaterThan => EntityFilterOperators.GreaterThan,
            QueryOperators.GreaterThanOrEqual => EntityFilterOperators.GreaterThanOrEqual,
            QueryOperators.LessThan => EntityFilterOperators.LessThan,
            QueryOperators.LessThanOrEqual => EntityFilterOperators.LessThanOrEqual,
            QueryOperators.IsNull => EntityFilterOperators.IsNull,
            QueryOperators.IsNotNull => EntityFilterOperators.IsNotNull,
            _ => value,
        };
    }
}
