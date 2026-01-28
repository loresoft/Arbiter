using Arbiter.CommandQuery.Formatters;

using MessagePack;
using MessagePack.Resolvers;

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

    /// <summary>
    /// The default serializer options for MessagePack.
    /// </summary>
    public static readonly MessagePackSerializerOptions DefaultSerializerOptions =
        MessagePackSerializerOptions.Standard
            .WithResolver(
                CompositeResolver.Create(
                [
                    // 0) Custom formatters
                    CustomFormatterResolver.Instance,

                    // 1) Uses the Roslyn source generator output produced automatically.
                    SourceGeneratedFormatterResolver.Instance,

                    // 2) Attribute-based + built-ins (enums, primitives, etc.)
                    StandardResolver.Instance,

                    // 3) Typeless (for object-typed or unknown static types)
                    TypelessObjectResolver.Instance,

                    // 4) Contractless fallback for POCOs without attributes
                    ContractlessStandardResolver.Instance,
                ])
            )
            .WithCompression(MessagePackCompression.Lz4Block);

}
