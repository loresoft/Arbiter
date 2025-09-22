using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Tests.Queries;

public class EntityFilterBuilderTests
{
    [Test]
    public void CreateSearchQueryShouldReturnEntityQuery()
    {
        // Arrange
        var searchText = "test";
        var page = 1;
        var pageSize = 20;

        // Act
        var result = EntityFilterBuilder.CreateSearchQuery<TestModel>(searchText, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);

        var filter = result.Filter;
        filter.Should().NotBeNull();

        filter.Logic.Should().Be(FilterLogic.Or);
        filter.Filters.Should().HaveCount(2);
        filter.Filters[0].Name.Should().Be("Field1");
        filter.Filters[0].Value.Should().Be("test");
        filter.Filters[1].Name.Should().Be("Field2");
        filter.Filters[1].Value.Should().Be("test");
    }

    [Test]
    public void CreateSearchSelectShouldReturnEntitySelect()
    {
        // Arrange
        var searchText = "test";

        // Act
        var result = EntityFilterBuilder.CreateSearchSelect<TestModel>(searchText);

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void CreateSearchFilterShouldReturnEntityFilter()
    {
        // Arrange
        var searchText = "test";

        // Act
        var result = EntityFilterBuilder.CreateSearchFilter<TestModel>(searchText);

        // Assert
        result.Should().NotBeNull();
        result.Logic.Should().Be(FilterLogic.Or);
        result.Filters.Should().HaveCount(2);
        result.Filters[0].Name.Should().Be("Field1");
        result.Filters[0].Value.Should().Be("test");
        result.Filters[1].Name.Should().Be("Field2");
        result.Filters[1].Value.Should().Be("test");
    }

    [Test]
    public void CreateSort_ShouldReturnEntitySort()
    {
        // Act
        var result = EntityFilterBuilder.CreateSort<TestModel>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("SortField");
    }

    [Test]
    public void CreateSearchFilter_WithFields_ShouldReturnEntityFilter()
    {
        // Arrange
        var fields = new List<string> { "Field1", "Field2" };
        var searchText = "test";

        // Act
        var result = EntityFilterBuilder.CreateSearchFilter(fields, searchText);

        // Assert
        result.Should().NotBeNull();
        result.Logic.Should().Be(FilterLogic.Or);
        result.Filters.Should().HaveCount(2);
        result.Filters[0].Name.Should().Be("Field1");
        result.Filters[0].Value.Should().Be("test");
        result.Filters[1].Name.Should().Be("Field2");
        result.Filters[1].Value.Should().Be("test");
    }

    [Test]
    public void CreateGroupShouldReturnEntityFilter()
    {
        // Arrange
        var filter1 = new EntityFilter { Name = "Field1", Value = "Value1" };
        var filter2 = new EntityFilter { Name = "Field2", Value = "Value2" };

        // Act
        var result = EntityFilterBuilder.CreateGroup(filter1, filter2);

        // Assert
        result.Should().NotBeNull();
        result.Logic.Should().Be(FilterLogic.And);
        result.Filters.Should().HaveCount(2);
        result.Filters[0].Name.Should().Be("Field1");
        result.Filters[0].Value.Should().Be("Value1");
        result.Filters[1].Name.Should().Be("Field2");
        result.Filters[1].Value.Should().Be("Value2");
    }

    [Test]
    public void CreateGroupShouldReturnEntityFilterLogic()
    {
        // Arrange
        var filter1 = new EntityFilter { Name = "Field1", Value = "Value1" };
        var filter2 = new EntityFilter { Name = "Field2", Value = "Value2" };

        // Act
        var result = EntityFilterBuilder.CreateGroup(FilterLogic.Or, filter1, filter2);

        // Assert
        result.Should().NotBeNull();
        result.Logic.Should().Be(FilterLogic.Or);
        result.Filters.Should().HaveCount(2);
        result.Filters[0].Name.Should().Be("Field1");
        result.Filters[0].Value.Should().Be("Value1");
        result.Filters[1].Name.Should().Be("Field2");
        result.Filters[1].Value.Should().Be("Value2");
    }

    [Test]
    public void CreateGroupShouldReturnNull()
    {
        // Arrange
        EntityFilter? filter1 = null;
        var filter2 = new EntityFilter();

        // Act
        var result = EntityFilterBuilder.CreateGroup(filter1, filter2);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void CreateGroupShouldReturnEntityFilterFirst()
    {
        // Arrange
        var filter1 = new EntityFilter { Name = "Field1", Value = "Value1" };
        var filter2 = new EntityFilter();

        // Act
        var result = EntityFilterBuilder.CreateGroup(FilterLogic.Or, filter1, filter2);

        // Assert
        result.Should().NotBeNull();
        result.Filters.Should().BeNullOrEmpty();
        result.Logic.Should().BeNull();
        result.Name.Should().Be("Field1");
        result.Value.Should().Be("Value1");
    }
}

public class TestModel : ISupportSearch
{
    public static IEnumerable<string> SearchFields() => ["Field1", "Field2"];
    public static string SortField() => "SortField";
}
