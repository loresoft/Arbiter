using System.Diagnostics;
using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents an OpenTelemetry trace record for trace views.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Equatable, MessagePackObject(true)]
public partial class TraceRecord
{
    private string DebuggerDisplay => $"{TraceId} {Duration}";

    /// <summary>
    /// Gets or sets the trace identifier.
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the trace display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the root span identifier.
    /// </summary>
    public string? RootSpanId { get; set; }

    /// <summary>
    /// Gets or sets the service name associated with the trace.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the trace start time.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the trace end time.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Gets the trace duration.
    /// </summary>
    [JsonIgnore]
    [IgnoreEquality]
    [IgnoreMember]
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Gets or sets the trace status code.
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the trace status description.
    /// </summary>
    public string? StatusDescription { get; set; }

    /// <summary>
    /// Gets or sets the spans in the trace.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<SpanRecord> Spans { get; set; } = [];
}
