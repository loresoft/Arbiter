using Arbiter.CommandQuery.Extensions;

using MessagePack;
using MessagePack.Formatters;

namespace Arbiter.CommandQuery.Formatters;

/// <summary>
/// A MessagePack formatter that preserves the original type of collections and primitives
/// when serializing object? properties, instead of converting everything to object[].
/// Uses portable type names for flexibility and compatibility across assembly versions.
/// </summary>
public class TypePreservingFormatter : IMessagePackFormatter<object?>
{
    /// <inheritdoc />
    public void Serialize(ref MessagePackWriter writer, object? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        // Get the actual type of the value
        var type = value.GetType();

        // Write type information as an array: [type_name, value]
        writer.WriteArrayHeader(2);

        // Write portable type name
        var typeName = type.GetPortableName();
        writer.Write(typeName);

        // Serialize the value using the appropriate formatter
        MessagePackSerializer.Serialize(type, ref writer, value, options);
    }

    /// <inheritdoc />
    public object? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        // Read the array header
        var count = reader.ReadArrayHeader();
        if (count != 2)
        {
            throw new MessagePackSerializationException("Invalid format for TypePreservingFormatter");
        }

        // Read portable type name
        var typeName = reader.ReadString();
        if (string.IsNullOrEmpty(typeName))
        {
            throw new MessagePackSerializationException("Type name cannot be null or empty");
        }

        // Resolve the type
        var type = Type.GetType(typeName);
        if (type == null)
        {
            throw new MessagePackSerializationException($"Unable to resolve type: {typeName}");
        }

        // Deserialize the value using the resolved type
        return MessagePackSerializer.Deserialize(type, ref reader, options);
    }
}
