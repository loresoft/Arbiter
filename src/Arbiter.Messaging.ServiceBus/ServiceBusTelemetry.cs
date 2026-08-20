using System.Diagnostics;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides OpenTelemetry instrumentation constants and helpers for Arbiter.Messaging.ServiceBus.
/// </summary>
public static class ServiceBusTelemetry
{
    /// <summary>The name of the <see cref="ActivitySource" />.</summary>
    public const string SourceName = "Arbiter.Messaging.ServiceBus";

    /// <summary>Operation name for typed Service Bus sends.</summary>
    public const string SendOperation = "servicebus send";

    /// <summary>Operation name for Service Bus message processing.</summary>
    public const string ProcessOperation = "servicebus process";

    /// <summary>Attribute: messaging system identifier.</summary>
    public const string MessagingSystemTag = "messaging.system";

    /// <summary>Attribute: destination queue or topic name.</summary>
    public const string DestinationNameTag = "messaging.destination.name";

    /// <summary>Attribute: messaging operation type.</summary>
    public const string OperationTypeTag = "messaging.operation.type";

    /// <summary>Attribute: message identifier.</summary>
    public const string MessageIdTag = "messaging.message.id";

    /// <summary>Attribute: fully-qualified application message type.</summary>
    public const string MessageTypeTag = "arbiter.message.type";

    /// <summary>The single <see cref="ActivitySource" /> for Service Bus instrumentation.</summary>
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
