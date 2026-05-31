using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Arbiter.CommandQuery;

using Azure.Messaging.ServiceBus;

using MessagePack;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides extension methods for deserializing the body of an Azure Service Bus received message.
/// </summary>
public static class ServiceBusMessageExtensions
{
    /// <summary>
    /// Deserializes the JSON body of a received message into a value of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="message">The received message whose body is deserialized.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the body represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this ServiceBusReceivedMessage message,
        JsonSerializerOptions? jsonOptions = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var reader = new Utf8JsonReader(message.Body.ToMemory().Span);
        return JsonSerializer.Deserialize<TMessage>(ref reader, jsonOptions);
    }

    /// <summary>
    /// Deserializes the JSON body of a received message into a value of <typeparamref name="TMessage" />
    /// using source-generated metadata.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="message">The received message whose body is deserialized.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the body represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this ServiceBusReceivedMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        var reader = new Utf8JsonReader(message.Body.ToMemory().Span);
        return JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
    }

    /// <summary>
    /// Deserializes the MessagePack body of a received message into a value of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="message">The received message whose body is deserialized.</param>
    /// <param name="messagePackOptions">The MessagePack serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <param name="cancellationToken">A token used to cancel the deserialization operation.</param>
    /// <returns>The deserialized message.</returns>
    public static TMessage ReadFromMessagePack<TMessage>(
        this ServiceBusReceivedMessage message,
        MessagePackSerializerOptions? messagePackOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        messagePackOptions ??= MessagePackDefaults.DefaultSerializerOptions;

        return MessagePackSerializer.Deserialize<TMessage>(message.Body.ToMemory(), messagePackOptions, cancellationToken);
    }

    /// <summary>
    /// Deserializes the JSON body of the received message contained in the event arguments
    /// into a value of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the body represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this ProcessMessageEventArgs args,
        JsonSerializerOptions? jsonOptions = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.Message.ReadFromJson<TMessage>(jsonOptions);
    }

    /// <summary>
    /// Deserializes the JSON body of the received message contained in the event arguments
    /// into a value of <typeparamref name="TMessage" /> using source-generated metadata.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <returns>The deserialized message, or <see langword="null" /> when the body represents a null value.</returns>
    public static TMessage? ReadFromJson<TMessage>(
        this ProcessMessageEventArgs args,
        JsonTypeInfo<TMessage> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.Message.ReadFromJson(jsonTypeInfo);
    }

    /// <summary>
    /// Deserializes the MessagePack body of the received message contained in the event arguments
    /// into a value of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="args">The event arguments containing the received message.</param>
    /// <param name="messagePackOptions">The MessagePack serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <returns>The deserialized message.</returns>
    public static TMessage ReadFromMessagePack<TMessage>(
        this ProcessMessageEventArgs args,
        MessagePackSerializerOptions? messagePackOptions = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.Message.ReadFromMessagePack<TMessage>(messagePackOptions, args.CancellationToken);
    }
}
