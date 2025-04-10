using System.ComponentModel;

using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Benchmarks.Domain;

[Description("Arbiter")]
public class ArbiterBenchmark
{
    private Arbiter.IMediator _mediator = null!;
    private readonly Ping _request = new() { Message = "Ping" };
    private readonly Pinged _notification = new();

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.TryAddSingleton(TextWriter.Null);
        services.TryAddSingleton<IMediator, Mediator>();

        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();

        services.AddTransient<INotificationHandler<Pinged>, PongNotificationHandler>();
        services.AddTransient<INotificationHandler<Pinged>, PungNotificationHandler>();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public ValueTask<Pong> SendingRequests()
    {
        return _mediator.Send<Ping, Pong>(_request);
    }

    [Benchmark]
    public ValueTask PublishingNotifications()
    {
        return _mediator.Publish(_notification);
    }
}
