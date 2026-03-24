namespace Arbiter.Mapping.Generators;

/// <summary>
/// Magic string constants for class and method names used by the mapper source generator.
/// </summary>
internal static class MapperConstants
{
    /// <summary>
    /// Fully qualified metadata name of the <c>[GenerateMapper]</c> attribute.
    /// </summary>
    public const string GenerateMapperAttributeName = "Arbiter.Mapping.GenerateMapperAttribute";

    /// <summary>
    /// Tracking name for the incremental generator pipeline.
    /// </summary>
    public const string GeneratorTrackingName = "MapperGenerator";

    /// <summary>
    /// Name of the <c>MapperProfile&lt;TSource, TDestination&gt;</c> base class.
    /// </summary>
    public const string MapperBaseClassName = "MapperProfile";

    /// <summary>
    /// Namespace containing the <c>MapperProfile</c> base class.
    /// </summary>
    public const string MapperBaseNamespace = "Arbiter.Mapping";

    /// <summary>
    /// Name of the <c>ConfigureMapping</c> method parsed for custom mappings.
    /// </summary>
    public const string ConfigureMappingMethodName = "ConfigureMapping";

    /// <summary>
    /// Name of the <c>Ignore</c> method in a mapping call chain.
    /// </summary>
    public const string IgnoreMethodName = "Ignore";

    /// <summary>
    /// Name of the <c>From</c> method in a mapping call chain.
    /// </summary>
    public const string FromMethodName = "From";

    /// <summary>
    /// Name of the <c>Value</c> method in a mapping call chain.
    /// </summary>
    public const string ValueMethodName = "Value";
}
