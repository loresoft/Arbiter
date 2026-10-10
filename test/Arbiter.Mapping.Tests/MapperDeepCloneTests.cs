namespace Arbiter.Mapping.Tests;

public class MapperDeepCloneTests
{
    [Test]
    public void Map_UnconfiguredNested_AssignsByReference()
    {
        var source = CreateOrder();
        var mapper = new OrderReferenceMapper();

        var result = mapper.Map(source);

        result.Should().NotBeNull();
        result!.Customer.Should().BeSameAs(source.Customer);
        result.Lines.Should().BeSameAs(source.Lines);
    }

    [Test]
    public void Map_TypeLevelMap_DeepClonesNestedObjectsAndCollections()
    {
        var source = CreateOrder();
        var mapper = new OrderCloneMapper();

        var result = mapper.Map(source);

        result.Should().NotBeNull();
        result!.Customer.Should().NotBeSameAs(source.Customer);
        result.Customer!.Name.Should().Be("Alice");
        result.Customer.Address.Should().NotBeSameAs(source.Customer!.Address);
        result.Customer.Address!.City.Should().Be("Springfield");

        result.Lines.Should().NotBeSameAs(source.Lines);
        result.Lines.Should().HaveCount(2);
        result.Lines[0].Should().NotBeSameAs(source.Lines[0]);
        result.Lines[0].Product.Should().Be("Widget");
        result.Lines[0].Quantity.Should().Be(2);

        result.Tags.Should().NotBeSameAs(source.Tags);
        result.Tags["primary"].Should().NotBeSameAs(source.Tags["primary"]);
        result.Tags["primary"].Value.Should().Be("vip");
    }

    [Test]
    public void Map_NestedConfiguration_AppliesCustomPropertyMapping()
    {
        var source = CreateOrder();
        var mapper = new OrderCustomNestedMapper();

        var result = mapper.Map(source);

        result.Should().NotBeNull();
        result!.Customer!.Name.Should().Be("ALICE");
        result.Customer.Address.Should().BeSameAs(source.Customer!.Address);
    }

    [Test]
    public void Map_MapWith_ClonesOnlyConfiguredProperty()
    {
        var source = CreateOrder();
        var mapper = new OrderMapWithMapper();

        var result = mapper.Map(source);

        result.Should().NotBeNull();
        result!.Customer.Should().NotBeSameAs(source.Customer);
        result.Customer!.Name.Should().Be("Alice");
        result.Lines.Should().BeSameAs(source.Lines);
    }

    [Test]
    public void Map_NullNested_ReturnsNull()
    {
        var source = CreateOrder();
        source.Customer = null;
        var mapper = new OrderCloneMapper();

        var result = mapper.Map(source);

        result!.Customer.Should().BeNull();
    }

    [Test]
    public void Map_CopyToExisting_DeepClonesNested()
    {
        var source = CreateOrder();
        var destination = new CloneOrder();
        var mapper = new OrderCloneMapper();

        mapper.Map(source, destination);

        destination.Customer.Should().NotBeSameAs(source.Customer);
        destination.Customer!.Name.Should().Be("Alice");
        destination.Lines.Should().HaveCount(2);
    }

    [Test]
    public void ProjectTo_TypeLevelMap_ProjectsNestedObjectsAndCollections()
    {
        var source = CreateOrder();
        var mapper = new OrderCloneMapper();
        var query = new[] { source }.AsQueryable();

        var result = mapper.ProjectTo(query).Single();

        result.Customer.Should().NotBeSameAs(source.Customer);
        result.Customer!.Address!.City.Should().Be("Springfield");
        result.Lines.Should().HaveCount(2);
        result.Lines[1].Product.Should().Be("Gadget");
    }

    [Test]
    public void Map_SelfReferencingConfiguredType_StopsRecursion()
    {
        var source = new CloneNode { Name = "root", Child = new CloneNode { Name = "child" } };
        var mapper = new NodeCloneMapper();

        var result = mapper.Map(source);

        result.Should().NotBeNull();
        result!.Name.Should().Be("root");
        result.Child.Should().BeSameAs(source.Child);
    }

    private static CloneOrder CreateOrder()
    {
        var address = new CloneAddress { Street = "1 Main St", City = "Springfield" };
        var customer = new CloneCustomer { Name = "Alice", Address = address };

        var lines = new List<CloneOrderLine>
        {
            new() { Product = "Widget", Quantity = 2 },
            new() { Product = "Gadget", Quantity = 1 },
        };

        var tags = new Dictionary<string, CloneTag>
        {
            ["primary"] = new() { Value = "vip" },
        };

        return new CloneOrder
        {
            Id = 1,
            Customer = customer,
            Lines = lines,
            Tags = tags,
        };
    }
}

public class CloneOrder
{
    public int Id { get; set; }
    public CloneCustomer? Customer { get; set; }
    public List<CloneOrderLine> Lines { get; set; } = [];
    public Dictionary<string, CloneTag> Tags { get; set; } = [];
}

public class CloneCustomer
{
    public string Name { get; set; } = string.Empty;
    public CloneAddress? Address { get; set; }
}

public class CloneAddress
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class CloneOrderLine
{
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CloneTag
{
    public string Value { get; set; } = string.Empty;
}

public class CloneNode
{
    public string Name { get; set; } = string.Empty;
    public CloneNode? Child { get; set; }
}

[GenerateMapper]
public partial class OrderReferenceMapper : MapperProfile<CloneOrder, CloneOrder>;

[GenerateMapper]
public partial class OrderCloneMapper : MapperProfile<CloneOrder, CloneOrder>
{
    protected override void ConfigureMapping(MappingBuilder<CloneOrder, CloneOrder> mapping)
    {
        mapping.Map<CloneCustomer, CloneCustomer>();
        mapping.Map<CloneAddress, CloneAddress>();
        mapping.Map<CloneOrderLine, CloneOrderLine>();
        mapping.Map<CloneTag, CloneTag>();
    }
}

[GenerateMapper]
public partial class OrderCustomNestedMapper : MapperProfile<CloneOrder, CloneOrder>
{
    protected override void ConfigureMapping(MappingBuilder<CloneOrder, CloneOrder> mapping)
    {
        mapping.Map<CloneCustomer, CloneCustomer>(customer =>
        {
            customer.Property(d => d.Name).From(s => s.Name.ToUpperInvariant());
        });
    }
}

[GenerateMapper]
public partial class OrderMapWithMapper : MapperProfile<CloneOrder, CloneOrder>
{
    protected override void ConfigureMapping(MappingBuilder<CloneOrder, CloneOrder> mapping)
    {
        mapping.Property(d => d.Customer).MapWith<CloneCustomer, CloneCustomer>();
    }
}

#pragma warning disable ARB0007 // expected: self-referencing configured type stops recursion
[GenerateMapper]
public partial class NodeCloneMapper : MapperProfile<CloneNode, CloneNode>
{
    protected override void ConfigureMapping(MappingBuilder<CloneNode, CloneNode> mapping)
    {
        mapping.Map<CloneNode, CloneNode>();
    }
}
#pragma warning restore ARB0007
