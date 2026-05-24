using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Arbiter.CommandQuery;
using Arbiter.CommandQuery.Extensions;

using Azure.Core;
using Azure.Messaging.ServiceBus;

using MessagePack;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides extension methods for sending serialized messages with Azure Service Bus senders.
/// </summary>
public static class ServiceBusSenderExtensions
{
    /// <summary>
    /// Serializes a message as JSON and sends it through the Service Bus sender.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="sender">The Service Bus sender.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendAsJsonAsync<TMessage>(
        this ServiceBusSender sender,
        TMessage message,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(message);

        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);

        JsonSerializer.Serialize(writer, message, jsonOptions);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        var body = BinaryData.FromBytes(bufferWriter.WrittenMemory);
        var serviceMessage = new ServiceBusMessage(body)
        {
            MessageId = Guid.NewGuid().ToString("N"),
            ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
            Subject = typeof(TMessage).GetPortableName(),
        };

        await sender
            .SendMessageAsync(serviceMessage, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

    }

    /// <summary>
    /// Serializes a message as JSON using source-generated metadata and sends it through the Service Bus sender.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="sender">The Service Bus sender.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendAsJsonAsync<TMessage>(
        this ServiceBusSender sender,
        TMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);

        JsonSerializer.Serialize(writer, message, jsonTypeInfo);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        var body = BinaryData.FromBytes(bufferWriter.WrittenMemory);
        var serviceMessage = new ServiceBusMessage(body)
        {
            MessageId = Guid.NewGuid().ToString("N"),
            ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
            Subject = typeof(TMessage).GetPortableName(),
        };

        await sender
            .SendMessageAsync(serviceMessage, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

    }

    /// <summary>
    /// Serializes a message as MessagePack and sends it through the Service Bus sender.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="sender">The Service Bus sender.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="messagePackOptions">The MessagePack serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendAsMessagePackAsync<TMessage>(
        this ServiceBusSender sender,
        TMessage message,
        MessagePackSerializerOptions? messagePackOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(message);

        messagePackOptions ??= MessagePackDefaults.DefaultSerializerOptions;

        // use actual type of the message for serialization and naming
        var messageType = message.GetType();
        var messageName = messageType.GetPortableName();

        var bufferWriter = new ArrayBufferWriter<byte>();

        MessagePackSerializer.Serialize(messageType, bufferWriter, message, messagePackOptions, cancellationToken);

        var body = BinaryData.FromBytes(bufferWriter.WrittenMemory);
        var serviceMessage = new ServiceBusMessage(body)
        {
            MessageId = Guid.NewGuid().ToString("N"),
            ContentType = MessagePackDefaults.MessagePackContentType,
            Subject = messageName,
        };

        await sender
            .SendMessageAsync(serviceMessage, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
