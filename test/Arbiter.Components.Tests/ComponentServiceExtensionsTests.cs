using Arbiter.CommandQuery.Definitions;
using Arbiter.Components.Services;
using Arbiter.Dispatcher;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arbiter.Components.Tests;

public class ComponentServiceExtensionsTests
{
    [Test]
    public async Task AddArbiterComponentsThrowsWhenTheServiceCollectionIsNull()
    {
        IServiceCollection services = null!;

        var action = () => services.AddArbiterComponents();

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddArbiterComponentsRegistersTheNotificationServiceAsScoped()
    {
        var services = new ServiceCollection();

        services.AddArbiterComponents();

        var descriptor = services.Single(d => d.ServiceType == typeof(INotificationService));
        await Assert.That(descriptor.ImplementationType).IsEqualTo(typeof(NotificationService));
        await Assert.That(descriptor.Lifetime).IsEqualTo(ServiceLifetime.Scoped);
    }

    [Test]
    public async Task AddArbiterComponentsRegistersTheBaseAddressResolverAsScoped()
    {
        var services = new ServiceCollection();

        services.AddArbiterComponents();

        var descriptor = services.Single(d => d.ServiceType == typeof(IBaseAddressResolver));
        await Assert.That(descriptor.ImplementationType).IsEqualTo(typeof(BaseAddressResolver));
        await Assert.That(descriptor.Lifetime).IsEqualTo(ServiceLifetime.Scoped);
    }

    [Test]
    public async Task AddArbiterComponentsRegistersThePrincipalDataServiceAsTransient()
    {
        var services = new ServiceCollection();

        services.AddArbiterComponents();

        var descriptor = services.Single(d => d.ServiceType == typeof(IDispatcherDataService));
        await Assert.That(descriptor.ImplementationType).IsEqualTo(typeof(PrincipalDataService));
        await Assert.That(descriptor.Lifetime).IsEqualTo(ServiceLifetime.Transient);
    }

    [Test]
    public async Task AddArbiterComponentsReplacesAnExistingDataServiceRegistration()
    {
        var services = new ServiceCollection();
        services.AddTransient<IDispatcherDataService>(_ => throw new NotSupportedException());

        services.AddArbiterComponents();

        var descriptor = services.Single(d => d.ServiceType == typeof(IDispatcherDataService));
        await Assert.That(descriptor.ImplementationType).IsEqualTo(typeof(PrincipalDataService));
    }

    [Test]
    public async Task AddArbiterComponentsConfiguresTheNotificationOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddArbiterComponents(options => options.ErrorTimeout = 42);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<NotificationServiceOptions>>();

        await Assert.That(options.Value.ErrorTimeout).IsEqualTo(42);
    }

    [Test]
    public async Task AddArbiterComponentsReturnsTheServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddArbiterComponents();

        await Assert.That(result).IsSameReferenceAs(services);
    }
}
