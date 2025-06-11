using Arbiter.CommandQuery.Queries;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Arbiter.Benchmarks.Query;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class LinqExpressionBuilderBenchmark
{
    private LinqExpressionBuilder _builder = null!;
    private EntityFilter _simpleFilter = null!;
    private EntityFilter _complexFilter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _builder = new LinqExpressionBuilder();

        _simpleFilter = new EntityFilter
        {
            Name = "Name",
            Operator = EntityFilterOperators.Equal,
            Value = "Test"
        };

        _complexFilter = new EntityFilter
        {
            Logic = "and",
            Filters = new List<EntityFilter>
            {
                new EntityFilter
                {
                    Name = "Age",
                    Operator = EntityFilterOperators.GreaterThan,
                    Value = 18
                },
                new EntityFilter
                {
                    Name = "IsActive",
                    Operator = EntityFilterOperators.Equal,
                    Value = true
                },
                new EntityFilter
                {
                    Logic = "or",
                    Filters = new List<EntityFilter>
                    {
                        new EntityFilter
                        {
                            Name = "Country",
                            Operator = EntityFilterOperators.Equal,
                            Value = "US"
                        },
                        new EntityFilter
                        {
                            Name = "Country",
                            Operator = EntityFilterOperators.Equal,
                            Value = "CA"
                        }
                    }
                }
            }
        };
    }

    [Benchmark]
    public string BuildSimpleFilter()
    {
        _builder.Build(_simpleFilter);
        return _builder.Expression;
    }

    [Benchmark]
    public string BuildComplexFilter()
    {
        _builder.Build(_complexFilter);
        return _builder.Expression;
    }
}
