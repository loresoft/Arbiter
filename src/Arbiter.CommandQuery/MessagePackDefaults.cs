using MessagePack;

namespace Arbiter.CommandQuery;

/// <summary>
/// Provides default values and settings for MessagePack serialization.
/// </summary>
public static class MessagePackDefaults
{
    /// <summary>
    /// The content type for MessagePack serialized data.
    /// </summary>
    public const string MessagePackContentType = "application/x-msgpack";

    // resolves type names embedded in object slots, assembly versions are omitted so client and server versions can differ
    private static readonly TypelessTypeLoader TypeLoader = TypelessTypeLoader.Create(Type.GetType);

    /// <summary>
    /// The default formatter factory chain for MessagePack.
    /// </summary>
    /// <remarks>
    /// Values in <see cref="object"/> slots, such as <see cref="Queries.EntityFilter.Value"/>, are serialized with their runtime
    /// type name so collections, primitives and complex objects round trip as their original type.
    /// </remarks>
    public static readonly MessagePackFormatterFactory DefaultFormatterFactory = MessagePackFormatterFactory.Combine(
        // 1) Source generated formatters take priority
        SourceGeneratedFormatterFactory.Instance,

        // 2) Typeless object slots, preserves the runtime type of the value. Must precede BuiltIn, which also claims object
        new TypelessFormatterFactory(TypeLoader, omitAssemblyVersion: true),

        // 3) Built-in types (primitives, enums, collections, etc.)
        BuiltInFormatterFactory.Instance,

        // 4) Generic collections, Nullable and enums closed over runtime types
        GenericFormatterFactory.Instance,

        // 5) Reflection for types the source generator did not cover, annotated and contractless (POCOs without attributes)
        new ReflectionFormatterFactory(annotatedOnly: false),

        // 6) Interface and abstract types not otherwise served, embeds the runtime type
        new ForceTypelessFormatterFactory()
    );

    /// <summary>
    /// The default formatter resolver for MessagePack.
    /// </summary>
    /// <remarks>
    /// Nullable annotation and required member validation are disabled to match the behavior of MessagePack v3.
    /// </remarks>
    public static readonly MessagePackFormatterResolver DefaultFormatterResolver = new(DefaultFormatterFactory)
    {
        ValidateNullableAnnotations = false,
        ValidateRequiredMembers = false,
    };

    /// <summary>
    /// The default serializer options for MessagePack.
    /// </summary>
    public static readonly MessagePackSerializerOptions DefaultSerializerOptions = new(DefaultFormatterResolver);
}
