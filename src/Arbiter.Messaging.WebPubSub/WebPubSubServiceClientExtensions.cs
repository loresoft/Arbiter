using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Azure;
using Azure.Core;
using Azure.Messaging.WebPubSub;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides extension methods for publishing JSON messages with an Azure Web PubSub service client.
/// </summary>
public static class WebPubSubServiceClientExtensions
{
    /// <summary>
    /// Serializes a message as JSON and sends it to all connections in the hub.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToAllAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        TMessage message,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentNullException.ThrowIfNull(message);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToAllOperation);

        try
        {
            var content = SerializeMessage(message, jsonOptions);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToAllAsync(
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Serializes a message as JSON using source-generated metadata and sends it to all connections in the hub.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToAllAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        TMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToAllOperation);

        try
        {
            var content = SerializeMessage(message, jsonTypeInfo);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToAllAsync(
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Serializes a message as JSON and sends it to all connections in a group.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="groupName">The group to send the message to.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToGroupAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        string groupName,
        TMessage message,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(message);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToGroupOperation,
            groupName: groupName);

        try
        {
            var content = SerializeMessage(message, jsonOptions);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToGroupAsync(
                    group: groupName,
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Serializes a message as JSON using source-generated metadata and sends it to all connections in a group.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="groupName">The group to send the message to.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToGroupAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        string groupName,
        TMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToGroupOperation,
            groupName: groupName);

        try
        {
            var content = SerializeMessage(message, jsonTypeInfo);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToGroupAsync(
                    group: groupName,
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Serializes a message as JSON and sends it to all connections for a user.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="userId">The user to send the message to.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToUserAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        string userId,
        TMessage message,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(message);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToUserOperation);

        try
        {
            var content = SerializeMessage(message, jsonOptions);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToUserAsync(
                    userId: userId,
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Serializes a message as JSON using source-generated metadata and sends it to all connections for a user.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="userId">The user to send the message to.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToUserAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        string userId,
        TMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToUserOperation);

        try
        {
            var content = SerializeMessage(message, jsonTypeInfo);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToUserAsync(
                    userId: userId,
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Serializes a message as JSON and sends it to a single connection.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="connectionId">The connection to send the message to.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonOptions">The serializer options to use, or <see langword="null" /> to use the default options.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToConnectionAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        string connectionId,
        TMessage message,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentNullException.ThrowIfNull(message);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToConnectionOperation,
            connectionId: connectionId);

        try
        {
            var content = SerializeMessage(message, jsonOptions);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToConnectionAsync(
                    connectionId: connectionId,
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Serializes a message as JSON using source-generated metadata and sends it to a single connection.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="serviceClient">The service client used to publish the message.</param>
    /// <param name="connectionId">The connection to send the message to.</param>
    /// <param name="message">The message to serialize and send.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata for <typeparamref name="TMessage" />.</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public static async Task SendToConnectionAsJsonAsync<TMessage>(
        this WebPubSubServiceClient serviceClient,
        string connectionId,
        TMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        using var activity = StartSendActivity(
            serviceClient: serviceClient,
            messageType: typeof(TMessage),
            operationName: WebPubSubTelemetry.SendToConnectionOperation,
            connectionId: connectionId);

        try
        {
            var content = SerializeMessage(message, jsonTypeInfo);
            var context = new RequestContext { CancellationToken = cancellationToken };

            var response = await serviceClient
                .SendToConnectionAsync(
                    connectionId: connectionId,
                    content: content,
                    contentType: ContentType.ApplicationJson,
                    context: context)
                .ConfigureAwait(false);

            RecordResponse(activity, response);
        }
        catch (Exception ex)
        {
            WebPubSubTelemetry.RecordException(activity, ex);
            throw;
        }
    }

    private static RequestContent SerializeMessage<TMessage>(
        TMessage message,
        JsonSerializerOptions? jsonOptions)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, jsonOptions);
        return RequestContent.Create(bytes);
    }

    private static RequestContent SerializeMessage<TMessage>(
        TMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, jsonTypeInfo);
        return RequestContent.Create(bytes);
    }

    private static void RecordResponse(
        Activity? activity,
        Response response)
    {
        if (activity is null)
            return;

        var code = response.IsError ? ActivityStatusCode.Error : ActivityStatusCode.Ok;
        var description = response.IsError ? response.ReasonPhrase : null;

        activity.SetTag("http.response.status_code", response.Status);
        activity.SetStatus(code, description);
    }

    private static Activity? StartSendActivity(
        WebPubSubServiceClient serviceClient,
        Type messageType,
        string operationName,
        string? groupName = null,
        string? connectionId = null)
    {
        var hubName = serviceClient.Hub;

        var activity = WebPubSubTelemetry.Source.StartActivity(
            operationName,
            ActivityKind.Producer);

        if (activity is null)
            return null;

        var contextName = groupName ?? connectionId ?? hubName;
        activity.DisplayName = $"{operationName} {contextName}";

        activity.SetTag(WebPubSubTelemetry.MessagingSystemTag, "azure.webpubsub");
        activity.SetTag(WebPubSubTelemetry.DestinationNameTag, hubName);
        activity.SetTag(WebPubSubTelemetry.DestinationGroupTag, groupName);
        activity.SetTag(WebPubSubTelemetry.DestinationConnectionIdTag, connectionId);
        activity.SetTag(WebPubSubTelemetry.OperationTypeTag, "send");
        activity.SetTag(WebPubSubTelemetry.MessageTypeTag, messageType.FullName);

        return activity;
    }
}
