using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Services;
using Arbiter.Components.Tests.Services;

using Bunit;

using LoreSoft.Blazor.Controls;
using LoreSoft.Blazor.Controls.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Components.Tests.Abstracts;

public class ListPageBaseTests
{
    [Test]
    public async Task LoadDataReturnsTheItemsAndTotalFromTheStore()
    {
        var model = new PurchaseOrderListModel { Id = 1, Number = "PO-1" };
        var pageResult = new EntityPagedResult<PurchaseOrderListModel>
        {
            Data = [model],
            Total = 42,
        };
        var dataService = new FakeDataService { PageResult = pageResult };

        using var context = CreateContext(dataService);
        var page = context.Render<TestListPage>().Instance;

        var request = new DataRequest { Page = 1, PageSize = 10 };
        var result = await page.PublicLoadData(request);

        await Assert.That(result.Total).IsEqualTo(42);
        await Assert.That(result.Items.Count()).IsEqualTo(1);
    }

    [Test]
    public async Task LoadDataSendsThePagingFromTheRequest()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestListPage>().Instance;

        var request = new DataRequest { Page = 3, PageSize = 50 };
        await page.PublicLoadData(request);

        await Assert.That(dataService.PageQueries[0]!.Page).IsEqualTo(3);
        await Assert.That(dataService.PageQueries[0]!.PageSize).IsEqualTo(50);
    }

    [Test]
    public async Task LoadDataSendsTheCombinedFilter()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestListPage>().Instance;
        page.UseCombinedFilter = true;
        page.CombinedFilter = new EntityFilter { Name = "TenantId", Value = 7 };

        var request = new DataRequest { Page = 1, PageSize = 10 };
        await page.PublicLoadData(request);

        await Assert.That(dataService.PageQueries[0]!.Filter!.Name).IsEqualTo("TenantId");
    }

    [Test]
    public async Task LoadDataReturnsEmptyAndNotifiesWhenTheLoadFails()
    {
        var toaster = new FakeToaster();
        var exception = new InvalidOperationException("boom");
        var dataService = new FakeDataService { PageException = exception };

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestListPage>().Instance;

        var request = new DataRequest { Page = 1, PageSize = 10 };
        var result = await page.PublicLoadData(request);

        await Assert.That(result.Total).IsEqualTo(0);
        await Assert.That(result.Items).IsEmpty();
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Error);
    }

    [Test]
    public async Task LoadDataReturnsEmptyWithoutNotifyingWhenCanceled()
    {
        var toaster = new FakeToaster();
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestListPage>().Instance;
        page.Dispose();

        var request = new DataRequest { Page = 1, PageSize = 10 };
        var result = await page.PublicLoadData(request);

        await Assert.That(result.Items).IsEmpty();
        await Assert.That(toaster.Toasts).IsEmpty();
    }

    [Test]
    public async Task CombineFilterReturnsTheGridFilterByDefault()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestListPage>().Instance;

        var original = new EntityFilter { Name = "Status" };
        var combined = page.PublicCombineFilter(original);

        await Assert.That(combined).IsSameReferenceAs(original);
    }

    [Test]
    public async Task GetDisplayNameUsesTheModelToStringByDefault()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestListPage>().Instance;

        var model = new PurchaseOrderListModel { Id = 1, Number = "PO-10432" };
        var name = page.PublicGetDisplayName(model);

        await Assert.That(name).IsEqualTo("PO-10432");
    }

    [Test]
    public async Task GetDisplayNameThrowsWhenTheModelIsNull()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestListPage>().Instance;

        var action = () => page.PublicGetDisplayName(null!);

        await Assert.That(action).Throws<ArgumentNullException>();
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
        context.Services.AddSingleton<EventBus>();
        context.Services.AddModals();

        return context;
    }
}
