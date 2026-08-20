using System.Diagnostics;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides OpenTelemetry instrumentation constants and helpers for Arbiter.Messaging.WebPubSub.
/// </summary>
public static class WebPubSubTelemetry
{
    /// <summary>The name of the <see cref="ActivitySource" />.</summary>
    public const string SourceName = "Arbiter.Messaging.WebPubSub";

    /// <summary>Operation name for publishing to all hub connections.</summary>
    public const string SendToAllOperation = "webpubsub publish broadcast";

    /// <summary>Operation name for publishing to a hub group.</summary>
    public const string SendToGroupOperation = "webpubsub publish group";

    /// <summary>Operation name for publishing to a user.</summary>
    public const string SendToUserOperation = "webpubsub publish user";

    /// <summary>Operation name for publishing to a connection.</summary>
    public const string SendToConnectionOperation = "webpubsub publish connection";

    /// <summary>Operation name for processing a group message.</summary>
    public const string ProcessGroupOperation = "webpubsub consume group";

    /// <summary>Operation name for processing a server message.</summary>
    public const string ProcessServerOperation = "webpubsub consume server";

    /// <summary>Attribute: messaging system identifier.</summary>
    public const string MessagingSystemTag = "messaging.system";

    /// <summary>Attribute: destination hub name.</summary>
    public const string DestinationNameTag = "messaging.destination.name";

    /// <summary>Attribute: destination group name.</summary>
    public const string DestinationGroupTag = "messaging.destination.group";

    /// <summary>Attribute: destination connection ID.</summary>
    public const string DestinationConnectionIdTag = "messaging.destination.connection_id";

    /// <summary>Attribute: messaging operation type.</summary>
    public const string OperationTypeTag = "messaging.operation.type";

    /// <summary>Attribute: fully-qualified application message type.</summary>
    public const string MessageTypeTag = "arbiter.message.type";

    /// <summary>The single <see cref="ActivitySource" /> for Web PubSub instrumentation.</summary>
    public static readonly ActivitySource Source = new(SourceName, ThisAssembly.Version);

    /// <summary>
    /// Records an exception and marks an activity as failed.
    /// </summary>
    /// <param name="activity">The activity to update, or <see langword="null" /> when tracing is disabled.</param>
    /// <param name="exception">The exception to record.</param>
    public static void RecordException(Activity? activity, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (activity is null)
            return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error.type", exception.GetType().FullName);

        ActivityEvent errorEvent = new(
            name: "exception",
            tags: new ActivityTagsCollection
            {
                { "exception.type", exception.GetType().FullName },
                { "exception.message", exception.Message },
                { "exception.stacktrace", exception.ToString() },
            }
        );

        activity.AddEvent(errorEvent);
    }
}
