using Arbiter.CommandQuery.Options;
using Arbiter.Components.Services;
using Arbiter.Components.Tests.Abstracts;
using Arbiter.Components.Tests.Services;

using Bunit;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Arbiter.Components.Tests;

public class EntitySelectTests
{
    [Test]
    public async Task IsEnabledWhenNoPermissionResolverIsSet()
    {
        using var context = CreateContext();

        var cut = RenderSelect(context);

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task IsEnabledWhenThePermissionResolverGrantsAccess()
    {
        using var context = CreateContext();

        var cut = RenderSelect(context, p => p.Add(x => x.PermissionResolver, () => Task.FromResult(true)));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task IsDisabledWhenThePermissionResolverDeniesAccess()
    {
        using var context = CreateContext();

        var cut = RenderSelect(context, p => p.Add(x => x.PermissionResolver, () => Task.FromResult(false)));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();
    }

    [Test]
    public async Task DisabledParameterTakesPriorityAndSkipsThePermissionResolver()
    {
        using var context = CreateContext();
        var calls = 0;

        var cut = RenderSelect(context, p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.PermissionResolver, () =>
            {
                calls++;
                return Task.FromResult(true);
            }));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();
        await Assert.That(calls).IsEqualTo(0);
    }

    [Test]
    public async Task DisabledAttributeTakesPriorityAndSkipsThePermissionResolver()
    {
        using var context = CreateContext();
        var calls = 0;

        var cut = RenderSelect(context, p => p
            .AddUnmatched("disabled", true)
            .Add(x => x.PermissionResolver, () =>
            {
                calls++;
                return Task.FromResult(true);
            }));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();
        await Assert.That(calls).IsEqualTo(0);
    }

    [Test]
    public async Task FalseDisabledAttributeDoesNotDisable()
    {
        using var context = CreateContext();

        var cut = RenderSelect(context, p => p.AddUnmatched("disabled", false));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task PreservesOtherAttributes()
    {
        using var context = CreateContext();

        var cut = RenderSelect(context, p => p
            .AddUnmatched("id", "TenantId")
            .AddUnmatched("class", "form-select")
            .Add(x => x.PermissionResolver, () => Task.FromResult(false)));

        var select = cut.Find("select");

        await Assert.That(select.GetAttribute("id")).IsEqualTo("TenantId");
        await Assert.That(select.ClassList.Contains("form-select")).IsTrue();
        await Assert.That(select.HasAttribute("disabled")).IsTrue();
    }

    [Test]
    public async Task IsDisabledAndNotifiesWhenThePermissionResolverThrows()
    {
        var toaster = new FakeToaster();
        using var context = CreateContext(toaster);

        var cut = RenderSelect(context, p => p.Add(x => x.PermissionResolver, () => throw new InvalidOperationException("boom")));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();
        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
    }

    [Test]
    public async Task IsDisabledWhileThePermissionResolverIsPending()
    {
        using var context = CreateContext();
        var completion = new TaskCompletionSource<bool>();

        var cut = RenderSelect(context, p => p.Add(x => x.PermissionResolver, () => completion.Task));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();

        completion.SetResult(true);

        cut.WaitForAssertion(() => cut.Find("select").HasAttribute("disabled").Should().BeFalse());
    }

    [Test]
    public async Task IsEnabledWhileLoadingWhenNoPermissionResolverIsSet()
    {
        var load = new TaskCompletionSource();
        var dataService = new FakeDataService { PageDelay = load.Task };
        using var context = CreateContext(dataService: dataService);

        var cut = RenderSelect(context);

        await Assert.That(cut.Find("option").TextContent).IsEqualTo("Loading...");
        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsFalse();

        load.SetResult();

        cut.WaitForAssertion(() => cut.Find("option").TextContent.Should().Be("- select -"));
        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task PermissionIsAppliedBeforeLoadingCompletes()
    {
        var load = new TaskCompletionSource();
        var dataService = new FakeDataService { PageDelay = load.Task };
        using var context = CreateContext(dataService: dataService);
        var permission = new TaskCompletionSource<bool>();

        var cut = RenderSelect(context, p => p.Add(x => x.PermissionResolver, () => permission.Task));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();

        permission.SetResult(true);

        cut.WaitForAssertion(() => cut.Find("select").HasAttribute("disabled").Should().BeFalse());
        await Assert.That(cut.Find("option").TextContent).IsEqualTo("Loading...");

        load.SetResult();
    }

    [Test]
    public async Task PermissionResolverIsInvokedOnce()
    {
        using var context = CreateContext();
        var calls = 0;

        var cut = RenderSelect(context, p => p.Add(x => x.PermissionResolver, () =>
        {
            calls++;
            return Task.FromResult(false);
        }));

        cut.Render(p => p.Add(x => x.Placeholder, "- choose -"));

        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();
    }

    [Test]
    public async Task IsDisabledWhenTheRegisteredResolverDeniesAccess()
    {
        var resolver = new FakePermissionResolver { Result = Task.FromResult(false) };
        using var context = CreateContext();
        context.Services.AddSingleton<IPermissionResolver>(resolver);

        var cut = RenderSelect(context, p => p.Add(x => x.Action, "assign"));

        var request = resolver.Requests.Single();
        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsTrue();
        await Assert.That(request.Action).IsEqualTo("assign");
        await Assert.That(request.Subject).IsEqualTo(nameof(TenantHolder));
        await Assert.That(request.Field).IsEqualTo(nameof(TenantHolder.TenantId));
    }

    [Test]
    public async Task RegisteredResolverUsesUpdateAndExplicitSubjectAndField()
    {
        var resolver = new FakePermissionResolver();
        using var context = CreateContext();
        context.Services.AddSingleton<IPermissionResolver>(resolver);

        RenderSelect(context, p => p
            .Add(x => x.Subject, "Tenant")
            .Add(x => x.Field, "Owner"));

        var request = resolver.Requests.Single();
        await Assert.That(request.Action).IsEqualTo("update");
        await Assert.That(request.Subject).IsEqualTo("Tenant");
        await Assert.That(request.Field).IsEqualTo("Owner");
    }

    [Test]
    public async Task RegisteredResolverSubjectRemovesModelSuffix()
    {
        var resolver = new FakePermissionResolver();
        using var context = CreateContext();
        context.Services.AddSingleton<IPermissionResolver>(resolver);

        var model = new TenantUpdateModel();
        var editContext = new EditContext(model);

        context.Render<EntitySelect<TenantModel, int>>(p => p
            .AddCascadingValue(editContext)
            .Add(x => x.Value, model.TenantId)
            .Add(x => x.ValueExpression, () => model.TenantId));

        var request = resolver.Requests.Single();
        await Assert.That(request.Subject).IsEqualTo("Tenant");
        await Assert.That(request.Field).IsEqualTo(nameof(TenantUpdateModel.TenantId));
    }

    [Test]
    public async Task IsEnabledWhenTheRegisteredResolverGrantsAccess()
    {
        var resolver = new FakePermissionResolver();
        using var context = CreateContext();
        context.Services.AddSingleton<IPermissionResolver>(resolver);

        var cut = RenderSelect(context);

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task PermissionResolverParameterTakesPriorityOverTheRegisteredResolver()
    {
        var resolver = new FakePermissionResolver { Result = Task.FromResult(false) };
        using var context = CreateContext();
        context.Services.AddSingleton<IPermissionResolver>(resolver);

        var cut = RenderSelect(context, p => p.Add(x => x.PermissionResolver, () => Task.FromResult(true)));

        await Assert.That(cut.Find("select").HasAttribute("disabled")).IsFalse();
        await Assert.That(resolver.Requests.Count).IsEqualTo(0);
    }

    private static IRenderedComponent<EntitySelect<TenantModel, int>> RenderSelect(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<EntitySelect<TenantModel, int>>>? configure = null)
    {
        var holder = new TenantHolder();
        var editContext = new EditContext(holder);

        return context.Render<EntitySelect<TenantModel, int>>(p =>
        {
            p.AddCascadingValue(editContext);
            p.Add(x => x.Value, holder.TenantId);
            p.Add(x => x.ValueExpression, () => holder.TenantId);
            configure?.Invoke(p);
        });
    }

    private static BunitContext CreateContext(FakeToaster? toaster = null, FakeDataService? dataService = null)
    {
        var context = new BunitContext();
        var toasterInstance = toaster ?? new FakeToaster();
        var dataServiceInstance = dataService ?? new FakeDataService();
        var options = Options.Create(new EnvironmentOptions());

        context.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        context.Services.AddSingleton<IToaster>(toasterInstance);
        context.Services.AddSingleton<INotificationService, NotificationService>();
        context.Services.AddSingleton<IPermissionResolver, DefaultPermissionResolver>();
        context.Services.AddSingleton<Arbiter.Dispatcher.IDispatcherDataService>(dataServiceInstance);
        context.Services.AddSingleton(options);

        return context;
    }
}
