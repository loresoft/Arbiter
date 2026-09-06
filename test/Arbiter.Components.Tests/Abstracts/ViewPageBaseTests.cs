using Arbiter.Components.Services;
using Arbiter.Components.Tests.Services;
using Arbiter.Dispatcher.State;

using Bunit;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Components.Tests.Abstracts;

public class ViewPageBaseTests
{
    [Test]
    public async Task ModelIsLoadedForTheSuppliedIdentifier()
    {
        var model = new InvoiceReadModel { Id = 5, Number = "INV-5" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.PublicModel!.Number).IsEqualTo("INV-5");
        await Assert.That(dataService.RequestedIds).Contains(5);
    }

    [Test]
    public async Task OnLoadedAsyncRunsAfterTheModelIsLoaded()
    {
        var model = new InvoiceReadModel { Id = 5, Number = "INV-5" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.LoadedCount).IsEqualTo(1);
    }

    [Test]
    public async Task OnLoadedAsyncDoesNotRunWhenTheModelIsNotFound()
    {
        var dataService = new FakeDataService { GetResult = null };

        using var context = CreateContext(dataService);
        var page = context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.LoadedCount).IsEqualTo(0);
    }

    [Test]
    public async Task NotFoundModelWarnsTheUser()
    {
        var toaster = new FakeToaster();
        var dataService = new FakeDataService { GetResult = null };

        using var context = CreateContext(dataService, toaster);
        context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5));

        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Warning);
    }

    [Test]
    public async Task NotFoundModelUsesTheRedirectReason()
    {
        var dataService = new FakeDataService { GetResult = null };

        using var context = CreateContext(dataService);
        var page = context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.RedirectReasons).Contains(RedirectReason.NotFound);
    }

    [Test]
    public async Task NotFoundModelNavigatesToTheRedirectLocationWhenSupplied()
    {
        var dataService = new FakeDataService { GetResult = null };

        using var context = CreateContext(dataService);
        var navigation = context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        context.Render<TestViewPage>(parameters => parameters
            .Add(p => p.Id, 5)
            .Add(p => p.RedirectLocation, "/invoices"));

        await Assert.That(navigation.Uri).EndsWith("/invoices");
    }

    [Test]
    public async Task LoadFailureNotifiesTheUser()
    {
        var toaster = new FakeToaster();
        var exception = new InvalidOperationException("boom");
        var dataService = new FakeDataService { GetException = exception };

        using var context = CreateContext(dataService, toaster);
        context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5));

        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Error);
    }

    [Test]
    public async Task HandleRefreshReloadsTheModel()
    {
        var model = new InvoiceReadModel { Id = 5, Number = "INV-5" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        dataService.RequestedIds.Clear();
        await page.PublicHandleRefresh();

        await Assert.That(dataService.RequestedIds).Contains(5);
        await Assert.That(page.LoadedCount).IsEqualTo(2);
    }

    [Test]
    public async Task PageTitleIncludesTheLoadedModelDisplay()
    {
        var model = new InvoiceReadModel { Id = 5, Number = "INV-5" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.PublicModelDisplay).IsEqualTo("Invoice - INV-5");
    }

    [Test]
    public async Task IsBusyIsFalseAfterTheLoadCompletes()
    {
        var model = new InvoiceReadModel { Id = 5, Number = "INV-5" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestViewPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.PublicIsBusy).IsFalse();
    }

    private static BunitContext CreateContext(FakeDataService dataService, FakeToaster? toaster = null)
    {
        var context = new BunitContext();
        var toasterInstance = toaster ?? new FakeToaster();

        context.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        context.Services.AddAuthorizationCore();
        context.Services.AddSingleton<IToaster>(toasterInstance);
        context.Services.AddSingleton<INotificationService, NotificationService>();
        context.Services.AddSingleton<Arbiter.Dispatcher.IDispatcherDataService>(dataService);
        context.Services.AddSingleton<ModelStateLoader<int, InvoiceReadModel>>();

        return context;
    }
}
