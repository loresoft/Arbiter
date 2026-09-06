using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Services;
using Arbiter.Components.Tests.Services;

using Bunit;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Components.Tests.Abstracts;

public class DataPageBaseTests
{
    [Test]
    public async Task DataLoaderReturnsTheLoadedModels()
    {
        var model = new PurchaseOrderReadModel { Number = "PO-1" };
        var pageResult = new EntityPagedResult<PurchaseOrderReadModel>
        {
            Data = [model],
            Total = 1,
        };
        var dataService = new FakeDataService { PageResult = pageResult };

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;

        var results = await page.PublicDataLoader();

        await Assert.That(results.Count()).IsEqualTo(1);
        await Assert.That(page.PublicData[0].Number).IsEqualTo("PO-1");
    }

    [Test]
    public async Task DataLoaderPassesTheQueryFromCreateQuery()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;
        page.Query = new EntityQuery { PageSize = 17 };

        await page.PublicDataLoader();

        await Assert.That(dataService.PageQueries.Count).IsEqualTo(1);
        await Assert.That(dataService.PageQueries[0]!.PageSize).IsEqualTo(17);
    }

    [Test]
    public async Task LoadDataReturnsEmptyWhenTheStoreReturnsNoData()
    {
        var pageResult = new EntityPagedResult<PurchaseOrderReadModel>();
        var dataService = new FakeDataService { PageResult = pageResult };

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;

        var results = await page.PublicLoadData();

        await Assert.That(results).IsEmpty();
    }

    [Test]
    public async Task DataLoaderReturnsEmptyAndNotifiesWhenTheLoadFails()
    {
        var toaster = new FakeToaster();
        var exception = new InvalidOperationException("boom");
        var dataService = new FakeDataService { PageException = exception };

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestResultPage>().Instance;

        var results = await page.PublicDataLoader();

        await Assert.That(results).IsEmpty();
        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Error);
    }

    [Test]
    public async Task DataLoaderReturnsEmptyWithoutNotifyingWhenCanceled()
    {
        var toaster = new FakeToaster();
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService, toaster);
        var page = context.Render<TestResultPage>().Instance;
        page.Dispose();

        var results = await page.PublicDataLoader();

        await Assert.That(results).IsEmpty();
        await Assert.That(toaster.Toasts).IsEmpty();
    }

    [Test]
    public async Task OnAfterRenderSubscribesOnceTheComponentReferenceIsAvailable()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;
        page.Subscribed.Clear();

        var dataComponent = new TestDataComponent();
        page.SetDataComponent(dataComponent);
        page.TriggerAfterRender();

        await Assert.That(page.Subscribed).Contains(dataComponent);
    }

    [Test]
    public async Task OnAfterRenderDoesNotResubscribeTheSameComponent()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;

        var dataComponent = new TestDataComponent();
        page.SetDataComponent(dataComponent);
        page.TriggerAfterRender();

        page.Subscribed.Clear();
        page.TriggerAfterRender();

        await Assert.That(page.Subscribed).IsEmpty();
    }

    [Test]
    public async Task OnAfterRenderUnsubscribesThePreviousComponentWhenTheReferenceChanges()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;

        var first = new TestDataComponent();
        page.SetDataComponent(first);
        page.TriggerAfterRender();

        var second = new TestDataComponent();
        page.SetDataComponent(second);
        page.Unsubscribed.Clear();
        page.TriggerAfterRender();

        await Assert.That(page.Unsubscribed).Contains(first);
        await Assert.That(page.Subscribed).Contains(second);
    }

    [Test]
    public async Task DisposeUnsubscribesTheComponent()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;

        var dataComponent = new TestDataComponent();
        page.SetDataComponent(dataComponent);
        page.TriggerAfterRender();

        page.Unsubscribed.Clear();
        page.Dispose();

        await Assert.That(page.Unsubscribed).Contains(dataComponent);
    }

    [Test]
    public async Task OnDataRefreshedRunsOnLoadedAsync()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;

        page.PublicOnDataRefreshed();

        await WaitForAsync(() => page.LoadedCount > 0);
        await Assert.That(page.LoadedCount).IsEqualTo(1);
    }

    [Test]
    public async Task OnDataRefreshedDoesNotRunOnLoadedAsyncAfterDispose()
    {
        var dataService = new FakeDataService();

        using var context = CreateContext(dataService);
        var page = context.Render<TestResultPage>().Instance;
        page.Dispose();

        page.PublicOnDataRefreshed();

        await Assert.That(page.LoadedCount).IsEqualTo(0);
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

        return context;
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        var timeout = DateTime.UtcNow.AddSeconds(5);

        while (!condition() && DateTime.UtcNow < timeout)
            await Task.Delay(10);

        if (!condition())
            throw new TimeoutException("The condition was not met before the timeout elapsed.");
    }
}
