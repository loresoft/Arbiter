using System.Collections.Concurrent;
using System.Diagnostics;

namespace Arbiter.Messaging.WebPubSub.Tests;

[NotInParallel(nameof(WebPubSubTelemetryTests))]
public class WebPubSubTelemetryTests
{
    [Test]
    public async Task Source_WhenListenerIsRegistered_CreatesProducerActivityWithAmbientParent()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var parent = StartTestActivity(nameof(Source_WhenListenerIsRegistered_CreatesProducerActivityWithAmbientParent));

        using (var activity = WebPubSubTelemetry.Source.StartActivity(
            WebPubSubTelemetry.SendToGroupOperation,
            ActivityKind.Producer))
        {
            activity?.SetTag(WebPubSubTelemetry.MessagingSystemTag, "azure.webpubsub");
            activity?.SetTag(WebPubSubTelemetry.DestinationNameTag, "notifications");
            activity?.SetTag(WebPubSubTelemetry.DestinationGroupTag, "updates");
        }

        var span = activities.Single();
        await Assert.That(span.Source.Name).IsEqualTo(WebPubSubTelemetry.SourceName);
        await Assert.That(span.OperationName).IsEqualTo(WebPubSubTelemetry.SendToGroupOperation);
        await Assert.That(span.Kind).IsEqualTo(ActivityKind.Producer);
        await Assert.That(span.ParentSpanId).IsEqualTo(parent.SpanId);
        await Assert.That(span.GetTagItem(WebPubSubTelemetry.DestinationNameTag)?.ToString()).IsEqualTo("notifications");
        await Assert.That(span.GetTagItem(WebPubSubTelemetry.DestinationGroupTag)?.ToString()).IsEqualTo("updates");
    }

    [Test]
    public async Task RecordException_SetsErrorStatusAndAddsExceptionEvent()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var activity = WebPubSubTelemetry.Source.StartActivity(
            WebPubSubTelemetry.ProcessServerOperation,
            ActivityKind.Consumer);

        WebPubSubTelemetry.RecordException(activity, new InvalidOperationException("Processing failed."));
        activity?.Stop();

        var span = activities.Single();
        await Assert.That(span.Status).IsEqualTo(ActivityStatusCode.Error);
        await Assert.That(span.GetTagItem("error.type")?.ToString()).IsEqualTo(typeof(InvalidOperationException).FullName);
        await Assert.That(span.Events.Single().Name).IsEqualTo("exception");
    }

    private static (ConcurrentQueue<Activity> Activities, ActivityListener Listener) CreateListener()
    {
        var activities = new ConcurrentQueue<Activity>();
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == WebPubSubTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue,
        };

        ActivitySource.AddActivityListener(listener);
        return (activities, listener);
    }

    private static Activity StartTestActivity(string name)
    {
        var activity = new Activity($"{nameof(WebPubSubTelemetryTests)}.{name}");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        return activity;
    }
}
