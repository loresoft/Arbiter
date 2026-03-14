using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbiter.Dispatcher;

/// <summary>
/// Provides telemetry instrumentation utilities for the Arbiter.Dispatcher component, including activity source
/// management and exception recording.
/// </summary>
/// <remarks>This static class exposes the shared ActivitySource used for Arbiter.Dispatcher tracing and offers
/// methods to record exceptions on activities. It is intended for use in distributed tracing scenarios and integrates
/// with .NET diagnostics infrastructure. All APIs are dependency-free and rely only on BCL types.</remarks>
public static class DispatcherTelemetry
{
    /// <summary>The name of the <see cref="ActivitySource"/> and the meter.</summary>
    public const string SourceName = "Arbiter.Dispatcher";

    /// <summary>Operation prefix for top-level receive spans: <c>"dispatcher receive"</c>.</summary>
    public const string ReceiveOperation = "dispatch received";

    /// <summary>Tag for the end user's name.</summary>
    public const string UserNameTag = "enduser.name";

    /// <summary>Attribute: fully-qualified request type name.</summary>
    public const string RequestTypeTag = "dispatch.request_type";

    /// <summary>
    /// The single <see cref="ActivitySource"/> for all Arbiter.Dispatcher instrumentation.
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
