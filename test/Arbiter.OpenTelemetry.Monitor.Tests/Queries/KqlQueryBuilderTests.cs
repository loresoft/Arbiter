using Arbiter.CommandQuery.Queries;
using Arbiter.OpenTelemetry.Monitor.Queries;

namespace Arbiter.OpenTelemetry.Monitor.Tests.Queries;

public class KqlQueryBuilderTests
{
    [Test]
    public async Task BuildWithMultipleSourceTablesUnionsSources()
    {
        var builder = new KqlQueryBuilder(["AppTraces", "AppExceptions"]);

        var query = builder.Build(filter: null, pageSize: 50);

        query.Should().Be(
            """
            union AppTraces, AppExceptions
            | top 51 by TimeGenerated desc
            """
        );
    }

    [Test]
    public async Task BuildWithConfigurableSourceTablesUsesConfiguredSources()
    {
        var builder = new KqlQueryBuilder(["AppRequests", "AppDependencies"]);

        var query = builder.Build(filter: null, pageSize: 25);

        query.Should().StartWith("union AppRequests, AppDependencies");
        query.Should().EndWith("| top 26 by TimeGenerated desc");
    }

    [Test]
    public async Task ConstructorWithEmptySourceTablesThrowsArgumentException()
    {
        var act = () => new KqlQueryBuilder([]);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("sourceTables");
    }

    [Test]
    public async Task ConstructorWithInvalidSourceTableThrowsArgumentException()
    {
        var act = () => new KqlQueryBuilder(["AppTraces", "Invalid-Table"]);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("sourceTables");
    }

