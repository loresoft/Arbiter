using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Services;
using Arbiter.Components.Tests.Services;
using Arbiter.Dispatcher.State;

using Bunit;

using LoreSoft.Blazor.Controls;
using LoreSoft.Blazor.Controls.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Components.Tests.Abstracts;

public class NestedListPageBaseTests
{
    [Test]
    public async Task ParentModelIsLoadedForTheSuppliedIdentifier()
    {
        var model = new InvoiceReadModel { Id = 5, Number = "INV-5" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.PublicModel!.Number).IsEqualTo("INV-5");
        await Assert.That(page.LoadedCount).IsEqualTo(1);
    }

    [Test]
    public async Task LoadDataReturnsTheItemsAndTotalFromTheStore()
    {
        var listModel = new PurchaseOrderListModel { Id = 1, Number = "PO-1" };
        var pageResult = new EntityPagedResult<PurchaseOrderListModel>
        {
            Data = [listModel],
            Total = 42,
        };
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 }, PageResult = pageResult };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        var request = new DataRequest { Page = 1, PageSize = 10 };
        var result = await page.PublicLoadData(request);

        await Assert.That(result.Total).IsEqualTo(42);
        await Assert.That(result.Items.Count()).IsEqualTo(1);
    }

    [Test]
    public async Task LoadDataSendsThePagingFromTheRequest()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        var request = new DataRequest { Page = 3, PageSize = 50 };
        await page.PublicLoadData(request);

        await Assert.That(dataService.PageQueries[0]!.Page).IsEqualTo(3);
        await Assert.That(dataService.PageQueries[0]!.PageSize).IsEqualTo(50);
    }

    [Test]
    public async Task LoadDataSendsTheCombinedFilter()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;
        page.UseCombinedFilter = true;
        page.CombinedFilter = new EntityFilter { Name = "InvoiceId", Value = 5 };

        var request = new DataRequest { Page = 1, PageSize = 10 };
        await page.PublicLoadData(request);

        await Assert.That(dataService.PageQueries[0]!.Filter!.Name).IsEqualTo("InvoiceId");
    }

    [Test]
    public async Task LoadDataReturnsEmptyAndNotifiesWhenTheLoadFails()
    {
        var toaster = new FakeToaster();
        var dataService = new FakeDataService
        {
            GetResult = new InvoiceReadModel { Id = 5 },
            PageException = new InvalidOperationException("boom"),
        };

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

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
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;
        page.Dispose();

        var request = new DataRequest { Page = 1, PageSize = 10 };
        var result = await page.PublicLoadData(request);

        await Assert.That(result.Items).IsEmpty();
        await Assert.That(toaster.Toasts).IsEmpty();
    }

    [Test]
    public async Task CombineFilterReturnsTheGridFilterByDefault()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        var original = new EntityFilter { Name = "Status" };
        var combined = page.PublicCombineFilter(original);

        await Assert.That(combined).IsSameReferenceAs(original);
    }

    [Test]
    public async Task GetDisplayNameUsesTheModelToStringByDefault()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        var listModel = new PurchaseOrderListModel { Id = 1, Number = "PO-10432" };
        var name = page.PublicGetDisplayName(listModel);

        await Assert.That(name).IsEqualTo("PO-10432");
    }

    [Test]
    public async Task GetDisplayNameThrowsWhenTheModelIsNull()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        var action = () => page.PublicGetDisplayName(null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task DefaultQueryIsNullWhenNoQueryIsCreated()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.PublicDefaultQuery).IsNull();
    }

    [Test]
    public async Task DefaultQueryUsesTheQueryCreatedFromTheParameters()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };
        var filters = new List<QueryRule> { new QueryFilter { Field = "InvoiceId", Operator = QueryOperators.Equal, Value = 5 } };
        var query = new QueryGroup { Filters = filters };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters
            .Add(p => p.Id, 5)
            .Add(p => p.Query, query)).Instance;

        await Assert.That(page.PublicDefaultQuery).IsSameReferenceAs(query);
    }

    [Test]
    public async Task HandleRefreshReloadsTheParentModel()
    {
        var model = new InvoiceReadModel { Id = 5, Number = "INV-5" };
        var dataService = new FakeDataService { GetResult = model };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        dataService.RequestedIds.Clear();
        await page.PublicHandleRefresh();

        await Assert.That(dataService.RequestedIds).Contains(5);
        await Assert.That(page.LoadedCount).IsEqualTo(2);
    }

    [Test]
    public async Task RefreshListDoesNothingWhenTheDataComponentIsNotAssigned()
    {
        var dataService = new FakeDataService { GetResult = new InvoiceReadModel { Id = 5 } };

        using var context = CreateContext(dataService);
        var page = context.Render<TestNestedListPage>(parameters => parameters.Add(p => p.Id, 5)).Instance;

        await Assert.That(page.PublicDataComponent).IsNull();
        await page.PublicRefreshList();
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
        context.Services.AddSingleton<EventBus>();
        context.Services.AddModals();

        return context;
    }
}
