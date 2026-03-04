using System.Diagnostics;

using Arbiter.Tests.Domain;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using TUnit.Core;

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
    private static (List<Activity> activities, ActivityListener listener) CreateListener()
    {
        var activities = new List<Activity>();
        var listener = new ActivityListener
        {
            ShouldListenTo = src => src.Name == MediatorTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Add,
        };
        ActivitySource.AddActivityListener(listener);
        return (activities, listener);
    }

    [Test]
    public async Task Send_CreatesTopLevelSpanWithCorrectAttributes()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;

        await using var provider = BuildProvider(services =>
        {
            services.TryAddSingleton(new Logger());
            services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();
        });

        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.Send<Ping, Pong>(new Ping { Message = "Hello" });

        var span = activities.FirstOrDefault(a => a.OperationName.StartsWith(MediatorTelemetry.SendOperation));
        await Assert.That(span).IsNotNull();
        await Assert.That(span!.Status).IsEqualTo(ActivityStatusCode.Ok);
        await Assert.That(span.Kind).IsEqualTo(ActivityKind.Internal);
        await Assert.That(span.OperationName).IsEqualTo($"{MediatorTelemetry.SendOperation} Ping");
        await Assert.That(span.GetTagItem(MediatorTelemetry.RequestTypeTag)?.ToString()).IsEqualTo(typeof(Ping).FullName);
        await Assert.That(span.GetTagItem(MediatorTelemetry.ResponseTypeTag)?.ToString()).IsEqualTo(typeof(Pong).FullName);
    }

    [Test]
    public async Task SendObject_CreatesSpanWithRequestTypeTag()
    {
        var (activities, listener) = CreateListener();
        using var _ = listener;

        await using var provider = BuildProvider(services =>
        {
            services.TryAddSingleton(new Logger());
            services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();
        });

        var mediator = provider.GetRequiredService<IMediator>();

        // Test the untyped object overload (uses runtime type reflection)
        object request = new Ping { Message = "Hello" };
        await mediator.Send(request);

        var span = activities.FirstOrDefault(a => a.OperationName.StartsWith(MediatorTelemetry.SendOperation));
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

        await using var provider = BuildProvider(services =>
            services.TryAddTransient<IRequestHandler<Ping, Pong>, PingExceptionHandler>());

        var mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.Send<Ping, Pong>(new Ping { Message = "Throw" }));

        var span = activities.FirstOrDefault(a => a.OperationName.StartsWith(MediatorTelemetry.SendOperation));
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

        await using var provider = BuildProvider(services =>
        {
            services.TryAddSingleton(new Logger());
            services.AddTransient<INotificationHandler<Pinged>, PongNotificationHandler>();
        });

        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.Publish(new Pinged { Message = "Test" });

        var span = activities.FirstOrDefault(a => a.OperationName.StartsWith(MediatorTelemetry.PublishOperation));
        await Assert.That(span).IsNotNull();
        await Assert.That(span!.Status).IsEqualTo(ActivityStatusCode.Ok);
        await Assert.That(span.GetTagItem(MediatorTelemetry.NotificationTypeTag)?.ToString()).IsEqualTo(typeof(Pinged).FullName);
    }
}
