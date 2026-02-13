using System.ComponentModel;

using Arbiter.Mediation;
using Arbiter.Mediation.Infrastructure;

using BenchmarkDotNet.Attributes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Benchmarks.Domain;

[Description("Arbiter")]
public class ArbiterBenchmark
{
    private ReflectionMediator _reflectionMediator = null!;
    private FrozenMediator _frozenMediator = null!;

    private readonly Ping _request = new() { Message = "Ping" };
    private readonly Pinged _notification = new();

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.TryAddSingleton(TextWriter.Null);
        services.TryAddSingleton<ReflectionMediator>();
        services.TryAddSingleton<FrozenMediator>();

        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();

        services.AddTransient<INotificationHandler<Pinged>, PongNotificationHandler>();
        services.AddTransient<INotificationHandler<Pinged>, PungNotificationHandler>();

        MediatorRegistry.Register([new KeyValuePair<Type, IMediatorHandler>(typeof(Ping), new MediatorHandler<Ping, Pong>())]);

        var provider = services.BuildServiceProvider();

        _reflectionMediator = provider.GetRequiredService<ReflectionMediator>();
        _frozenMediator = provider.GetRequiredService<FrozenMediator>();
    }

    [Benchmark]
    public ValueTask<Pong?> SendingReflectionTyped()
    {
        return _reflectionMediator.Send<Ping, Pong>(_request);
    }

    [Benchmark]
    public ValueTask<Pong?> SendingReflectionRequest()
    {
        return _reflectionMediator.Send(_request);
    }

    [Benchmark]
    public ValueTask<object?> SendingReflectionObject()
    {
        var request = (object)_request;
        return _reflectionMediator.Send(request);
    }

    [Benchmark]
    public ValueTask<Pong?> SendingFrozenTyped()
    {
        return _frozenMediator.Send<Ping, Pong>(_request);
    }

    [Benchmark]
    public ValueTask<Pong?> SendingFrozenRequest()
    {
        return _frozenMediator.Send(_request);
    }

    [Benchmark]
    public ValueTask<object?> SendingFrozenObject()
    {
        var request = (object)_request;
        return _frozenMediator.Send(request);
    }


    [Benchmark]
    public ValueTask PublishingNotifications()
    {
        return _reflectionMediator.Publish(_notification);
    }
}
