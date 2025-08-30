namespace Arbiter.MemoryPack;

/// <summary>
/// Contains constants used by the MemoryPack dispatcher for HTTP communication and serialization.
/// </summary>
public class MemoryPackConstants
{
    /// <summary>
    /// HTTP header name used to specify the .NET type of the serialized data being transmitted.
    /// </summary>
    /// <remarks>
    /// This header contains the assembly-qualified name of the request type, allowing the server
    /// to properly deserialize the MemoryPack binary data back to the correct .NET type.
    /// </remarks>
    public const string DataTypeHeader = "X-Data-Type";

    /// <summary>
    /// MIME media type identifier for MemoryPack serialized binary content.
    /// </summary>
    /// <remarks>
    /// This custom media type is used in HTTP Content-Type headers to indicate that the message
    /// body contains binary data serialized using the MemoryPack serialization format.
    /// </remarks>
    public const string MemoryPackMediaType = "application/x-memorypack";

    /// <summary>
    /// Route suffix appended to dispatcher endpoints to distinguish MemoryPack-enabled routes.
    /// </summary>
    /// <remarks>
    /// This suffix is automatically appended to the base dispatcher route to create a dedicated
    /// endpoint for handling MemoryPack serialized requests, allowing both regular and packed
    /// endpoints to coexist on the same service.
    /// </remarks>
    public const string RouteSuffix = "-packed";
}
