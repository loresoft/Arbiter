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
            Logic = "or",
            Filters =
            [
                new EntityFilter{ Name = "Rank", Value = 7 },
                new EntityFilter{ Name = "Name", Value = "Apple" }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(Rank == @0 or Name == @1)");

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
        builder.Expression.Should().Be("(Rank == @0 and Name == @1)");

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
                new EntityFilter{ Name = "Rank", Operator = ">", Value = 5 },
                new EntityFilter
                {
                    Logic = "or",
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
        builder.Expression.Should().Be("(Rank > @0 and (Name == @1 or Name == @2))");

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
                new EntityFilter{ Name = "Rank", Operator = ">", Value = 5 },
                new EntityFilter
                {
                    Logic = "or",
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
            Operator = "Contains",
            Value = "Berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name.Contains(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("Berry");
    }

    [Test]
    public void FilterIsNull()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = EntityFilterOperators.IsNull
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
            Operator = EntityFilterOperators.IsNotNull
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
            Operator = "in",
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
            Operator = "!Contains",
            Value = "Berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("!Name.Contains(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("Berry");
    }

    [Test]
    public void FilterStartsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = "StartsWith",
            Value = "P"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name.StartsWith(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("P");
    }

    [Test]
    public void FilterNotStartsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = "!StartsWith",
            Value = "P"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("!Name.StartsWith(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("P");
    }

    [Test]
    public void FilterEndsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = "EndsWith",
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("Name.EndsWith(@0)");

        builder.Parameters.Count.Should().Be(1);
        builder.Parameters[0].Should().Be("berry");
    }

    [Test]
    public void FilterNotEndsWith()
    {
        var entityFilter = new EntityFilter
        {
            Name = "Name",
            Operator = "!EndsWith",
            Value = "berry"
        };
        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("!Name.EndsWith(@0)");

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
            Operator = EntityFilterOperators.Expression,
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
            Logic = EntityFilterLogic.And,
            Filters =
            [
                new EntityFilter
                {
                    Name = "Id",
                    Value = new[] { 1000, 1001 },
                    Operator = EntityFilterOperators.In
                },
                new EntityFilter
                {
                    Name = "Locations.Any(it.Id in @0)",
                    Value = new[] { 100, 200 },
                    Operator = EntityFilterOperators.Expression
                }
            ]
        };

        var builder = new LinqExpressionBuilder();
        builder.Build(entityFilter);

        builder.Expression.Should().NotBeEmpty();
        builder.Expression.Should().Be("(it.Id in @0 and Locations.Any(it.Id in @1))");

        builder.Parameters.Count.Should().Be(2);
        builder.Parameters[0].Should().BeOfType<int[]>();
        builder.Parameters[1].Should().BeOfType<int[]>();
    }
}
