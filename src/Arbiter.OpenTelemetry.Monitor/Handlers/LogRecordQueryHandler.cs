using System.Globalization;

using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Handlers;
using Arbiter.OpenTelemetry.Models;
using Arbiter.OpenTelemetry.Monitor.Queries;
using Arbiter.OpenTelemetry.Queries;
using Arbiter.Services;

using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arbiter.OpenTelemetry.Monitor.Handlers;

/// <summary>
/// Handles log record queries by executing KQL against an Azure Monitor Log Analytics workspace.
/// </summary>
public partial class LogRecordQueryHandler : RequestHandlerBase<LogRecordQuery, LogRecordResult>
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 500;
    private const int DefaultAgeMinutes = 1440;
    private const int MaxAgeMinutes = 43200;

    private readonly KqlQueryBuilder _queryBuilder = new(["AppTraces", "AppExceptions"]);
    private readonly LogsQueryClient _logsQueryClient;
    private readonly IConfiguration _configuration;
    private readonly Lazy<string?> _workspaceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogRecordQueryHandler"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create the handler logger.</param>
    /// <param name="logsQueryClient">The Azure Monitor logs query client.</param>
    /// <param name="configuration">The application configuration that contains the Log Analytics workspace identifier.</param>
    public LogRecordQueryHandler(
        ILoggerFactory loggerFactory,
        LogsQueryClient logsQueryClient,
        IConfiguration configuration) : base(loggerFactory)
    {
        _logsQueryClient = logsQueryClient;
        _configuration = configuration;

        _workspaceId = new Lazy<string?>(() =>
            _configuration.GetValue<string>("LogAnalytics:WorkspaceId")
            ?? _configuration.GetValue<string>("AzureMonitor:WorkspaceId")
        );

    }

    /// <summary>
    /// Processes a log record query by building a KQL statement, querying Azure Monitor, and mapping the results.
    /// </summary>
    /// <param name="request">The log record query containing filters, paging, and time-range settings.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The log records, telemetry descriptors, and optional continuation token for the next page.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "<Pending>")]
    protected override async ValueTask<LogRecordResult?> Process(
        LogRecordQuery request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var workspaceId = _workspaceId.Value;

        // gracefully handle missing configuration by logging a warning and returning an empty result
        if (workspaceId.IsNullOrWhiteSpace())
        {
            LogWorkspaceNotConfigured(Logger);
            return new LogRecordResult();
        }

        // Ensure page size is within allowed limits
        var pageSize = Math.Clamp(request.PageSize <= 0 ? DefaultPageSize : request.PageSize, 1, MaxPageSize);

        // Parse the continuation token to determine the timestamp cursor for pagination
        var cursorTime = ContinuationToken.Parse<DateTimeOffset?>(request.ContinuationToken);

        // Build the KQL query based on the request parameters
        var query = _queryBuilder.Build(request.Filter, pageSize, cursorTime);

        LogExecutingLogQuery(Logger,
            workspaceId, pageSize, cursorTime, request.AgeMinutes, query);

        // Create the time range for the query based on the request parameters
        var timeRange = CreateTimeRange(request);

        var options = new LogsQueryOptions { ServerTimeout = TimeSpan.FromMinutes(5) };

        var response = await _logsQueryClient
            .QueryWorkspaceAsync(
                workspaceId: workspaceId,
                query: query,
                timeRange: timeRange,
                options: options,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var descriptors = MapDescriptors(response.Value.Table);
        var logs = MapLogs(response.Value.Table);

        var hasMore = logs.Count > pageSize;
        string? continuationToken = null;

        // Retrieved one more than the requested page size to determine if there are more results.
        // Capture the cursor timestamp before removing the extra log to guard against duplicate timestamps.
        if (hasMore && logs.Count > pageSize)
        {
            // Get the timestamp of the last log (the extra one fetched)
            var cursorTimestamp = logs[^1].Timestamp;

            // Remove the extra log to return only the requested page size
            logs.RemoveAt(logs.Count - 1);

            // Use the cursor timestamp for the next page query
            continuationToken = ContinuationToken.Create(cursorTimestamp);
        }

        return new LogRecordResult
        {
            Data = logs,
            Descriptors = descriptors,
            ContinuationToken = continuationToken,
        };

    }

    private static QueryTimeRange CreateTimeRange(LogRecordQuery request)
    {
        var ageMinutes = Math.Clamp(request.AgeMinutes <= 0 ? DefaultAgeMinutes : request.AgeMinutes, 1, MaxAgeMinutes);
        return new QueryTimeRange(TimeSpan.FromMinutes(ageMinutes));
    }


    private static List<LogRecord> MapLogs(LogsTable table)
    {
        var logs = new List<LogRecord>(table.Rows.Count);

        foreach (var row in table.Rows)
        {
            var attributes = MapAttributes(table, row);

            var timestamp = row.GetDateTimeOffset("TimeGenerated") ?? default;
            var severityLevel = row.GetInt32("SeverityLevel") ?? 0;
            var operationId = row.GetString("OperationId");
            var parentId = row.GetString("ParentId");
            var appRoleName = row.GetString("AppRoleName");

            var message = row.GetString("Message");
            if (string.IsNullOrWhiteSpace(message))
                message = row.GetString("OuterMessage");
            if (string.IsNullOrWhiteSpace(message))
                message = row.GetString("InnermostMessage");
            if (string.IsNullOrWhiteSpace(message))
                message = row.GetString("ProblemId");

            LogRecord log = new()
            {
                Timestamp = timestamp,
                SeverityNumber = severityLevel,
                SeverityText = ToSeverityText(severityLevel),
                Body = message,
                TraceId = operationId,
                SpanId = parentId,
                ServiceName = appRoleName,
                Attributes = attributes,
            };

            logs.Add(log);
        }

        return logs;
    }

    private static List<TelemetryDescriptor> MapDescriptors(LogsTable table)
    {
        var columns = new List<TelemetryDescriptor>(table.Columns.Count);

        foreach (var column in table.Columns)
        {
            TelemetryDescriptor item = new()
            {
                Name = column.Name,
                Type = column.Type.ToString(),
            };
            columns.Add(item);
        }

        return columns;
    }

    private static List<TelemetryAttribute> MapAttributes(LogsTable table, LogsTableRow row)
    {
        var values = new List<TelemetryAttribute>(table.Columns.Count);

        for (var index = 0; index < table.Columns.Count; index++)
        {
            var column = table.Columns[index];
            string? value = StringifyValue(row[index]);

            if (!value.HasValue())
                continue;

            TelemetryAttribute item = new()
            {
                Name = column.Name,
                Value = value,
                Type = column.Type.ToString(),
            };
            values.Add(item);
        }

        return values;
    }


    private static string? StringifyValue(object? value)
    {
        return value switch
        {
            null => null,
            string stringValue => stringValue,
            BinaryData binaryData => binaryData.ToString(),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O"),
            DateTime dateTime => dateTime.ToString("O"),
            IFormattable formattable => formattable.ToString(format: null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static string? ToSeverityText(int? severityNumber)
    {
        if (severityNumber is null)
            return null;

        return severityNumber.Value switch
        {
            0 => "Debug",
            1 => "Information",
            2 => "Warning",
            3 => "Error",
            4 => "Critical",
            _ => null,
        };
    }


    [LoggerMessage(LogLevel.Warning, "Azure Monitor log query skipped because LogAnalytics:WorkspaceId is not configured.")]
    static partial void LogWorkspaceNotConfigured(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Executing log query with workspaceId={WorkspaceId}, pageSize={PageSize}, cursorTime={CursorTime}, ageMinutes={AgeMinutes};\r\n{Query}")]
    static partial void LogExecutingLogQuery(ILogger logger, string workspaceId, int pageSize, DateTimeOffset? cursorTime, int ageMinutes, string query);
}
