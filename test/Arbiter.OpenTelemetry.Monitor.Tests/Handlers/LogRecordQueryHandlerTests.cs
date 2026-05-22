using Arbiter.CommandQuery.Queries;
using Arbiter.Mediation;
using Arbiter.OpenTelemetry.Models;
using Arbiter.OpenTelemetry.Queries;
using Arbiter.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.OpenTelemetry.Monitor.Tests.Handlers;

[Skip("Local Only")]
[Category("LocalOnly")]
public class LogRecordQueryHandlerTests
{
    private static readonly string[] _baseColumns = ["TimeGenerated", "SeverityLevel", "OperationId"];

    [ClassDataSource<TestApplication>(Shared = SharedType.PerAssembly)]
    public required TestApplication Application { get; init; }

    public IServiceProvider Services => Application.Services;

    [Test]
    public async Task HandleWithRecentAppTracesQueryReturnsTraceSchemaAndFlexibleRows()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Name = "Message",
                Operator = FilterOperators.IsNotNull
            }
        };

        var result = await handler.Handle(request);

        result.Should().NotBeNull();

        result.Descriptors.Should().NotBeEmpty();
        result.Descriptors.Select(d => d.Name).Should().Contain(["TimeGenerated", "Message", "SeverityLevel"]);

        result.Data.Should().HaveCountLessThanOrEqualTo(request.PageSize);
        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
        result.Data.Should().OnlyContain(log => log.SeverityNumber >= 0 && log.SeverityNumber <= 4);
    }

    [Test]
    public async Task HandleWithRecentAppExceptionsQueryReturnsExceptionSchemaAndFlexibleRows()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Name = "OuterMessage",
                Operator = FilterOperators.IsNotNull
            }
        };

        var result = await handler.Handle(request);

        result.Should().NotBeNull();

        result.Descriptors.Should().NotBeEmpty();
        result.Descriptors.Select(d => d.Name).Should().Contain(["TimeGenerated", "OuterMessage", "ProblemId"]);

        result.Data.Should().HaveCountLessThanOrEqualTo(request.PageSize);
        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
        result.Data.Should().OnlyContain(log => !string.IsNullOrWhiteSpace(log.Body));
    }

    [Test]
    public async Task HandleWithWarningOrHigherQueryReturnsPagedTelemetryShape()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 5,
            Filter = new EntityFilter
            {
                Name = "SeverityLevel",
                Operator = FilterOperators.GreaterThanOrEqual,
                Value = 2
            }
        };

        var result = await handler.Handle(request);

        result.Should().NotBeNull();

        result.Descriptors.Should().NotBeEmpty();
        result.Descriptors.Select(d => d.Name).Should().Contain(["TimeGenerated", "SeverityLevel", "OperationId"]);

        result.Data.Should().HaveCountLessThanOrEqualTo(request.PageSize);
        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.SeverityNumber >= 2);

        if (result.ContinuationToken is not null)
            result.ContinuationToken.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task HandleWithMessageContainsQueryExecutesStringFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Name = "Message",
                Operator = FilterOperators.Contains,
                Value = "request"
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "Message"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithAppRoleNameStartsWithQueryExecutesStartsWithFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Name = "AppRoleName",
                Operator = FilterOperators.StartsWith,
                Value = "Tracker"
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "AppRoleName"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithGroupedTraceOrExceptionQueryExecutesNestedLogicalFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Logic = FilterLogic.Or,
                Filters =
                [
                    new EntityFilter
                    {
                        Name = "Message",
                        Operator = FilterOperators.IsNotNull
                    },
                    new EntityFilter
                    {
                        Name = "OuterMessage",
                        Operator = FilterOperators.IsNotNull
                    }
                ]
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "Message", "OuterMessage"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithPropertiesKeyQueryExecutesDynamicColumnFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Name = "Properties",
                Key = "CategoryName",
                Operator = FilterOperators.IsNotNull
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "Properties"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithNestedPropertiesKeyQueryExecutesDynamicColumnFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Name = "Properties",
                Key = "OriginalFormat",
                Operator = FilterOperators.Contains,
                Value = "{"
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "Properties"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithNestedPropertiesAndMessageGroupQueryExecutesDynamicColumnFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Logic = FilterLogic.And,
                Filters =
                [
                    new EntityFilter
                    {
                        Name = "Properties",
                        Key = "OriginalFormat",
                        Operator = FilterOperators.IsNotNull
                    },
                    new EntityFilter
                    {
                        Name = "Message",
                        Operator = FilterOperators.IsNotNull
                    }
                ]
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "Message", "Properties"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithNestedPropertiesKeyContainingDotExecutesEscapedDynamicColumnFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Name = "Properties",
                Key = "otel.scope.name",
                Operator = FilterOperators.IsNotNull
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "Properties"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithContinuationTokenQueryExecutesCursorFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var cursorTime = DateTimeOffset.UtcNow.AddMinutes(-30);
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            ContinuationToken = ContinuationToken.Create<DateTimeOffset?>(cursorTime),
            Filter = new EntityFilter
            {
                Name = "SeverityLevel",
                Operator = FilterOperators.GreaterThanOrEqual,
                Value = 0
            }
        };

        var result = await handler.Handle(request);
        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, _baseColumns);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.Timestamp <= cursorTime);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }

    [Test]
    public async Task HandleWithServiceAndSeverityGroupQueryExecutesAndFilter()
    {
        var handler = Services.GetRequiredService<IRequestHandler<LogRecordQuery, LogRecordResult>>();
        var request = new LogRecordQuery
        {
            AgeMinutes = 43200,
            PageSize = 10,
            Filter = new EntityFilter
            {
                Logic = FilterLogic.And,
                Filters =
                [
                    new EntityFilter
                    {
                        Name = "AppRoleName",
                        Operator = FilterOperators.IsNotNull
                    },
                    new EntityFilter
                    {
                        Name = "SeverityLevel",
                        Operator = FilterOperators.LessThanOrEqual,
                        Value = 4
                    }
                ]
            }
        };

        var result = await handler.Handle(request);

        result.Should().NotBeNull();

        AssertExecutableResult(result, request.PageSize, [.. _baseColumns, "AppRoleName"]);

        result.Data.Should().OnlyContain(log => log.Timestamp != default);
        result.Data.Should().OnlyContain(log => log.SeverityNumber <= 4);
        result.Data.Should().OnlyContain(log => log.Attributes.Count > 0);
    }


    private static void AssertExecutableResult(LogRecordResult? result, int pageSize, IEnumerable<string> expectedColumns)
    {
        result.Should().NotBeNull();

        result.Descriptors.Should().NotBeEmpty();
        result.Descriptors.Select(d => d.Name).Should().Contain(expectedColumns);

        result.Data.Should().HaveCountLessThanOrEqualTo(pageSize);
    }
}
