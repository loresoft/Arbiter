using System.ComponentModel;

using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Benchmarks.MediatR;

[Description("MediatR")]
public class MediatRBenchmark
{
    private global::MediatR.IMediator _mediator = null!;
    private readonly Ping _request = new() { Message = "Ping" };
    private readonly Pinged _notification = new();

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.TryAddSingleton(TextWriter.Null);
        services.TryAddSingleton<global::MediatR.IMediator, global::MediatR.Mediator>();

        services.TryAddTransient<global::MediatR.IRequestHandler<Ping, Pong>, PingHandler>();

        services.AddTransient<global::MediatR.IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<global::MediatR.IPipelineBehavior<Ping, Pong>, InnerBehavior>();

        services.AddTransient<global::MediatR.INotificationHandler<Pinged>, PongNotificationHandler>();
        services.AddTransient<global::MediatR.INotificationHandler<Pinged>, PungNotificationHandler>();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<global::MediatR.IMediator>();
    }

    [Benchmark]
    public Task<Pong> SendingRequest()
    {
        return _mediator.Send(_request);
    }

    [Benchmark]
    public Task<object?> SendingObject()
    {
        var request = (object)_request;
        return _mediator.Send(request);
    }

    [Benchmark]
    public Task PublishingNotifications()
    {
        return _mediator.Publish(_notification);
    }

}
