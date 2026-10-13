using Arbiter.CommandQuery.Options;
using Arbiter.Components.Services;
using Arbiter.Components.Tests.Abstracts;
using Arbiter.Components.Tests.Services;

using Bunit;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Arbiter.Components.Tests;

public class EntityTypeaheadTests
{
    [Test]
    public async Task NullableIdentifierLookupUsesUnderlyingKeyType()
    {
        var id = 42;
        var model = new TenantModel { Id = id, Name = "Tenant" };
        var dataService = new FakeDataService { GetResult = model };
        using var context = CreateContext(dataService: dataService);
        var cacheTime = TimeSpan.FromMinutes(5);
        var cut = context.Render<EntityTypeahead<TenantModel, int?>>(p => p
            .Add(x => x.Preload, false)
            .Add(x => x.CacheTime, cacheTime));
        var component = cut.Instance;

        var result = await component.LookupMethod!(id);

        await Assert.That(result).IsSameReferenceAs(model);
        await Assert.That(dataService.RequestedKeyTypes.Single()).IsEqualTo(typeof(int));
        await Assert.That(dataService.RequestedIds.Single()).IsEqualTo(id);
        await Assert.That(dataService.RequestedCacheTimes.Single()).IsEqualTo(cacheTime);
        await Assert.That(component.ConvertMethod!(model)).IsEqualTo((int?)id);
    }

    [Test]
    public async Task NullIdentifierDoesNotQuery()
    {
        var dataService = new FakeDataService();
        using var context = CreateContext();
        var component = CreateTypeahead<int?>(context, dataService);

        var result = await component.LookupMethod!(null);

        await Assert.That(result).IsNull();
        await Assert.That(dataService.RequestedIds.Count).IsEqualTo(0);
    }

    [Test]
    public async Task NullableDefaultIdentifierDoesNotQuery()
    {
        var dataService = new FakeDataService { GetResult = new TenantModel() };
        using var context = CreateContext();
        var component = CreateTypeahead<int?>(context, dataService);

        var result = await component.LookupMethod!(0);

        await Assert.That(result).IsNull();
        await Assert.That(dataService.RequestedIds.Count).IsEqualTo(0);
    }

    [Test]
    public async Task NullableNonIdentifierValueDoesNotQuery()
    {
        var dataService = new FakeDataService { GetResult = new TenantModel() };
        using var context = CreateContext();
        var component = CreateTypeahead<DateTime?>(context, dataService);

        var result = await component.LookupMethod!(DateTime.UtcNow);

        await Assert.That(result).IsNull();
        await Assert.That(dataService.RequestedIds.Count).IsEqualTo(0);
    }

    [Test]
    public async Task NonNullableIdentifierLookupPreservesKeyType()
    {
        var model = new TenantModel { Id = 42 };
        var dataService = new FakeDataService { GetResult = model };
        using var context = CreateContext();
        var component = CreateTypeahead<int>(context, dataService);

        var result = await component.LookupMethod!(model.Id);

        await Assert.That(result).IsSameReferenceAs(model);
        await Assert.That(dataService.RequestedKeyTypes.Single()).IsEqualTo(typeof(int));
        await Assert.That(dataService.RequestedCacheTimes.Single())
            .IsEqualTo(component.EnvironmentOptions.Value.DefaultCacheTime);
    }

    [Test]
    public async Task DefaultNonNullableIdentifierDoesNotQuery()
    {
        var dataService = new FakeDataService();
        using var context = CreateContext();
        var component = CreateTypeahead<int>(context, dataService);

        var result = await component.LookupMethod!(0);

        await Assert.That(result).IsNull();
        await Assert.That(dataService.RequestedIds.Count).IsEqualTo(0);
    }

    [Test]
    public async Task EntityValueIsReturnedWithoutQuerying()
    {
        var model = new TenantModel { Id = 42 };
        var dataService = new FakeDataService();
        using var context = CreateContext();
        var component = CreateTypeahead<TenantModel>(context, dataService);

        var result = await component.LookupMethod!(model);

        await Assert.That(result).IsSameReferenceAs(model);
        await Assert.That(dataService.RequestedIds.Count).IsEqualTo(0);
    }

    [Test]
    public async Task NonIdentifierValueDoesNotQuery()
    {
        var dataService = new FakeDataService { GetResult = new TenantModel { Id = 42 } };
        using var context = CreateContext();
        var component = CreateTypeahead<string>(context, dataService);

        var result = await component.LookupMethod!("42");

        await Assert.That(result).IsNull();
        await Assert.That(dataService.RequestedIds.Count).IsEqualTo(0);
    }

    [Test]
    public async Task NullableIdentifierLookupErrorsAreReported()
    {
        var toaster = new FakeToaster();
        var dataService = new FakeDataService { GetException = new InvalidOperationException("boom") };
        using var context = CreateContext(toaster);
        var component = CreateTypeahead<int?>(context, dataService);

        var result = await component.LookupMethod!(42);

        await Assert.That(result).IsNull();
        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(dataService.RequestedKeyTypes.Single()).IsEqualTo(typeof(int));
    }

