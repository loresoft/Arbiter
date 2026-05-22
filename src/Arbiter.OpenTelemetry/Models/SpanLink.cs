using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents a link from an OpenTelemetry span to another span context.
/// </summary>
[Equatable, MessagePackObject(true)]
public partial class SpanLink
{
    /// <summary>
    /// Gets or sets the linked trace identifier.
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the linked span identifier.
    /// </summary>
    public string SpanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the linked trace state.
    /// </summary>
    public string? TraceState { get; set; }

    /// <summary>
    /// Gets or sets the link attributes.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<TelemetryAttribute> Attributes { get; set; } = [];
}
