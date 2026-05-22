using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents OpenTelemetry resource metadata.
/// </summary>
[Equatable, MessagePackObject(true)]
public partial class TelemetryResource
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the service instance identifier.
    /// </summary>
    public string? ServiceInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the resource attributes.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<TelemetryAttribute> Attributes { get; set; } = [];
}
