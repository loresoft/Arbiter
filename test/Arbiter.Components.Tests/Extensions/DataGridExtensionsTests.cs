using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Extensions;

using LoreSoft.Blazor.Controls;

namespace Arbiter.Components.Tests.Extensions;

public class DataGridExtensionsTests
{
    [Test]
    public async Task ToQueryCopiesPagingSortingAndFiltering()
    {
        var request = new DataRequest
        {
            Page = 2,
            PageSize = 25,
            ContinuationToken = "token",
            Sorts = [new DataSort("Name", true)],
            Query = new QueryGroup
            {
                Logic = QueryLogic.And,
                Filters = [new QueryFilter { Field = "Status", Operator = QueryOperators.Equal, Value = "Active" }],
            },
        };

        var query = request.ToQuery();

        await Assert.That(query.Page).IsEqualTo(2);
        await Assert.That(query.PageSize).IsEqualTo(25);
        await Assert.That(query.ContinuationToken).IsEqualTo("token");
        await Assert.That(query.Sort).IsNotNull();
        await Assert.That(query.Sort![0].Name).IsEqualTo("Name");
        await Assert.That(query.Sort[0].Direction).IsEqualTo(SortDirections.Descending);
        await Assert.That(query.Filter).IsNotNull();
        await Assert.That(query.Filter!.Logic).IsEqualTo(FilterLogic.And);
        await Assert.That(query.Filter.Filters![0].Name).IsEqualTo("Status");
    }

    [Test]
    public async Task ToQueryThrowsWhenRequestIsNull()
    {
        var action = () => DataGridExtensions.ToQuery(null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ToSortReturnsNullWhenSortsAreNull()
    {
        var sorts = DataGridExtensions.ToSort(null);

        await Assert.That(sorts).IsNull();
    }

    [Test]
    public async Task ToSortMapsAscendingAndDescending()
    {
        var sorts = new[]
        {
            new DataSort("First", false),
            new DataSort("Second", true),
        }.ToSort();

        await Assert.That(sorts).IsNotNull();
        await Assert.That(sorts![0].Name).IsEqualTo("First");
        await Assert.That(sorts[0].Direction).IsEqualTo(SortDirections.Ascending);
        await Assert.That(sorts[1].Direction).IsEqualTo(SortDirections.Descending);
    }

    [Test]
    public async Task ToFilterReturnsNullForNullRule()
    {
        var filter = DataGridExtensions.ToFilter((QueryRule?)null);

        await Assert.That(filter).IsNull();
    }

    [Test]
    public async Task ToFilterReturnsNullForEmptyGroup()
    {
        var filter = new QueryGroup { Logic = QueryLogic.And }.ToFilter();

        await Assert.That(filter).IsNull();
    }

    [Test]
    public async Task ToFilterMapsNestedFilters()
    {
        var group = new QueryGroup
        {
            Logic = QueryLogic.Or,
            Filters =
            [
                new QueryFilter { Field = "First", Operator = QueryOperators.Equal, Value = 1 },
                new QueryFilter { Field = "Second", Operator = QueryOperators.Contains, Value = "abc" },
            ],
        };

        var filter = group.ToFilter();

        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.Logic).IsEqualTo(FilterLogic.Or);
        await Assert.That(filter.Filters).IsNotNull();
        await Assert.That(filter.Filters!.Count).IsEqualTo(2);
        await Assert.That(filter.Filters[1].Operator).IsEqualTo(FilterOperators.Contains);
        await Assert.That(filter.Filters[1].Value).IsEqualTo("abc");
    }

    [Test]
    public async Task ToFilterMapsSingleFilter()
    {
        var filter = new QueryFilter
        {
            Field = "Name",
            Operator = QueryOperators.StartsWith,
            Value = "abc",
        }.ToFilter();

        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.Name).IsEqualTo("Name");
        await Assert.That(filter.Operator).IsEqualTo(FilterOperators.StartsWith);
    }

    [Test]
    [Arguments(QueryOperators.Equal, FilterOperators.Equal)]
    [Arguments(QueryOperators.NotEqual, FilterOperators.NotEqual)]
    [Arguments(QueryOperators.StartsWith, FilterOperators.StartsWith)]
    [Arguments(QueryOperators.IsNull, FilterOperators.IsNull)]
    public async Task ToOperatorMapsKnownValues(string value, FilterOperators expected)
    {
        var result = value.ToOperator();

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("")]
    [Arguments("  ")]
    [Arguments("not-an-operator")]
    [Arguments(null)]
    public async Task ToOperatorReturnsNullForUnknownValues(string? value)
    {
        var result = value.ToOperator();

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ToLogicMapsKnownValues()
    {
        await Assert.That(QueryLogic.And.ToLogic()).IsEqualTo(FilterLogic.And);
        await Assert.That(QueryLogic.Or.ToLogic()).IsEqualTo(FilterLogic.Or);
        await Assert.That("unknown".ToLogic()).IsNull();
        await Assert.That(((string?)null).ToLogic()).IsNull();
    }

    [Test]
    public async Task ToResultThrowsWhenPagedResultIsNull()
    {
        var action = () => DataGridExtensions.ToResult<string>(null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ToResultUsesEmptyItemsWhenDataIsNull()
    {
        var result = new EntityPagedResult<string> { Data = null, Total = 0 }.ToResult();

        await Assert.That(result.Items).IsNotNull();
        await Assert.That(result.Total).IsEqualTo(0);
    }

    [Test]
    public async Task ToResultClampsTotalLargerThanIntMaxValue()
    {
        var pagedResult = new EntityPagedResult<string>
        {
            Data = ["one"],
            Total = (long)int.MaxValue + 100,
        };

        var result = pagedResult.ToResult();

        await Assert.That(result.Total).IsEqualTo(int.MaxValue);
    }

    [Test]
    public async Task ToResultClampsNegativeTotalToZero()
    {
        var pagedResult = new EntityPagedResult<string> { Data = [], Total = -5 };

        var result = pagedResult.ToResult();

        await Assert.That(result.Total).IsEqualTo(0);
    }
}
