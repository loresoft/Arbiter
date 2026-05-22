using System.Diagnostics;

using Arbiter.CommandQuery.Extensions;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents an OpenTelemetry log record.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Equatable, MessagePackObject(true)]
public partial class LogRecord
{
    private string DebuggerDisplay => $"{Timestamp:O} {Body}";

    /// <summary>
    /// Gets or sets the UI-only identifier for the log record.
    /// </summary>
    [IgnoreEquality]
    public string RecordId { get; set; } = Guid.CreateVersion7().ToString("N");

    /// <summary>
    /// Gets or sets the timestamp of the log record.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the OpenTelemetry severity number of the log record.
    /// </summary>
    public int SeverityNumber { get; set; }

    /// <summary>
    /// Gets or sets the severity text derived from <see cref="SeverityNumber"/>.
    /// </summary>
    public string? SeverityText { get; set; }

    /// <summary>
    /// Gets or sets the log record body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the trace identifier associated with the log record.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the span identifier associated with the log record.
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// Gets or sets the service name associated with the log record.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the collection of log record attributes.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<TelemetryAttribute> Attributes { get; set; } = [];

}
