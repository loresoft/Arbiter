using Arbiter.CommandQuery.Models;
using Arbiter.CommandQuery.Queries;

using MessagePack;

namespace Arbiter.CommandQuery.Tests;

public class MessagePackSerializationTests
{
    [Test]
    public void EntityFilterRoundTripSerialization()
    {
        // Arrange - create a simple filter
        var filter = new EntityFilter
        {
            Name = "Status",
            Operator = FilterOperators.Equal,
            Value = "Active"
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().Be(filter.Value);
    }

    [Test]
    public void EntityFilterGroupRoundTripSerialization()
    {
        // Arrange - create a group filter with nested filters
        var filter = new EntityFilter
        {
            Logic = FilterLogic.And,
            Filters = new List<EntityFilter>
            {
                new EntityFilter { Name = "Priority", Operator = FilterOperators.GreaterThan, Value = 1 },
                new EntityFilter { Name = "Status", Operator = FilterOperators.Equal, Value = "Active" },
                new EntityFilter
                {
                    Logic = FilterLogic.Or,
                    Filters = new List<EntityFilter>
                    {
                        new EntityFilter { Name = "Category", Operator = FilterOperators.Equal, Value = "Urgent" },
                        new EntityFilter { Name = "Category", Operator = FilterOperators.Equal, Value = "High" }
                    }
                }
            }
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Logic.Should().Be(filter.Logic);
        deserialized.Filters.Should().NotBeNull();
        deserialized.Filters.Should().HaveCount(3);
        deserialized.Filters![0].Name.Should().Be("Priority");
        deserialized.Filters[0].Operator.Should().Be(FilterOperators.GreaterThan);
        deserialized.Filters[0].Value.Should().Be(1);
        deserialized.Filters[1].Name.Should().Be("Status");
        deserialized.Filters[2].Logic.Should().Be(FilterLogic.Or);
        deserialized.Filters[2].Filters.Should().HaveCount(2);
    }

    [Test]
    public void EntityFilterWithNullOperatorRoundTripSerialization()
    {
        // Arrange - create a filter with IsNull operator
        var filter = new EntityFilter
        {
            Name = "DeletedDate",
            Operator = FilterOperators.IsNull
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
    }

    [Test]
    public void EntityFilterWithArrayValueRoundTripSerialization()
    {
        // Arrange - create a filter with an array value for In operator
        var values = new[] { 1, 2, 3, 4, 5 };
        var filter = new EntityFilter
        {
            Name = "CategoryId",
            Operator = FilterOperators.In,
            Value = values
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<int[]>();

        var deserializedArray = (int[])deserialized.Value!;
        deserializedArray.Should().HaveCount(5);
        deserializedArray[0].Should().Be(1);
        deserializedArray[1].Should().Be(2);
        deserializedArray[2].Should().Be(3);
        deserializedArray[3].Should().Be(4);
        deserializedArray[4].Should().Be(5);
    }

    [Test]
    public void EntityFilterWithStringArrayValueRoundTripSerialization()
    {
        // Arrange - create a filter with a string array value
        var values = new[] { "Active", "Pending", "InProgress" };
        var filter = new EntityFilter
        {
            Name = "Status",
            Operator = FilterOperators.In,
            Value = values
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<string[]>();

        var deserializedArray = (string[])deserialized.Value!;
        deserializedArray.Should().HaveCount(3);
        deserializedArray[0].Should().Be("Active");
        deserializedArray[1].Should().Be("Pending");
        deserializedArray[2].Should().Be("InProgress");
    }

    [Test]
    public void EntityFilterWithListValueRoundTripSerialization()
    {
        // Arrange - create a filter with a List value
        var values = new List<int> { 10, 20, 30 };
        var filter = new EntityFilter
        {
            Name = "Price",
            Operator = FilterOperators.NotIn,
            Value = values
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<List<int>>();

        var deserializedList = (List<int>)deserialized.Value!;
        deserializedList.Should().HaveCount(3);
        deserializedList[0].Should().Be(10);
        deserializedList[1].Should().Be(20);
        deserializedList[2].Should().Be(30);
    }

    [Test]
    public void EntityFilterWithNullableListValueRoundTripSerialization()
    {
        // Arrange - create a filter with a List value
        var values = new List<int?> { 10, 20, 30 };
        var filter = new EntityFilter
        {
            Name = "Price",
            Operator = FilterOperators.NotIn,
            Value = values
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<List<int?>>();

        var deserializedList = (List<int?>)deserialized.Value!;
        deserializedList.Should().HaveCount(3);
        deserializedList[0].Should().Be(10);
        deserializedList[1].Should().Be(20);
        deserializedList[2].Should().Be(30);
    }

    [Test]
    public void EntityFilterWithMixedTypeCollectionRoundTripSerialization()
    {
        // Arrange - create a filter with mixed type collection
        var values = new object[] { 1, "two", 3.0, true };
        var filter = new EntityFilter
        {
            Name = "MixedField",
            Operator = FilterOperators.In,
            Value = values
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<object[]>();

        var deserializedArray = (object[])deserialized.Value!;
        deserializedArray.Should().HaveCount(4);
        deserializedArray[0].Should().Be(1);
        deserializedArray[1].Should().Be("two");
        deserializedArray[2].Should().Be(3.0);
        deserializedArray[3].Should().Be(true);
    }

    [Test]
    public void EntityFilterWithEmptyCollectionRoundTripSerialization()
    {
        // Arrange - create a filter with an empty collection
        var values = new int[] { };
        var filter = new EntityFilter
        {
            Name = "Tags",
            Operator = FilterOperators.In,
            Value = values
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<int[]>();

        var deserializedArray = (int[])deserialized.Value!;
        deserializedArray.Should().BeEmpty();
    }

    [Test]
    public void EntityFilterWithNestedCollectionInGroupRoundTripSerialization()
    {
        // Arrange - create a group filter with nested filters containing collections
        var filter = new EntityFilter
        {
            Logic = FilterLogic.Or,
            Filters = new List<EntityFilter>
            {
                new EntityFilter
                {
                    Name = "CategoryId",
                    Operator = FilterOperators.In,
                    Value = new[] { 1, 2, 3 }
                },
                new EntityFilter
                {
                    Name = "Status",
                    Operator = FilterOperators.NotIn,
                    Value = new[] { "Deleted", "Archived" }
                }
            }
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Logic.Should().Be(FilterLogic.Or);
        deserialized.Filters.Should().NotBeNull();
        deserialized.Filters.Should().HaveCount(2);

        // Check first filter with integer array - type is preserved
        deserialized.Filters![0].Name.Should().Be("CategoryId");
        deserialized.Filters[0].Operator.Should().Be(FilterOperators.In);
        deserialized.Filters[0].Value.Should().BeOfType<int[]>();
        var firstArray = (int[])deserialized.Filters[0].Value!;
        firstArray.Should().HaveCount(3);
        firstArray[0].Should().Be(1);
        firstArray[1].Should().Be(2);
        firstArray[2].Should().Be(3);

        // Check second filter with string array - type is preserved
        deserialized.Filters[1].Name.Should().Be("Status");
        deserialized.Filters[1].Operator.Should().Be(FilterOperators.NotIn);
        deserialized.Filters[1].Value.Should().BeOfType<string[]>();
        var secondArray = (string[])deserialized.Filters[1].Value!;
        secondArray.Should().HaveCount(2);
        secondArray[0].Should().Be("Deleted");
        secondArray[1].Should().Be("Archived");
    }

    [Test]
    public void EntityFilterWithDateTimeArrayValueRoundTripSerialization()
    {
        // Arrange - create a filter with DateTime array (type not explicitly handled in old version)
        var dates = new[] { new DateTime(2024, 1, 1), new DateTime(2024, 12, 31) };
        var filter = new EntityFilter
        {
            Name = "EventDate",
            Operator = FilterOperators.In,
            Value = dates
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<DateTime[]>();

        var deserializedArray = (DateTime[])deserialized.Value!;
        deserializedArray.Should().HaveCount(2);
        deserializedArray[0].Should().Be(new DateTime(2024, 1, 1));
        deserializedArray[1].Should().Be(new DateTime(2024, 12, 31));
    }

    [Test]
    public void EntityFilterWithGuidArrayValueRoundTripSerialization()
    {
        // Arrange - create a filter with Guid array (type not explicitly handled in old version)
        var guids = new[] { Guid.Parse("12345678-1234-1234-1234-123456789012"), Guid.Parse("87654321-4321-4321-4321-210987654321") };
        var filter = new EntityFilter
        {
            Name = "EntityId",
            Operator = FilterOperators.In,
            Value = guids
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<Guid[]>();

        var deserializedArray = (Guid[])deserialized.Value!;
        deserializedArray.Should().HaveCount(2);
        deserializedArray[0].Should().Be(Guid.Parse("12345678-1234-1234-1234-123456789012"));
        deserializedArray[1].Should().Be(Guid.Parse("87654321-4321-4321-4321-210987654321"));
    }

    [Test]
    public void EntityFilterWithDecimalArrayValueRoundTripSerialization()
    {
        // Arrange - create a filter with decimal array (type not explicitly handled in old version)
        var prices = new[] { 99.99m, 149.50m, 299.00m };
        var filter = new EntityFilter
        {
            Name = "Price",
            Operator = FilterOperators.In,
            Value = prices
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(filter, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityFilter>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(filter.Name);
        deserialized.Operator.Should().Be(filter.Operator);
        deserialized.Value.Should().NotBeNull();
        deserialized.Value.Should().BeOfType<decimal[]>();

        var deserializedArray = (decimal[])deserialized.Value!;
        deserializedArray.Should().HaveCount(3);
        deserializedArray[0].Should().Be(99.99m);
        deserializedArray[1].Should().Be(149.50m);
        deserializedArray[2].Should().Be(299.00m);
    }

    [Test]
    public void EntityQueryRoundTripSerialization()
    {
        // Arrange - create a basic query
        var query = new EntityQuery
        {
            Query = "search text",
            Page = 2,
            PageSize = 50
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(query, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityQuery>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Query.Should().Be(query.Query);
        deserialized.Page.Should().Be(query.Page);
        deserialized.PageSize.Should().Be(query.PageSize);
    }

    [Test]
    public void EntityQueryWithFilterAndSortRoundTripSerialization()
    {
        // Arrange - create a complex query with filter and sort
        var query = new EntityQuery
        {
            Query = "search text",
            Filter = new EntityFilter
            {
                Logic = FilterLogic.And,
                Filters = new List<EntityFilter>
                {
                    new EntityFilter { Name = "Status", Operator = FilterOperators.Equal, Value = "Active" },
                    new EntityFilter { Name = "Priority", Operator = FilterOperators.GreaterThanOrEqual, Value = 3 }
                }
            },
            Sort = new List<EntitySort>
            {
                new EntitySort { Name = "CreatedDate", Direction = SortDirections.Descending },
                new EntitySort { Name = "Name", Direction = SortDirections.Ascending }
            },
            Page = 1,
            PageSize = 20
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(query, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityQuery>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Query.Should().Be(query.Query);
        deserialized.Filter.Should().NotBeNull();
        deserialized.Filter!.Logic.Should().Be(FilterLogic.And);
        deserialized.Filter.Filters.Should().HaveCount(2);
        deserialized.Sort.Should().NotBeNull();
        deserialized.Sort.Should().HaveCount(2);
        deserialized.Sort![0].Name.Should().Be("CreatedDate");
        deserialized.Sort[0].Direction.Should().Be(SortDirections.Descending);
        deserialized.Sort[1].Name.Should().Be("Name");
        deserialized.Sort[1].Direction.Should().Be(SortDirections.Ascending);
        deserialized.Page.Should().Be(query.Page);
        deserialized.PageSize.Should().Be(query.PageSize);
    }

    [Test]
    public void EntityQueryWithContinuationTokenRoundTripSerialization()
    {
        // Arrange - create a query with continuation token
        var query = new EntityQuery
        {
            ContinuationToken = "eyJwYWdlIjoyLCJpZCI6MTIzfQ==",
            PageSize = 25
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(query, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityQuery>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.ContinuationToken.Should().Be(query.ContinuationToken);
        deserialized.PageSize.Should().Be(query.PageSize);
    }

    [Test]
    public void EntityPagedResultRoundTripSerialization()
    {
        // Arrange - create a paged result with simple data
        var result = new EntityPagedResult<string>
        {
            Total = 100,
            Data = new List<string> { "Item1", "Item2", "Item3" }
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(result, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityPagedResult<string>>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Total.Should().Be(result.Total);
        deserialized.Data.Should().NotBeNull();
        deserialized.Data.Should().HaveCount(3);
        deserialized.Data.Should().ContainInOrder("Item1", "Item2", "Item3");
    }

    [Test]
    public void EntityPagedResultWithComplexTypeRoundTripSerialization()
    {
        // Arrange - create a paged result with complex objects
        var result = new EntityPagedResult<TestModel>
        {
            Total = 50,
            Data = new List<TestModel>
            {
                new TestModel { Id = 1, Name = "Test1", Status = "Active" },
                new TestModel { Id = 2, Name = "Test2", Status = "Inactive" }
            },
            ContinuationToken = "nextPageToken123"
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(result, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityPagedResult<TestModel>>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Total.Should().Be(result.Total);
        deserialized.ContinuationToken.Should().Be(result.ContinuationToken);
        deserialized.Data.Should().NotBeNull();
        deserialized.Data.Should().HaveCount(2);
        deserialized.Data![0].Id.Should().Be(1);
        deserialized.Data[0].Name.Should().Be("Test1");
        deserialized.Data[0].Status.Should().Be("Active");
        deserialized.Data[1].Id.Should().Be(2);
    }

    [Test]
    public void EntityPagedResultEmptyRoundTripSerialization()
    {
        // Arrange - create an empty paged result
        var result = new EntityPagedResult<int>
        {
            Total = 0,
            Data = new List<int>()
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(result, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityPagedResult<int>>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Total.Should().Be(0);
        deserialized.Data.Should().NotBeNull();
        deserialized.Data.Should().BeEmpty();
    }

    [Test]
    public void EntityPagedResultWithNullDataRoundTripSerialization()
    {
        // Arrange - create a paged result with null data
        var result = new EntityPagedResult<string>
        {
            Total = null,
            Data = null,
            ContinuationToken = null
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(result, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<EntityPagedResult<string>>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Total.Should().BeNull();
        deserialized.Data.Should().BeNull();
        deserialized.ContinuationToken.Should().BeNull();
    }

    [Test]
    public void ProblemDetailsBasicRoundTripSerialization()
    {
        // Arrange - create a basic problem details
        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/problems/validation-error",
            Title = "Validation Error",
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Instance = "/api/entities/123"
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(problemDetails, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<ProblemDetails>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Type.Should().Be(problemDetails.Type);
        deserialized.Title.Should().Be(problemDetails.Title);
        deserialized.Status.Should().Be(problemDetails.Status);
        deserialized.Detail.Should().Be(problemDetails.Detail);
        deserialized.Instance.Should().Be(problemDetails.Instance);
    }

    [Test]
    public void ProblemDetailsWithErrorsRoundTripSerialization()
    {
        // Arrange - create problem details with validation errors
        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/problems/validation-error",
            Title = "Validation Error",
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Errors = new Dictionary<string, string[]>
            {
                ["Name"] = ["The Name field is required.", "The Name field must be at least 3 characters."],
                ["Email"] = ["The Email field is not a valid e-mail address."],
                ["Age"] = ["The Age field must be between 0 and 120."]
            }
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(problemDetails, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<ProblemDetails>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Type.Should().Be(problemDetails.Type);
        deserialized.Title.Should().Be(problemDetails.Title);
        deserialized.Status.Should().Be(problemDetails.Status);
        deserialized.Detail.Should().Be(problemDetails.Detail);
        deserialized.Errors.Should().NotBeNull();
        deserialized.Errors.Should().HaveCount(3);
        deserialized.Errors["Name"].Should().HaveCount(2);
        deserialized.Errors["Name"][0].Should().Be("The Name field is required.");
        deserialized.Errors["Name"][1].Should().Be("The Name field must be at least 3 characters.");
        deserialized.Errors["Email"].Should().HaveCount(1);
        deserialized.Errors["Email"][0].Should().Be("The Email field is not a valid e-mail address.");
        deserialized.Errors["Age"].Should().HaveCount(1);
        deserialized.Errors["Age"][0].Should().Be("The Age field must be between 0 and 120.");
    }

    [Test]
    public void ProblemDetailsWithExtensionsRoundTripSerialization()
    {
        // Arrange - create problem details with extension data
        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/problems/server-error",
            Title = "Internal Server Error",
            Status = 500,
            Detail = "An unexpected error occurred while processing the request.",
            Extensions = new Dictionary<string, object?>
            {
                ["traceId"] = "0HMVFE0A2S7UT:00000001",
                ["requestId"] = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                ["timestamp"] = new DateTime(2024, 1, 15, 10, 30, 0),
                ["customData"] = new { code = 1001, severity = "high" }
            }
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(problemDetails, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<ProblemDetails>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Type.Should().Be(problemDetails.Type);
        deserialized.Title.Should().Be(problemDetails.Title);
        deserialized.Status.Should().Be(problemDetails.Status);
        deserialized.Detail.Should().Be(problemDetails.Detail);
        deserialized.Extensions.Should().NotBeNull();
        deserialized.Extensions.Should().HaveCount(4);
        deserialized.Extensions["traceId"].Should().Be("0HMVFE0A2S7UT:00000001");
        deserialized.Extensions["requestId"].Should().Be(Guid.Parse("12345678-1234-1234-1234-123456789012"));
        deserialized.Extensions["timestamp"].Should().Be(new DateTime(2024, 1, 15, 10, 30, 0));
        deserialized.Extensions["customData"].Should().NotBeNull();
    }

    [Test]
    public void ProblemDetailsWithNullPropertiesRoundTripSerialization()
    {
        // Arrange - create problem details with null optional properties
        var problemDetails = new ProblemDetails
        {
            Type = null,
            Title = "Error",
            Status = null,
            Detail = null,
            Instance = null
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(problemDetails, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<ProblemDetails>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Type.Should().BeNull();
        deserialized.Title.Should().Be("Error");
        deserialized.Status.Should().BeNull();
        deserialized.Detail.Should().BeNull();
        deserialized.Instance.Should().BeNull();
    }

    [Test]
    public void ProblemDetailsCompleteRoundTripSerialization()
    {
        // Arrange - create a complete problem details with all properties
        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/problems/complex-error",
            Title = "Complex Error",
            Status = 422,
            Detail = "The request could not be processed due to multiple issues.",
            Instance = "/api/orders/456",
            Errors = new Dictionary<string, string[]>
            {
                ["Items"] = ["At least one item is required."],
                ["ShippingAddress"] = ["Invalid postal code.", "State is required."]
            },
            Extensions = new Dictionary<string, object?>
            {
                ["correlationId"] = "abc-123-def-456",
                ["retryable"] = false,
                ["estimatedResolution"] = "Contact support"
            }
        };

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(problemDetails, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<ProblemDetails>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Type.Should().Be(problemDetails.Type);
        deserialized.Title.Should().Be(problemDetails.Title);
        deserialized.Status.Should().Be(problemDetails.Status);
        deserialized.Detail.Should().Be(problemDetails.Detail);
        deserialized.Instance.Should().Be(problemDetails.Instance);
        deserialized.Errors.Should().HaveCount(2);
        deserialized.Errors["Items"].Should().HaveCount(1);
        deserialized.Errors["ShippingAddress"].Should().HaveCount(2);
        deserialized.Extensions.Should().HaveCount(3);
        deserialized.Extensions["correlationId"].Should().Be("abc-123-def-456");
        deserialized.Extensions["retryable"].Should().Be(false);
        deserialized.Extensions["estimatedResolution"].Should().Be("Contact support");
    }

    [Test]
    public void ProblemDetailsEmptyRoundTripSerialization()
    {
        // Arrange - create an empty problem details
        var problemDetails = new ProblemDetails();

        // Act - serialize and deserialize
        var bytes = MessagePackSerializer.Serialize(problemDetails, MessagePackDefaults.DefaultSerializerOptions);
        var deserialized = MessagePackSerializer.Deserialize<ProblemDetails>(bytes, MessagePackDefaults.DefaultSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Type.Should().BeNull();
        deserialized.Title.Should().BeNull();
        deserialized.Status.Should().BeNull();
        deserialized.Detail.Should().BeNull();
        deserialized.Instance.Should().BeNull();
        deserialized.Errors.Should().NotBeNull();
        deserialized.Errors.Should().BeEmpty();
        deserialized.Extensions.Should().NotBeNull();
        deserialized.Extensions.Should().BeEmpty();
    }

    // Helper class for testing complex type serialization
    [MessagePackObject(keyAsPropertyName: true, AllowPrivate = true)]
    internal class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
