using Arbiter.Components.Services;
using Arbiter.Components.Tests.Services;
using Arbiter.Dispatcher.State;
using Arbiter.Mapping;

using Bunit;

using LoreSoft.Blazor.Controls;
using LoreSoft.Blazor.Controls.Events;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Components.Tests.Abstracts;

public class EditPageBaseTests
{
    [Test]
    public async Task DefaultIdentifierStartsACreateOperation()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 0)).Instance;

        await Assert.That(page.PublicIsCreate).IsTrue();
        await Assert.That(page.CreatedCount).IsEqualTo(1);
    }

    [Test]
    public async Task CreateOperationStartsWithACleanModel()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 0)).Instance;

        await Assert.That(page.PublicIsDirty).IsFalse();
    }

    [Test]
    public async Task EditLabelReflectsTheCreateOperation()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 0)).Instance;

        await Assert.That(page.PublicEditTitle()).IsEqualTo("Invoice Create");
    }

    [Test]
    public async Task EditLabelReflectsTheEditOperation()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        await Assert.That(page.PublicEditTitle()).IsEqualTo("Invoice Edit");
    }

    [Test]
    public async Task ExistingIdentifierLoadsTheModelIntoTheForm()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        await Assert.That(page.PublicModel!.Number).IsEqualTo("INV-7");
        await Assert.That(page.LoadedCount).IsEqualTo(1);
    }

    [Test]
    public async Task NotFoundModelWarnsTheUser()
    {
        var toaster = new FakeToaster();
        var dataService = new FakeDataService { GetResult = null };

        using var context = CreateContext(dataService, toaster);
        context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7));

        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Warning);
    }

    [Test]
    public async Task NotFoundModelUsesTheNotFoundRedirectReason()
    {
        var dataService = new FakeDataService { GetResult = null };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        await Assert.That(page.RedirectReasons).Contains(RedirectReason.NotFound);
    }

    [Test]
    public async Task UpsertCreatesTheModelWhenItIsNotFound()
    {
        var dataService = new FakeDataService { GetResult = null };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters
            .Add(p => p.Id, 7)
            .Add(p => p.EnableUpsert, true)).Instance;

        await Assert.That(page.CreatedCount).IsEqualTo(1);
        await Assert.That(page.RedirectReasons).DoesNotContain(RedirectReason.NotFound);
    }

    [Test]
    public async Task LoadFailureNotifiesTheUser()
    {
        var toaster = new FakeToaster();
        var exception = new InvalidOperationException("boom");
        var dataService = new FakeDataService { GetException = exception };

        using var context = CreateContext(dataService, toaster);
        context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7));

        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Error);
    }

    [Test]
    public async Task SaveIsSkippedWhenTheEditContextIsMissing()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        page.ClearEditContext();
        await page.PublicHandleSave();

        await Assert.That(dataService.SavedModels).IsEmpty();
    }

    [Test]
    public async Task SaveIsSkippedWhenOnSavingAsyncVetoesIt()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters
            .Add(p => p.Id, 7)
            .Add(p => p.AllowSaving, false)).Instance;

        await page.PublicHandleSave();

        await Assert.That(dataService.SavedModels).IsEmpty();
    }

    [Test]
    public async Task SaveSendsTheUpdateModelToTheDataService()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var savedModel = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService
        {
            GetResult = model,
            SaveResult = savedModel,
        };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        await page.PublicHandleSave();

        await Assert.That(dataService.SavedModels.Count).IsEqualTo(1);
        await Assert.That(page.SavedCount).IsEqualTo(1);
    }

    [Test]
    public async Task SaveNotifiesTheUserOnSuccess()
    {
        var toaster = new FakeToaster();
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var savedModel = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService
        {
            GetResult = model,
            SaveResult = savedModel,
        };

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        await page.PublicHandleSave();

        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Success);
    }

    [Test]
    public async Task SaveFailureNotifiesTheUser()
    {
        var toaster = new FakeToaster();
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var exception = new InvalidOperationException("boom");
        var dataService = new FakeDataService
        {
            GetResult = model,
            SaveException = exception,
        };

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        await page.PublicHandleSave();

        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Error);
    }

    [Test]
    public async Task SaveNavigatesToTheCreatedLocationWhenTheIdentifierChanges()
    {
        var savedModel = new InvoiceReadModel { Id = 12, Number = "INV-12" };
        var dataService = new FakeDataService { SaveResult = savedModel };

        using var context = CreateContext(dataService);
        var navigation = context.Services.GetRequiredService<NavigationManager>();

        var page = context.Render<TestEditPage>(parameters => parameters
            .Add(p => p.Id, 0)
            .Add(p => p.RedirectLocation, "/invoices/12")).Instance;

        await page.PublicHandleSave();

        await Assert.That(page.RedirectReasons).Contains(RedirectReason.Created);
        await Assert.That(navigation.Uri).EndsWith("/invoices/12");
    }

    [Test]
    public async Task CancelNavigatesToTheCanceledLocation()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var navigation = context.Services.GetRequiredService<NavigationManager>();

        var page = context.Render<TestEditPage>(parameters => parameters
            .Add(p => p.Id, 7)
            .Add(p => p.RedirectLocation, "/invoices")).Instance;

        await page.PublicHandleCancel();

        await Assert.That(page.RedirectReasons).Contains(RedirectReason.Canceled);
        await Assert.That(navigation.Uri).EndsWith("/invoices");
    }

    [Test]
    public async Task CancelPrefersTheSuppliedRedirectLocation()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var navigation = context.Services.GetRequiredService<NavigationManager>();

        var page = context.Render<TestEditPage>(parameters => parameters
            .Add(p => p.Id, 7)
            .Add(p => p.RedirectLocation, "/invoices")).Instance;

        await page.PublicHandleCancel("/dashboard");

        await Assert.That(navigation.Uri).EndsWith("/dashboard");
    }

    [Test]
    public async Task RefreshReloadsTheModel()
    {
        var model = new InvoiceReadModel { Id = 7, Number = "INV-7" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestEditPage>(parameters => parameters.Add(p => p.Id, 7)).Instance;

        dataService.RequestedIds.Clear();
        await page.PublicHandleRefresh();

        await Assert.That(dataService.RequestedIds).Contains(7);
        await Assert.That(page.LoadedCount).IsEqualTo(2);
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
        context.Services.AddSingleton<IMapper, InvoiceMapper>();
        context.Services.AddSingleton<EventBus>();
        context.Services.AddModals();
        context.Services.AddSingleton<ModelStateEditor<int, InvoiceReadModel, InvoiceUpdateModel>>();

        return context;
    }
}
