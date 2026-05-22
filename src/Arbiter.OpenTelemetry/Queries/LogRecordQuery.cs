using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Queries;
using Arbiter.OpenTelemetry.Models;

using MessagePack;

namespace Arbiter.OpenTelemetry.Queries;

/// <summary>
/// Query for retrieving log records based on filter and pagination criteria.
/// </summary>
[MessagePackObject(true)]
public record LogRecordQuery : PrincipalCommandBase<LogRecordResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogRecordQuery"/> class.
    /// </summary>
    /// <param name="principal">The claims principal for the query request.</param>
    public LogRecordQuery(ClaimsPrincipal? principal)
        : base(principal)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogRecordQuery"/> class for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    [SerializationConstructor]
    public LogRecordQuery()
        : this(principal: null)
    { }

    /// <summary>
    /// Gets the optional filter to apply to the log records query.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public EntityFilter? Filter { get; init; }

    /// <summary>
    /// Gets the age in minutes for filtering log records (default: 1440 minutes / 1 day).
    /// </summary>
    [JsonPropertyName("ageMinutes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int AgeMinutes { get; init; } = 1440;

    /// <summary>
    /// Gets the page size for pagination of log records (default: 50).
    /// </summary>
    [JsonPropertyName("pageSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PageSize { get; init; } = 50;

    /// <summary>
    /// Gets the continuation token for paginating through log records.
    /// </summary>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ContinuationToken { get; init; }
}
