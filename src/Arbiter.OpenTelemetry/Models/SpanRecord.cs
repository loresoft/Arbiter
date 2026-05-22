using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents an OpenTelemetry span record.
/// </summary>
[Equatable, MessagePackObject(true)]
public partial class SpanRecord
{
    /// <summary>
    /// Gets or sets the trace identifier associated with the span.
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the span identifier.
    /// </summary>
    public string SpanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent span identifier.
    /// </summary>
    public string? ParentSpanId { get; set; }

    /// <summary>
    /// Gets or sets the span name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the span kind.
    /// </summary>
    public string? Kind { get; set; }

    /// <summary>
    /// Gets or sets the service name associated with the span.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the span start time.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the span end time.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Gets the span duration.
    /// </summary>
    [JsonIgnore]
    [IgnoreEquality]
    [IgnoreMember]
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Gets or sets the span status code.
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the span status description.
    /// </summary>
    public string? StatusDescription { get; set; }

    /// <summary>
    /// Gets or sets the resource associated with the span.
    /// </summary>
    public TelemetryResource? Resource { get; set; }

    /// <summary>
    /// Gets or sets the span attributes.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<TelemetryAttribute> Attributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the events associated with the span.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<SpanEvent> Events { get; set; } = [];

    /// <summary>
    /// Gets or sets the links associated with the span.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<SpanLink> Links { get; set; } = [];
}
