using Arbiter.CommandQuery.Filters;

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
            Operator = FilterOperators.Equal,
            Value = "Test"
        };

        _complexFilter = new EntityFilter
        {
            Logic = FilterLogic.And,
            Filters = new List<EntityFilter>
            {
                new EntityFilter
                {
                    Name = "Age",
                    Operator = FilterOperators.GreaterThan,
                    Value = 18
                },
                new EntityFilter
                {
                    Name = "IsActive",
                    Operator = FilterOperators.Equal,
                    Value = true
                },
                new EntityFilter
                {
                    Logic = FilterLogic.Or,
                    Filters = new List<EntityFilter>
                    {
                        new EntityFilter
                        {
                            Name = "Country",
                            Operator = FilterOperators.Equal,
                            Value = "US"
                        },
                        new EntityFilter
                        {
                            Name = "Country",
                            Operator = FilterOperators.Equal,
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
