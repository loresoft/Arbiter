using System.Diagnostics;

namespace Arbiter.Mediation;

/// <summary>
/// Provides OpenTelemetry instrumentation constants and helpers for Arbiter.Mediation.
/// </summary>
/// <remarks>
/// The <see cref="Source"/> is always present in the core library. Register it with the
/// OpenTelemetry tracing provider via <c>AddMediatorInstrumentation</c> in the
/// <c>Arbiter.Mediation.OpenTelemetry</c> package.
/// </remarks>
public static class MediatorTelemetry
{
    /// <summary>The name of the <see cref="ActivitySource"/> and the meter.</summary>
    public const string SourceName = "Arbiter.Mediation";

    /// <summary>The name of the meter used for mediator metrics.</summary>
    public const string MeterName = "Arbiter.Mediation";


    /// <summary>Counter: number of mediator send operations.</summary>
    public const string SendCount = "mediator.send.count";

    /// <summary>Counter: number of mediator publish operations.</summary>
    public const string PublishCount = "mediator.publish.count";

    /// <summary>Histogram: duration of mediator send operations in milliseconds.</summary>
    public const string SendDuration = "mediator.send.duration";

    /// <summary>Histogram: duration of mediator publish operations in milliseconds.</summary>
    public const string PublishDuration = "mediator.publish.duration";


    /// <summary>Operation prefix for top-level send spans: <c>"mediator send"</c>.</summary>
    public const string SendOperation = "mediator send";

    /// <summary>Operation prefix for top-level publish spans: <c>"mediator publish"</c>.</summary>
    public const string PublishOperation = "mediator publish";

    /// <summary>Operation prefix for pipeline behavior spans: <c>"mediator behavior"</c>.</summary>
    public const string BehaviorOperation = "mediator behavior";

    /// <summary>Operation prefix for request handler spans: <c>"mediator handler"</c>.</summary>
    public const string HandlerOperation = "mediator handler";


    /// <summary>Attribute: fully-qualified request type name.</summary>
    public const string RequestTypeTag = "mediator.request_type";

    /// <summary>Attribute: fully-qualified response type name.</summary>
    public const string ResponseTypeTag = "mediator.response_type";

    /// <summary>Attribute: fully-qualified notification type name.</summary>
    public const string NotificationTypeTag = "mediator.notification_type";

    /// <summary>Attribute: short name of the pipeline behavior.</summary>
    public const string BehaviorTag = "mediator.behavior";

    /// <summary>Attribute: short name of the request handler.</summary>
    public const string HandlerTag = "mediator.handler";

    /// <summary>Attribute: mediator operation kind (send / publish / behavior / handler).</summary>
    public const string OperationTag = "mediator.operation";


    /// <summary>
    /// The single <see cref="ActivitySource"/> for all Arbiter.Mediation instrumentation.
    /// </summary>
    public static readonly ActivitySource Source = new(SourceName, ThisAssembly.Version);


    /// <summary>
    /// Records an exception on the activity and sets the span status to <see cref="ActivityStatusCode.Error"/>.
    /// Uses only BCL types — no dependency on <c>OpenTelemetry.Api</c>.
    /// </summary>
    /// <param name="activity">The activity to record the exception on. Safe to call with <see langword="null"/>.</param>
    /// <param name="exception">The exception to record.</param>
    public static void RecordException(Activity? activity, Exception exception)
    {
        if (activity is null)
            return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error.type", exception.GetType().FullName);

        activity.AddEvent(
            new ActivityEvent(
                name: "exception",
                tags: new ActivityTagsCollection
                {
                    { "exception.type", exception.GetType().FullName },
                    { "exception.message", exception.Message },
                    { "exception.stacktrace", exception.ToString() },
                }
            )
        );
    }
}
