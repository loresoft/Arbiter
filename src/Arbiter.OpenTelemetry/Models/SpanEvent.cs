using System.Diagnostics;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents an event attached to an OpenTelemetry span.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Equatable, MessagePackObject(true)]
public partial class SpanEvent
{
    private string DebuggerDisplay => $"{Timestamp:O} {Name} Attributes={Attributes?.Count ?? 0}";

    /// <summary>
    /// Gets or sets the event name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the event attributes.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<TelemetryAttribute> Attributes { get; set; } = [];
}
