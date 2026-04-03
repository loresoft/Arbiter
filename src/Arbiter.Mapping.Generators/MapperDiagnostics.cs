using Microsoft.CodeAnalysis;

namespace Arbiter.Mapping.Generators;

/// <summary>
/// Diagnostic descriptors reported by <see cref="MapperDiagnosticAnalyzer"/> to validate
/// mapper classes annotated with <c>[GenerateMapper]</c>.
/// </summary>
internal static class MapperDiagnostics
{
    private const string Category = "Arbiter.Mapping";

    /// <summary>
    /// ARB0001: A class with [GenerateMapper] must be declared partial so the source generator
    /// can emit the mapping implementation.
    /// </summary>
    public static readonly DiagnosticDescriptor ClassMustBePartial = new(
        id: "ARB0001",
        title: "Mapper class must be partial",
        messageFormat: "Class '{0}' has [GenerateMapper] but is not declared partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes annotated with [GenerateMapper] must be declared partial so the source generator can emit the mapping implementation.");

    /// <summary>
    /// ARB0002: A class with [GenerateMapper] must inherit from MapperProfile&lt;TSource, TDestination&gt;.
    /// </summary>
    public static readonly DiagnosticDescriptor ClassMustInheritMapperProfile = new(
        id: "ARB0002",
        title: "Mapper class must inherit MapperProfile<TSource, TDestination>",
        messageFormat: "Class '{0}' has [GenerateMapper] but does not inherit from MapperProfile<TSource, TDestination>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes annotated with [GenerateMapper] must inherit from MapperProfile<TSource, TDestination>.");

    /// <summary>
    /// ARB0003: ConfigureMapping must only contain mapping configuration calls.
    /// Arbitrary code (variable declarations, loops, conditionals, etc.) is silently ignored
    /// by the source generator and will never execute, which breaks the generation pipeline cache.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidStatementInConfigureMapping = new(
        id: "ARB0003",
        title: "ConfigureMapping contains unsupported statement",
        messageFormat: "ConfigureMapping must only contain mapping configuration calls; '{0}' will be ignored by the source generator",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The ConfigureMapping method body is parsed as syntax by the source generator and never executed at runtime. " +
                     "Only mapping.Property(...).From(...), .Value(...), and .Ignore() calls are recognized. " +
                     "All other statements are silently ignored and may break the generation pipeline cache.");

    /// <summary>
    /// ARB0005: The same destination property is mapped more than once in ConfigureMapping.
    /// Only the last mapping will take effect.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateDestinationMapping = new(
        id: "ARB0005",
        title: "Duplicate destination property mapping",
        messageFormat: "Destination property '{0}' is mapped more than once; only the last mapping will take effect",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The same destination property is configured multiple times in ConfigureMapping. Only the last mapping will be used by the source generator.");

    /// <summary>
    /// ARB0006: An invocation in ConfigureMapping does not follow the expected
    /// mapping.Property(...).From/Value/Ignore() pattern.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidMappingCallPattern = new(
        id: "ARB0006",
        title: "Invalid mapping call pattern",
        messageFormat: "Unrecognized mapping call '{0}'; expected mapping.Property(...).From(...), .Value(...), or .Ignore()",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calls inside ConfigureMapping must follow the pattern mapping.Property(d => d.Prop).From(...), .Value(...), or .Ignore(). " +
                     "Other call patterns are not recognized by the source generator.");

}
