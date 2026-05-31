using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus.Tests;

public class ServiceBusProcessorExtensionsTests
{
    private const string TestConnectionString =
        "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc";

    [Test]
    public void AddServiceBusProcessor_Queue_RegistersKeyedProcessor()
    {
        var serviceProvider = BuildProvider(services =>
            services.AddServiceBusProcessor<SampleProcessor>("TestService", "test-queue"));

        var processor = serviceProvider.GetKeyedService<ServiceBusProcessor>("test-queue");

        processor.Should().NotBeNull();
    }

    [Test]
    public void AddServiceBusProcessor_Queue_RegistersProcessorAsHostedService()
    {
        var serviceProvider = BuildProvider(services =>
            services.AddServiceBusProcessor<SampleProcessor>("TestService", "test-queue"));

        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();

        hostedServices.Should().Contain(hs => hs is SampleProcessor);
    }

    [Test]
    public void AddServiceBusProcessor_Queue_ResolvesSameProcessorInstanceForHostedService()
    {
        var serviceProvider = BuildProvider(services =>
            services.AddServiceBusProcessor<SampleProcessor>("TestService", "test-queue"));

        var processor = serviceProvider.GetRequiredService<SampleProcessor>();
        var hostedService = serviceProvider.GetServices<IHostedService>().OfType<SampleProcessor>().Single();

        hostedService.Should().BeSameAs(processor);
    }

    [Test]
    public void AddServiceBusProcessor_Subscription_RegistersKeyedProcessor()
    {
        var serviceProvider = BuildProvider(services =>
            services.AddServiceBusProcessor<SampleProcessor>("TestService", "test-topic", "unit-test"));

        var processor = serviceProvider.GetKeyedService<ServiceBusProcessor>("test-topic/unit-test");

        processor.Should().NotBeNull();
    }

    [Test]
    public void AddServiceBusProcessor_Subscription_RegistersProcessorAsHostedService()
    {
        var serviceProvider = BuildProvider(services =>
            services.AddServiceBusProcessor<SampleProcessor>("TestService", "test-topic", "unit-test"));

        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();

        hostedServices.Should().Contain(hs => hs is SampleProcessor);
    }

    [Test]
    public void AddServiceBusProcessor_Queue_AppliesConfigureProcessorOptions()
    {
        var serviceProvider = BuildProvider(services =>
            services.AddServiceBusProcessor<SampleProcessor>(
                "TestService",
                "test-queue",
                processorOptions => processorOptions.MaxConcurrentCalls = 4));

        var processor = serviceProvider.GetRequiredKeyedService<ServiceBusProcessor>("test-queue");

        processor.MaxConcurrentCalls.Should().Be(4);
    }

    [Test]
    public void AddServiceBusProcessor_Queue_ThrowsForNullServices()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddServiceBusProcessor<SampleProcessor>("TestService", "test-queue");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddServiceBusProcessor_Queue_ThrowsForNullOrWhiteSpaceQueueName()
    {
        var services = new ServiceCollection();

        var act = () => services.AddServiceBusProcessor<SampleProcessor>("TestService", null!);
        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBusProcessor<SampleProcessor>("TestService", "   ");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddServiceBusProcessor_Subscription_ThrowsForNullOrWhiteSpaceSubscriptionName()
    {
        var services = new ServiceCollection();

        // pass the configure delegate explicitly so the subscription overload is selected for the null case
        var act = () => services.AddServiceBusProcessor<SampleProcessor>("TestService", "test-topic", null!, configureProcessor: null);
        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBusProcessor<SampleProcessor>("TestService", "test-topic", "   ");
        act.Should().Throw<ArgumentException>();
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: TestConnectionString,
            configureBus: entities => entities
                .AddQueue("test-queue")
                .AddTopic("test-topic", "unit-test"));

        configure(services);

        return services.BuildServiceProvider();
    }

    private sealed class SampleProcessor : ServiceBusProcessorBase
    {
        public SampleProcessor(ServiceBusProcessor processor, ILogger<SampleProcessor> logger)
            : base(processor, logger)
        {
        }

        protected override Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            _ = args.ReadFromJson<IntegrationTestMessage>();
            return Task.CompletedTask;
        }
    }
}
