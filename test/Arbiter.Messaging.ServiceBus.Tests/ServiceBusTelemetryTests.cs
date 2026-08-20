using System.Collections.Concurrent;
using System.Diagnostics;

namespace Arbiter.Messaging.ServiceBus.Tests;

[NotInParallel(nameof(ServiceBusTelemetryTests))]
public class ServiceBusTelemetryTests
{
    [Test]
    public async Task Source_WhenListenerIsRegistered_CreatesProducerActivityWithAmbientParent()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var parent = StartTestActivity(nameof(Source_WhenListenerIsRegistered_CreatesProducerActivityWithAmbientParent));

        using (var activity = ServiceBusTelemetry.Source.StartActivity(
            ServiceBusTelemetry.SendOperation,
            ActivityKind.Producer))
        {
            activity?.SetTag(ServiceBusTelemetry.MessagingSystemTag, "servicebus");
        }

        var span = activities.Single();
        await Assert.That(span.Source.Name).IsEqualTo(ServiceBusTelemetry.SourceName);
        await Assert.That(span.OperationName).IsEqualTo(ServiceBusTelemetry.SendOperation);
        await Assert.That(span.Kind).IsEqualTo(ActivityKind.Producer);
        await Assert.That(span.ParentSpanId).IsEqualTo(parent.SpanId);
        await Assert.That(span.GetTagItem(ServiceBusTelemetry.MessagingSystemTag)?.ToString()).IsEqualTo("servicebus");
    }

    [Test]
    public async Task RecordException_SetsErrorStatusAndAddsExceptionEvent()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var activity = ServiceBusTelemetry.Source.StartActivity(
            ServiceBusTelemetry.ProcessOperation,
            ActivityKind.Consumer);

        var exception = new InvalidOperationException("Processing failed.");
        ServiceBusTelemetry.RecordException(activity, exception);
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
            ShouldListenTo = source => source.Name == ServiceBusTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue,
        };

        ActivitySource.AddActivityListener(listener);
        return (activities, listener);
    }

    private static Activity StartTestActivity(string name)
    {
        var activity = new Activity($"{nameof(ServiceBusTelemetryTests)}.{name}");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        return activity;
    }
}
