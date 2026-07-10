using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbiter.Queue.Tests;

public class BackgroundQueueTests
{
    [Test]
    public async Task Enqueue_WithNullRequest_ThrowsArgumentNullException()
    {
        var queue = new BackgroundQueue();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await queue.Enqueue<TestRequest>(null!));
    }

    [Test]
    public async Task AddBackgroundQueue_RegistersQueueAndHostedService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBackgroundQueue();

        using var provider = services.BuildServiceProvider();
        var queue = provider.GetRequiredService<IBackgroundQueue>();
        var hostedServices = provider.GetServices<IHostedService>().ToArray();

        await Assert.That(queue).IsNotNull();
        await Assert.That(hostedServices.Length).IsEqualTo(1);
    }

    [Test]
    public async Task BackgroundQueueWorker_DispatchesRequestThroughMediator()
    {
        var state = new QueueTestState();
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddBackgroundQueue();
        services.AddSingleton(state);
        services.AddTransient<IRequestHandler<TestRequest, Unit>, TestRequestHandler>();

        await using var provider = services.BuildServiceProvider();
        await StartHostedServices(provider);

        try
        {
            var queue = provider.GetRequiredService<IBackgroundQueue>();
            await queue.Enqueue(new TestRequest("Processed"));

            var completed = await state.WaitAsync();

            await Assert.That(completed).IsTrue();
            await Assert.That(state.Message).IsEqualTo("Processed");
        }
        finally
        {
            await StopHostedServices(provider);
        }
    }

    [Test]
    public async Task BackgroundQueueWorker_ResolvesHandlersFromScopedProvider()
    {
        var state = new QueueTestState();
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddBackgroundQueue();
        services.AddSingleton(state);
        services.AddScoped<ScopedDependency>();
        services.AddTransient<IRequestHandler<ScopedRequest, Unit>, ScopedRequestHandler>();

        await using var provider = services.BuildServiceProvider();
        await StartHostedServices(provider);

        try
        {
            var queue = provider.GetRequiredService<IBackgroundQueue>();
            await queue.Enqueue(new ScopedRequest());

            var completed = await state.WaitAsync();

            await Assert.That(completed).IsTrue();
            await Assert.That(state.ScopedDependencyId).IsNotEqualTo(Guid.Empty);
        }
        finally
        {
            await StopHostedServices(provider);
        }
    }

    [Test]
    public async Task NotificationHandler_CanEnqueueBackgroundRequest()
    {
        var state = new QueueTestState();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBackgroundQueue();
        services.AddSingleton(state);
        services.AddTransient<INotificationHandler<TestNotification>, TestNotificationHandler>();
        services.AddTransient<IRequestHandler<TestRequest, Unit>, TestRequestHandler>();

        await using var provider = services.BuildServiceProvider();
        await StartHostedServices(provider);

        try
        {
            var mediator = provider.GetRequiredService<IMediator>();
            await mediator.Publish(new TestNotification("Notification Processed"));

            var completed = await state.WaitAsync();

            await Assert.That(completed).IsTrue();
            await Assert.That(state.Message).IsEqualTo("Notification Processed");
        }
        finally
        {
            await StopHostedServices(provider);
        }
    }

    private static async Task StartHostedServices(IServiceProvider serviceProvider)
    {
        foreach (var hostedService in serviceProvider.GetServices<IHostedService>())
            await hostedService.StartAsync(CancellationToken.None);
    }

    private static async Task StopHostedServices(IServiceProvider serviceProvider)
    {
        foreach (var hostedService in serviceProvider.GetServices<IHostedService>().Reverse())
            await hostedService.StopAsync(CancellationToken.None);
    }

    private sealed record TestRequest(string Message) : IRequest<Unit>;

    private sealed record ScopedRequest : IRequest<Unit>;

    private sealed record TestNotification(string Message) : INotification;

    private sealed class TestRequestHandler(QueueTestState state) : IRequestHandler<TestRequest, Unit>
    {
        public ValueTask<Unit> Handle(TestRequest request, CancellationToken cancellationToken = default)
        {
            state.Message = request.Message;
            state.Complete();
            return ValueTask.FromResult(default(Unit));
        }
    }

    private sealed class ScopedRequestHandler(ScopedDependency dependency, QueueTestState state) : IRequestHandler<ScopedRequest, Unit>
    {
        public ValueTask<Unit> Handle(ScopedRequest request, CancellationToken cancellationToken = default)
        {
            state.ScopedDependencyId = dependency.Id;
            state.Complete();
            return ValueTask.FromResult(default(Unit));
        }
    }

    private sealed class TestNotificationHandler(IBackgroundQueue queue) : INotificationHandler<TestNotification>
    {
        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken = default)
            => queue.Enqueue(new TestRequest(notification.Message), cancellationToken);
    }

    private sealed class ScopedDependency
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    private sealed class QueueTestState
    {
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string? Message { get; set; }

        public Guid ScopedDependencyId { get; set; }

        public void Complete() => _completion.TrySetResult();

        public async Task<bool> WaitAsync()
        {
            var completed = await Task.WhenAny(_completion.Task, Task.Delay(TimeSpan.FromSeconds(5)));
            return completed == _completion.Task;
        }
    }
}
