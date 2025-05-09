using Arbiter.Mediation;
using Arbiter.Tests.Domain;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Tests;

public class MediatorTests
{
    [Test]
    public async Task SendWithBasicHandler()
    {
        var logger = new Logger();

        var services = new ServiceCollection();
        services.AddMediator();

        services.TryAddSingleton(logger);
        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new Ping { Message = "Ping" };
        var response = await mediator.Send<Ping, Pong>(request);

        await Assert.That(response).IsNotNull();
        await Assert.That(response!.Message).IsEqualTo("Ping Pong");
    }

    [Test]
    public async Task SendWithException()
    {
        var services = new ServiceCollection();
        services.AddMediator();

        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingExceptionHandler>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new Ping { Message = "Throw" };

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var result = await mediator.Send<Ping, Pong>(request);
        });
    }

    [Test]
    public async Task SendWithBehaviorPipeline()
    {
        var logger = new Logger();

        var services = new ServiceCollection();
        services.AddMediator();

        services.TryAddSingleton(logger);
        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new Ping { Message = "Ping" };
        var response = await mediator.Send<Ping, Pong>(request);

        await Assert.That(response).IsNotNull();
        await Assert.That(response!.Message).IsEqualTo("Ping Pong");

        await Assert.That(logger.Messages[0]).IsEqualTo("Outer before");
        await Assert.That(logger.Messages[1]).Contains("Inner before");
        await Assert.That(logger.Messages[2]).Contains("Ping Handler");
        await Assert.That(logger.Messages[3]).Contains("Inner after");
        await Assert.That(logger.Messages[4]).Contains("Outer after");
    }

    [Test]
    public async Task SendWithBehaviorPipelineReflection()
    {
        var logger = new Logger();

        var services = new ServiceCollection();
        services.AddMediator();

        services.TryAddSingleton(logger);
        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new Ping { Message = "Ping" };
        var response = await mediator.Send(request);

        await Assert.That(response).IsNotNull();
        await Assert.That(response!.Message).IsEqualTo("Ping Pong");

        await Assert.That(logger.Messages[0]).IsEqualTo("Outer before");
        await Assert.That(logger.Messages[1]).Contains("Inner before");
        await Assert.That(logger.Messages[2]).Contains("Ping Handler");
        await Assert.That(logger.Messages[3]).Contains("Inner after");
        await Assert.That(logger.Messages[4]).Contains("Outer after");
    }

    [Test]
    public async Task SendWithBehaviorPipelineObjectType()
    {
        var logger = new Logger();

        var services = new ServiceCollection();
        services.AddMediator();

        services.TryAddSingleton(logger);
        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        object request = new Ping { Message = "Ping" };
        var response = await mediator.Send(request);

        await Assert.That(response).IsNotNull();

        var pong = response as Pong;
        await Assert.That(pong).IsNotNull();
        await Assert.That(pong!.Message).IsEqualTo("Ping Pong");

        await Assert.That(logger.Messages[0]).IsEqualTo("Outer before");
        await Assert.That(logger.Messages[1]).Contains("Inner before");
        await Assert.That(logger.Messages[2]).Contains("Ping Handler");
        await Assert.That(logger.Messages[3]).Contains("Inner after");
        await Assert.That(logger.Messages[4]).Contains("Outer after");
    }

    [Test]
    public async Task SendWithBehaviorException()
    {
        var logger = new Logger();

        var services = new ServiceCollection();
        services.AddMediator();

        services.TryAddSingleton(logger);
        services.TryAddTransient<IRequestHandler<Ping, Pong>, PingHandler>();

        services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, ExceptionBehavior>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new Ping { Message = "Throw" };
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var result = await mediator.Send<Ping, Pong>(request);
        });

        await Assert.That(logger.Messages).Contains("Outer before");
        await Assert.That(logger.Messages).Contains("Inner before");
        await Assert.That(logger.Messages).Contains("Exception before");
    }

    [Test]
    public async Task PublishWithBasicHandler()
    {
        var logger = new Logger();

        var services = new ServiceCollection();
        services.AddMediator();

        services.TryAddSingleton(logger);

        services.AddTransient<INotificationHandler<Pinged>, PongNotificationHandler>();
        services.AddTransient<INotificationHandler<Pinged>, PungNotificationHandler>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new Pinged { Message = "Pinged" };
        await mediator.Publish(request);

        await Assert.That(logger.Messages).Contains("Pinged Pong");
        await Assert.That(logger.Messages).Contains("Pinged Pung");
    }
}
