using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Tests.Queries;

public class LinqExpressionBuilderTest
{
    [Test]
    public void FilterNormal()
    {
        var entityFilter = new EntityFilter { Name = "Rank", Value = 7 };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Rank == @0");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be(7);
    }

    [Test]
    public void FilterLogicalOr()
    {
        var entityFilter = new EntityFilter
        {
            Logic = FilterLogic.Or,
            Filters =
            [
                new EntityFilter{ Name = "Rank", Value = 7 },
                new EntityFilter{ Name = "Name", Value = "Apple" }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank == @0 || Name == @1)");

        builder.Parameters.Count.Should().Be(2);
        builder.Parameters[0].Should().Be(7);
        builder.Parameters[1].Should().Be("Apple");
    }

    [Test]
    public void FilterLogicalAnd()
    {
        var entityFilter = new EntityFilter
        {
            Filters =
            [
                new EntityFilter{ Name = "Rank", Value = 7 },
                new EntityFilter{ Name = "Name", Value = "Blueberry" }
            ]

        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank == @0 && Name == @1)");

        builder.Parameters.Count.Should().Be(2);
        builder.Parameters[0].Should().Be(7);
        builder.Parameters[1].Should().Be("Blueberry");
    }

    [Test]
    public void FilterLogicalAndEmpty()
    {
        var entityFilter = new EntityFilter
        {
            Filters =
            [
                new EntityFilter(),
                new EntityFilter{ Name = "Name", Value = "Blueberry" }
            ]

        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Name == @0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("Blueberry");
    }

    [Test]
    public void FilterComplex()
    {
        var entityFilter = new EntityFilter
        {
            Filters =
            [
                new EntityFilter{ Name = "Rank", Operator = FilterOperators.GreaterThan, Value = 5 },
                new EntityFilter
                {
                    Logic = FilterLogic.Or,
                    Filters =
                    [
                        new EntityFilter{ Name = "Name", Value = "Strawberry" },
                        new EntityFilter{ Name = "Name", Value = "Blueberry" }
                    ]
                }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank > @0 && (Name == @1 || Name == @2))");

        builder.Parameters.Count.Should().Be(3);
        builder.Parameters[0].Should().Be(5);
        builder.Parameters[1].Should().Be("Strawberry");
        builder.Parameters[2].Should().Be("Blueberry");
    }

    [Test]
    public void FilterComplexEmpty()
    {
        var entityFilter = new EntityFilter
        {
            Filters =
            [
                new EntityFilter{ Name = "Rank", Operator = FilterOperators.GreaterThan, Value = 5 },
                new EntityFilter
                {
                    Logic = FilterLogic.Or,
                    Filters =
                    [
                        new EntityFilter(),
                        new EntityFilter()
                    ]
                }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank > @0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be(5);
    }

    [Test]
    public void FilterContains()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.Contains,
            Value = "Berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL && Name.Contains(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("Berry");
    }

    [Test]
    public void FilterIsNull()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.IsNull
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name == NULL");

        builder.Parameters.Count.Should().Be(0);
    }

    [Test]
    public void FilterIsNotNull()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.IsNotNull
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL");

        builder.Parameters.Count.Should().Be(0);
    }

    [Test]
    public void FilterIn()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.In,
            Value = new[] { "Test", "Tester" }
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("it.Name in @0");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().BeOfType<string[]>();
    }

    [Test]
    public void FilterNotContains()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.NotContains,
            Value = "Berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL && !Name.Contains(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("Berry");
    }

    [Test]
    public void FilterStartsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.StartsWith,
            Value = "P"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL && Name.StartsWith(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("P");
    }

    [Test]
    public void FilterNotStartsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.NotStartsWith,
            Value = "P"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL && !Name.StartsWith(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("P");
    }

    [Test]
    public void FilterEndsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.EndsWith,
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL && Name.EndsWith(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("berry");
    }

    [Test]
    public void FilterNotEndsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = FilterOperators.NotEndsWith,
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name != NULL && !Name.EndsWith(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("berry");
    }

    [Test]
    public void FilterNull()
    {
        var builder = new LinqExpressionBuilder();
        builder.Build(null);

        builder.Expression.Should().BeEmpty();

        builder.Parameters.Count.Should().Be(0);
    }

    [Test]
    public void FilterEmpty()
    {
        var entityFilter = new EntityFilter();

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().BeEmpty();

        builder.Parameters.Count.Should().Be(0);
    }

    [Test]
    public void FilterExpression()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Locations.Any(it.Id in @0)",
            Operator = FilterOperators.Expression,
            Value = new[] { 100, 200 }
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Locations.Any(it.Id in @0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().BeOfType<int[]>();
    }

    [Test]
    public void FilterExpressionComplex()
    {
        var entityFilter = new EntityFilter
        {
            Logic = FilterLogic.And,
            Filters =
            [
                new EntityFilter
                {
                    Name = "Id",
                    Value = new[] { 1000, 1001 },
                    Operator = FilterOperators.In
                },
                new EntityFilter
                {
                    Name = "Locations.Any(it.Id in @0)",
                    Value = new[] { 100, 200 },
                    Operator = FilterOperators.Expression
                }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(it.Id in @0 && Locations.Any(it.Id in @1))");

        builder.Parameters.Count.Should().Be(2);
        builder.Parameters[0].Should().BeOfType<int[]>();
        builder.Parameters[1].Should().BeOfType<int[]>();
    }
}
