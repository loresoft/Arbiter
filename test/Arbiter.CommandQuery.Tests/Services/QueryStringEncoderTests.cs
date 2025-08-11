using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Services.Tests;

public class QueryStringEncoderTests
{
    [Test]
    public void EncodeEntitySelect()
    {
        var entitySelect = new EntitySelect
        {
            Sort = [new() { Name = "Updated", Direction = "Descending" }],
            Filter = new() { Name = "Description", Operator = "IsNull" }
        };

        var queryString = QueryStringEncoder.Encode(entitySelect);
        queryString.Should().NotBeNullOrWhiteSpace();

        var resultSelect = QueryStringEncoder.Decode<EntitySelect>(queryString);
        resultSelect.Should().NotBeNull();
        resultSelect.Filter.Should().NotBeNull();
        resultSelect.Filter.Name.Should().Be("Description");
        resultSelect.Filter.Operator.Should().Be("IsNull");

        resultSelect.Sort.Should().NotBeNullOrEmpty();
        resultSelect.Sort[0].Name.Should().Be("Updated");
        resultSelect.Sort[0].Direction.Should().Be("Descending");
    }

    [Test]
    public void EncodeNullValue()
    {
        string? queryString = QueryStringEncoder.Encode<object>(null!);
        queryString.Should().BeNull();
    }

    [Test]
    public void DecodeNullValue()
    {
        var result = QueryStringEncoder.Decode<object>(null);
        result.Should().BeNull();
    }

    [Test]
    public void EncodeAndDecodeEmptyObject()
    {
        var emptyObject = new { };

        var queryString = QueryStringEncoder.Encode(emptyObject);
        queryString.Should().NotBeNullOrWhiteSpace();

        var result = QueryStringEncoder.Decode<object>(queryString);
        result.Should().NotBeNull();
    }

    [Test]
    public void EncodeAndDecodeComplexObject()
    {
        var complexObject = new ComplexObject
        {
            Name = "Test",
            Details = new DetailsObject
            {
                Age = 30,
                Address = "123 Main St"
            },
            Tags = new[] { "tag1", "tag2" }
        };

        var queryString = QueryStringEncoder.Encode(complexObject);
        queryString.Should().NotBeNullOrWhiteSpace();

        var result = QueryStringEncoder.Decode<ComplexObject>(queryString);
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.Details.Age.Should().Be(30);
        result.Details.Address.Should().Be("123 Main St");
        result.Tags[0].Should().Be("tag1");
        result.Tags[1].Should().Be("tag2");
    }

    [Test]
    public void EncodeAndDecodeLargeObject()
    {
        var largeObject = new LargeObject
        {
            Name = new string('A', 10000), // Large string
            Numbers = Enumerable.Range(1, 10000).ToArray(), // Large array
            Nested = Enumerable.Range(1, 1000).Select(i => new NestedObject { Index = i, Value = new string('B', 100) }).ToArray() // Large nested structure
        };

        var queryString = QueryStringEncoder.Encode(largeObject);
        queryString.Should().NotBeNullOrWhiteSpace();

        var result = QueryStringEncoder.Decode<LargeObject>(queryString);
        result.Should().NotBeNull();
        result.Name.Should().Be(new string('A', 10000));
        result.Numbers.Length.Should().Be(10000);
        result.Nested.Length.Should().Be(1000);
        result.Nested[0].Index.Should().Be(1);
        result.Nested[0].Value.Should().Be(new string('B', 100));
    }
}

public class LargeObject
{
    public string Name { get; set; } = string.Empty;
    public int[] Numbers { get; set; } = Array.Empty<int>();
    public NestedObject[] Nested { get; set; } = Array.Empty<NestedObject>();
}

public class NestedObject
{
    public int Index { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class ComplexObject
{
    public string Name { get; set; } = string.Empty;
    public DetailsObject Details { get; set; } = new DetailsObject();
    public string[] Tags { get; set; } = Array.Empty<string>();
}

public class DetailsObject
{
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
}