    [Test]
    public async Task BuildWithColumnFilterAddsWhereWithoutExtend()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "SeverityLevel",
            Operator = FilterOperators.GreaterThanOrEqual,
            Value = 2
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be(
            """
            union AppTraces
            | where SeverityLevel >= 2
            | top 11 by TimeGenerated desc
            """
        );
    }

    [Test]
    public async Task BuildWithNullFilterOmitsWhereClause()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);

        var query = builder.Build(filter: null, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithZeroPageSizeThrowsArgumentOutOfRangeException()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);

        var act = () => builder.Build(filter: null, pageSize: 0);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("pageSize");
    }

    [Test]
    public async Task BuildWithDefaultOperatorUsesEqualComparison()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "SeverityLevel",
            Value = 4
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where SeverityLevel == 4
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithInvalidTopLevelPropertyNameOmitsWhereClause()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "Invalid-Column",
            Operator = FilterOperators.Equal,
            Value = "ignored"
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithPropertyNameStartingWithNumberOmitsWhereClause()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "1Column",
            Operator = FilterOperators.Equal,
            Value = "ignored"
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithKeyedPropertiesFilterAddsDynamicAliasExtend()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "Properties",
            Key = "RequestId",
            Operator = FilterOperators.Equal,
            Value = "abc-123"
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be(
            """
            union AppTraces
            | where tostring(Properties["RequestId"]) == 'abc-123'
            | top 11 by TimeGenerated desc
            """
        );
    }

    [Test]
    public async Task BuildWithKeyedNonPropertiesFilterUsesNameAsDynamicColumn()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "Attributes",
            Key = "http.method",
            Operator = FilterOperators.Equal,
            Value = "GET"
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Contain("| where tostring(Attributes[\"http.method\"]) == 'GET'");
    }

    [Test]
    public async Task BuildWithKeyedFilterEscapesIndexerKey()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "Properties",
            Key = "path\\\"id",
            Operator = FilterOperators.Equal,
            Value = "abc"
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Contain("| where tostring(Properties[\"path\\\\\\\"id\"]) == 'abc'");
    }

    [Test]
    public async Task BuildWithGroupedFiltersUsesLogicalExpressionAndAliases()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Logic = FilterLogic.Or,
            Filters =
            [
                new EntityFilter
                {
                    Name = "SeverityLevel",
                    Operator = FilterOperators.GreaterThan,
                    Value = 3
                },
                new EntityFilter
                {
                    Name = "Properties",
                    Key = "Category",
                    Operator = FilterOperators.Contains,
                    Value = "Order"
                }
            ]
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where (SeverityLevel > 3 or tostring(Properties["Category"]) has 'Order')
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithNestedGroupedFiltersWritesTreeExpression()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Filters =
            [
                new EntityFilter
                {
                    Logic = FilterLogic.Or,
                    Filters =
                    [
                        new EntityFilter
                        {
                            Name = "SeverityLevel",
                            Operator = FilterOperators.GreaterThan,
                            Value = 3
                        },
                        new EntityFilter
                        {
                            Name = "Message",
                            Operator = FilterOperators.Contains,
                            Value = "warning"
                        }
                    ]
                },
                new EntityFilter
                {
                    Name = "ResourceId",
                    Operator = FilterOperators.NotEqual,
                    Value = "excluded"
                }
            ]
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where ((SeverityLevel > 3 or tostring(Message) has 'warning') and tostring(ResourceId) != 'excluded')
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithSingleValidNestedFilterOmitsGroupParentheses()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Filters =
            [
                new EntityFilter
                {
                    Name = "Message",
                    Operator = FilterOperators.Contains,
                    Value = "error"
                },
                new EntityFilter
                {
                    Name = "Invalid Column",
                    Operator = FilterOperators.Equal,
                    Value = "ignored"
                }
            ]
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where tostring(Message) has 'error'
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithEscapedStringLiteralEscapesSingleQuote()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "Message",
            Operator = FilterOperators.Equal,
            Value = "can't"
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where tostring(Message) == 'can''t'
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithEscapedCharLiteralEscapesSingleQuote()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "Symbol",
            Operator = FilterOperators.Equal,
            Value = '\''
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where tostring(Symbol) == ''''
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithBooleanValueWritesKqlBooleanLiteral()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "IsError",
            Operator = FilterOperators.Equal,
            Value = true
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where IsError == true
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithDateTimeOffsetValueWritesKqlDateTimeLiteral()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "TimeGenerated",
            Operator = FilterOperators.LessThanOrEqual,
            Value = new DateTimeOffset(2024, 6, 2, 1, 2, 3, TimeSpan.Zero)
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where TimeGenerated <= datetime(2024-06-02T01:02:03.0000000+00:00)
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithGuidValueWritesKqlGuidLiteral()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "OperationId",
            Operator = FilterOperators.Equal,
            Value = Guid.Parse("11111111-2222-3333-4444-555555555555")
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where OperationId == guid(11111111-2222-3333-4444-555555555555)
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithUnsignedLongValueWritesInvariantNumberLiteral()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "ItemCount",
            Operator = FilterOperators.GreaterThan,
            Value = ulong.MaxValue
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where ItemCount > 18446744073709551615
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithNullOperatorWritesIsNullFunction()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "ParentId",
            Operator = FilterOperators.IsNull
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where isnull(ParentId)
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithNotNullOperatorWritesIsNotNullFunction()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Name = "ParentId",
            Operator = FilterOperators.IsNotNull
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Be("""
            union AppTraces
            | where isnotnull(ParentId)
            | top 11 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithCursorAddsCursorWhereBeforeFilterWhere()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var cursor = new DateTimeOffset(2024, 5, 1, 12, 30, 15, TimeSpan.Zero);
        var filter = new EntityFilter
        {
            Name = "Message",
            Operator = FilterOperators.Contains,
            Value = "error"
        };

        var query = builder.Build(filter, pageSize: 5, cursor);

        query.Should().Be("""
            union AppTraces
            | where TimeGenerated <= datetime(2024-05-01T12:30:15.0000000+00:00)
            | where tostring(Message) has 'error'
            | top 6 by TimeGenerated desc
            """);
    }

    [Test]
    public async Task BuildWithMultipleKeyedFiltersCreatesDeterministicAliases()
    {
        var builder = new KqlQueryBuilder(["AppTraces"]);
        var filter = new EntityFilter
        {
            Filters =
            [
                new EntityFilter
                {
                    Name = "Properties",
                    Key = "TenantId",
                    Operator = FilterOperators.Equal,
                    Value = "tenant-1"
                },
                new EntityFilter
                {
                    Name = "Attributes",
                    Key = "http.status_code",
                    Operator = FilterOperators.GreaterThanOrEqual,
                    Value = 500
                }
            ]
        };

        var query = builder.Build(filter, pageSize: 10);

        query.Should().Contain("| where (tostring(Properties[\"TenantId\"]) == 'tenant-1' and Attributes[\"http.status_code\"] >= 500)");
    }
}
