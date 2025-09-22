using System.Text.Json;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Converters;

/// <summary>
/// <see cref="JsonConverter{T}"/> for <see cref="EntityFilter"/>
/// </summary>
public sealed class EntityFilterConverter : JsonConverter<EntityFilter>
{
    private static readonly JsonEncodedText Name = JsonEncodedText.Encode("name");
    private static readonly JsonEncodedText Value = JsonEncodedText.Encode("value");
    private static readonly JsonEncodedText Operator = JsonEncodedText.Encode("operator");
    private static readonly JsonEncodedText Logic = JsonEncodedText.Encode("logic");
    private static readonly JsonEncodedText Filters = JsonEncodedText.Encode("filters");

    /// <inheritdoc />
    public override EntityFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected StartObject token, but found {reader.TokenType}.");

        var filter = new EntityFilter();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Expected PropertyName token, but found {reader.TokenType}.");

            ReadValue(ref reader, filter, options);
        }

        if (reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException("Unexpected end when reading JSON.");

        return filter;
    }

    private static void ReadValue(ref Utf8JsonReader reader, EntityFilter value, JsonSerializerOptions options)
    {
        if (TryReadStringProperty(ref reader, Name, out var propertyValue))
        {
            value.Name = propertyValue;
        }
        else if (TryReadObjectProperty(ref reader, Value, out var objectValue))
        {
            value.Value = objectValue;
        }
        else if (TryReadStringProperty(ref reader, Operator, out propertyValue))
        {
            value.Operator = TryParseEnum<FilterOperators>(propertyValue);
        }
        else if (TryReadStringProperty(ref reader, Logic, out propertyValue))
        {
            value.Logic = TryParseEnum<FilterLogic>(propertyValue);
        }
        else if (TryReadFiltersProperty(ref reader, options, out var filtersValue))
        {
            value.Filters = filtersValue;
        }
        else
        {
            // Skip unknown properties for forward compatibility
            reader.Read();
            reader.Skip();
        }
    }

    private static bool TryReadFiltersProperty(ref Utf8JsonReader reader, JsonSerializerOptions options, out List<EntityFilter>? value)
    {
        if (!reader.ValueTextEquals(Filters.EncodedUtf8Bytes))
        {
            value = default;
            return false;
        }

        reader.Read();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            value = JsonSerializer.Deserialize<List<EntityFilter>>(ref reader, options);
            return true;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            value = null;
            return true;
        }

        throw new JsonException($"Expected StartArray or Null token for filters, but found {reader.TokenType}.");
    }

    private static bool TryReadStringProperty(ref Utf8JsonReader reader, JsonEncodedText propertyName, out string? value)
    {
        if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
        {
            value = default;
            return false;
        }

        reader.Read();

        if (reader.TokenType == JsonTokenType.String)
        {
            value = reader.GetString();
            return true;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            value = null;
            return true;
        }

        throw new JsonException($"Expected String or Null token for {propertyName}, but found {reader.TokenType}.");
    }

    private static bool TryReadObjectProperty(ref Utf8JsonReader reader, JsonEncodedText propertyName, out object? value)
    {
        if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
        {
            value = default;
            return false;
        }

        reader.Read();
        value = reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number when reader.TryGetInt32(out var intValue) => intValue,
            JsonTokenType.Number when reader.TryGetInt64(out var longValue) => longValue,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null,
            JsonTokenType.StartArray => TryReadArray(ref reader),
            _ => throw new JsonException($"Unsupported token type {reader.TokenType} for value property.")
        };

        return true;
    }

    private static T? TryParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : null;
    }

    private static object? TryReadArray(ref Utf8JsonReader reader)
    {
        var items = new List<object?>();
        var hasNumbers = false;
        var hasStrings = false;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            object? item = reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number when reader.TryGetInt32(out var intValue) => intValue,
                JsonTokenType.Number when reader.TryGetInt64(out var longValue) => longValue,
                JsonTokenType.Number => reader.GetDouble(),
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Null => null,
                _ => throw new JsonException($"Unsupported token type {reader.TokenType} in array.")
            };

            items.Add(item);

            if (item is string)
                hasStrings = true;
            else if (item is int or long or double)
                hasNumbers = true;
        }

        // For empty arrays, return object array since we can't determine the type
        if (items.Count == 0)
            return items.ToArray();

        // Return strongly-typed arrays when possible
        if (hasStrings && !hasNumbers && items.All(x => x is string or null))
            return items.Cast<string?>().ToArray();

        if (hasNumbers && !hasStrings && items.All(x => x is int or null))
            return items.Cast<int?>().Where(x => x.HasValue).Select(x => x!.Value).ToArray();

        // Fall back to object array for mixed types
        return items.ToArray();
    }


    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EntityFilter value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();
        WriteEntityFilter(writer, value, options);
        writer.WriteEndObject();
    }

    private static void WriteEntityFilter(Utf8JsonWriter writer, EntityFilter value, JsonSerializerOptions options)
    {
        if (value.Logic.HasValue)
            writer.WriteString(Logic, value.Logic.Value.ToString());

        if (!string.IsNullOrEmpty(value.Name))
            writer.WriteString(Name, value.Name);

        if (value.Operator.HasValue)
            writer.WriteString(Operator, value.Operator.Value.ToString());

        if (value.Value is not null)
        {
            writer.WritePropertyName(Value);
            JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);
        }

        if (value.Filters is not null)
        {
            writer.WritePropertyName(Filters);
            JsonSerializer.Serialize(writer, value.Filters, options);
        }
    }
}
