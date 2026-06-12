using System.Collections.Concurrent;
using System.Diagnostics;

using Arbiter.Tests.Domain;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Tests;

// ActivityListener is a global subscription, so telemetry tests must not run in parallel
// with each other to avoid cross-test span pollution.
[NotInParallel(nameof(MediatorTelemetryTests))]
public class MediatorTelemetryTests
{
    // Builds a service provider with mediator and the supplied registrations.
    private static ServiceProvider BuildProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddMediator();
        configure(services);
        return services.BuildServiceProvider();
    }

    // Creates an ActivityListener subscribed to MediatorTelemetry.SourceName and collects stopped activities.
    private static (ConcurrentQueue<Activity> activities, ActivityListener listener) CreateListener()
    {
        var activities = new ConcurrentQueue<Activity>();
        var listener = new ActivityListener
        {
            ShouldListenTo = src => src.Name == MediatorTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue,
        };
        ActivitySource.AddActivityListener(listener);
        return (activities, listener);
    }

    // Creates an ambient parent activity so assertions only inspect spans from the current test.
    private static Activity StartTestActivity(string name)
    {
        var activity = new Activity($"{nameof(MediatorTelemetryTests)}.{name}");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();

        return activity;
    }

    [Test]
    public async Task Send_CreatesTopLevelSpanWithCorrectAttributes()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var testActivity = StartTestActivity(nameof(Send_CreatesTopLevelSpanWithCorrectAttributes));

        await using var provider = BuildProvider(services =>
        {
            services.TryAddSingleton(new Logger());
            services.TryAddTransient<IRequestHandler<TelemetryPing, Pong>, TelemetryPingHandler>();
        });

        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.Send<TelemetryPing, Pong>(new TelemetryPing { Message = "Hello" });

        var span = activities.FirstOrDefault(a =>
            a.ParentSpanId == testActivity.SpanId &&
            a.OperationName == $"{MediatorTelemetry.SendOperation} TelemetryPing");
        await Assert.That(span).IsNotNull();
        await Assert.That(span!.Status).IsEqualTo(ActivityStatusCode.Ok);
        await Assert.That(span.Kind).IsEqualTo(ActivityKind.Internal);
        await Assert.That(span.OperationName).IsEqualTo($"{MediatorTelemetry.SendOperation} TelemetryPing");
        await Assert.That(span.GetTagItem(MediatorTelemetry.RequestTypeTag)?.ToString()).IsEqualTo(typeof(TelemetryPing).FullName);
        await Assert.That(span.GetTagItem(MediatorTelemetry.ResponseTypeTag)?.ToString()).IsEqualTo(typeof(Pong).FullName);
    }

    [Test]
    public async Task SendObject_CreatesSpanWithRequestTypeTag()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var testActivity = StartTestActivity(nameof(SendObject_CreatesSpanWithRequestTypeTag));

        await using var provider = BuildProvider(services =>
        {
            services.TryAddSingleton(new Logger());
            services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();
        });

        var mediator = provider.GetRequiredService<IMediator>();

        // Test the untyped object overload (uses runtime type reflection)
        object request = new Ping { Message = "Hello" };
        await mediator.Send(request);

        var span = activities.FirstOrDefault(a =>
            a.ParentSpanId == testActivity.SpanId &&
            a.OperationName == $"{MediatorTelemetry.SendOperation} Ping");
        await Assert.That(span).IsNotNull();
        await Assert.That(span!.Status).IsEqualTo(ActivityStatusCode.Ok);
        await Assert.That(span.OperationName).IsEqualTo($"{MediatorTelemetry.SendOperation} Ping");
        await Assert.That(span.GetTagItem(MediatorTelemetry.RequestTypeTag)?.ToString()).IsEqualTo(typeof(Ping).FullName);
    }

    [Test]
    public async Task Send_OnException_SetsErrorStatusAndRecordsEvent()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var testActivity = StartTestActivity(nameof(Send_OnException_SetsErrorStatusAndRecordsEvent));

        await using var provider = BuildProvider(services =>
            services.TryAddTransient<IRequestHandler<Ping, Pong>, PingExceptionHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.Send<Ping, Pong>(new Ping { Message = "Throw" }));

        var span = activities.FirstOrDefault(a =>
            a.ParentSpanId == testActivity.SpanId &&
            a.OperationName == $"{MediatorTelemetry.SendOperation} Ping");
        await Assert.That(span).IsNotNull();
        await Assert.That(span!.Status).IsEqualTo(ActivityStatusCode.Error);
        await Assert.That(span.GetTagItem("error.type")?.ToString()).IsEqualTo(typeof(InvalidOperationException).FullName);

        var exceptionEvent = span.Events.FirstOrDefault(e => e.Name == "exception");
        await Assert.That(exceptionEvent.Name).IsEqualTo("exception");

        var exceptionType = exceptionEvent.Tags
            .FirstOrDefault(t => t.Key == "exception.type").Value?.ToString();
        await Assert.That(exceptionType).IsEqualTo(typeof(InvalidOperationException).FullName);
    }

    [Test]
    public async Task Publish_CreatesTopLevelSpanWithNotificationTag()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;
        using var testActivity = StartTestActivity(nameof(Publish_CreatesTopLevelSpanWithNotificationTag));

        await using var provider = BuildProvider(services =>
        {
            services.TryAddSingleton(new Logger());
            services.AddTransient<INotificationHandler<Pinged>, PongNotificationHandler>();
        });

        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.Publish(new Pinged { Message = "Test" });

        var span = activities.FirstOrDefault(a =>
            a.ParentSpanId == testActivity.SpanId &&
            a.OperationName == $"{MediatorTelemetry.PublishOperation} Pinged");
        await Assert.That(span).IsNotNull();
        await Assert.That(span!.Status).IsEqualTo(ActivityStatusCode.Ok);
        await Assert.That(span.GetTagItem(MediatorTelemetry.NotificationTypeTag)?.ToString()).IsEqualTo(typeof(Pinged).FullName);
    }

    private sealed class TelemetryPing : IRequest<Pong>
    {
        public string? Message { get; init; }
    }

    private sealed class TelemetryPingHandler : IRequestHandler<TelemetryPing, Pong>
    {
        public ValueTask<Pong?> Handle(TelemetryPing request, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<Pong?>(new Pong { Message = $"{request.Message} Pong" });
        }
    }
}