    [Test]
    public async Task IsEnabledWhenNoPermissionResolverIsSet()
    {
        using var context = CreateContext();

        var cut = RenderTypeahead(context);

        await Assert.That(cut.Instance.Disabled).IsFalse();
    }

    [Test]
    public async Task IsEnabledWhenThePermissionResolverGrantsAccess()
    {
        using var context = CreateContext();

        var cut = RenderTypeahead(context, p => p.Add(x => x.PermissionResolver, () => Task.FromResult(true)));

        await Assert.That(cut.Instance.Disabled).IsFalse();
        await Assert.That(cut.FindAll("[disabled]").Count).IsEqualTo(0);
    }

    [Test]
    public async Task IsDisabledWhenThePermissionResolverDeniesAccess()
    {
        using var context = CreateContext();

        var cut = RenderTypeahead(context, p => p.Add(x => x.PermissionResolver, () => Task.FromResult(false)));

        await Assert.That(cut.Instance.Disabled).IsTrue();
        await Assert.That(cut.FindAll("[disabled]").Count).IsGreaterThan(0);
    }

    [Test]
    public async Task DisabledParameterTakesPriorityAndSkipsThePermissionResolver()
    {
        using var context = CreateContext();
        var calls = 0;

        var cut = RenderTypeahead(context, p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.PermissionResolver, () =>
            {
                calls++;
                return Task.FromResult(true);
            }));

        await Assert.That(cut.Instance.Disabled).IsTrue();
        await Assert.That(calls).IsEqualTo(0);
    }

    [Test]
    public async Task PermissionIsCheckedOnceDisabledParameterIsCleared()
    {
        using var context = CreateContext();
        var calls = 0;

        var cut = RenderTypeahead(context, p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.PermissionResolver, () =>
            {
                calls++;
                return Task.FromResult(false);
            }));

        cut.Render(p => p.Add(x => x.Disabled, false));

        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(cut.Instance.Disabled).IsTrue();
    }

    [Test]
    public async Task ExplicitFalseDisabledDoesNotOverrideDeniedPermission()
    {
        using var context = CreateContext();

        var cut = RenderTypeahead(context, p => p
            .Add(x => x.PermissionResolver, () => Task.FromResult(false)));

        cut.Render(p => p.Add(x => x.Disabled, false));

        await Assert.That(cut.Instance.Disabled).IsTrue();
    }

    [Test]
    public async Task IsDisabledAndNotifiesWhenThePermissionResolverThrows()
    {
        var toaster = new FakeToaster();
        using var context = CreateContext(toaster);

        var cut = RenderTypeahead(context, p => p.Add(x => x.PermissionResolver, () => throw new InvalidOperationException("boom")));

        await Assert.That(cut.Instance.Disabled).IsTrue();
        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
    }

    [Test]
    public async Task IsDisabledWhileThePermissionResolverIsPending()
    {
        using var context = CreateContext();
        var completion = new TaskCompletionSource<bool>();

        var cut = RenderTypeahead(context, p => p.Add(x => x.PermissionResolver, () => completion.Task));

        await Assert.That(cut.Instance.Disabled).IsTrue();

        completion.SetResult(true);

        cut.WaitForAssertion(() => cut.Instance.Disabled.Should().BeFalse());
    }

    [Test]
    public async Task PermissionResolverIsInvokedOnce()
    {
        using var context = CreateContext();
        var calls = 0;

        var cut = RenderTypeahead(context, p => p.Add(x => x.PermissionResolver, () =>
        {
            calls++;
            return Task.FromResult(true);
        }));

        cut.Render(p => p.Add(x => x.Placeholder, "Search"));

        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(cut.Instance.Disabled).IsFalse();
    }

    private static EntityTypeahead<TenantModel, TValue> CreateTypeahead<TValue>(
        BunitContext context,
        FakeDataService dataService)
    {
        return new EntityTypeahead<TenantModel, TValue>
        {
            DataService = dataService,
            Notification = context.Services.GetRequiredService<INotificationService>(),
            EnvironmentOptions = context.Services.GetRequiredService<IOptions<EnvironmentOptions>>()
        };
    }

    private static IRenderedComponent<EntityTypeahead<TenantModel, int>> RenderTypeahead(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<EntityTypeahead<TenantModel, int>>>? configure = null)
    {
        return context.Render<EntityTypeahead<TenantModel, int>>(p =>
        {
            p.Add(x => x.Preload, false);
            configure?.Invoke(p);
        });
    }

    private static BunitContext CreateContext(FakeToaster? toaster = null, FakeDataService? dataService = null)
    {
        var context = new BunitContext();
        var toasterInstance = toaster ?? new FakeToaster();
        var options = Options.Create(new EnvironmentOptions());

        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        context.Services.AddSingleton<IToaster>(toasterInstance);
        context.Services.AddSingleton<INotificationService, NotificationService>();
        var dataServiceInstance = dataService ?? new FakeDataService();
        context.Services.AddSingleton<Arbiter.Dispatcher.IDispatcherDataService>(dataServiceInstance);
        context.Services.AddSingleton(options);

        return context;
    }
}
