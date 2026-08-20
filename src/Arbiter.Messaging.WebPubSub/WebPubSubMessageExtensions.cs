using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Azure.Messaging.WebPubSub.Clients;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides extension methods for deserializing the data of an Azure Web PubSub received message.
/// </summary>
public static class WebPubSubMessageExtensions
{
    /// <summary>
    /// Deserializes the JSON data of a received group message into a value of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the data represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this WebPubSubGroupMessageEventArgs args,
        JsonSerializerOptions? jsonOptions = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.Message.Data.ToObjectFromJson<TMessage>(jsonOptions);
    }

    /// <summary>
    /// Deserializes the JSON data of a received group message into a value of <typeparamref name="TMessage" />
    /// using source-generated metadata.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the data represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this WebPubSubGroupMessageEventArgs args,
        JsonTypeInfo<TMessage> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        return args.Message.Data.ToObjectFromJson(jsonTypeInfo);
    }

    /// <summary>
    /// Deserializes the JSON data of a received server message into a value of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the data represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this WebPubSubServerMessageEventArgs args,
        JsonSerializerOptions? jsonOptions = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.Message.Data.ToObjectFromJson<TMessage>(jsonOptions);
    }

    /// <summary>
    /// Deserializes the JSON data of a received server message into a value of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the data represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this WebPubSubServerMessageEventArgs args,
        JsonTypeInfo<TMessage> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        return args.Message.Data.ToObjectFromJson(jsonTypeInfo);
    }
}
