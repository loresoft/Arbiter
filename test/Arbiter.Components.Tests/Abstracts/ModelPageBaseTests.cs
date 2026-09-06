using Arbiter.Components.Services;
using Arbiter.Components.Tests.Services;

using Bunit;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Components.Tests.Abstracts;

public class ModelPageBaseTests
{
    [Test]
    public async Task ModelLabelSplitsTypeNameAndRemovesModelSuffix()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;

        await Assert.That(page.PublicModelLabel).IsEqualTo("Purchase Order");
    }

    [Test]
    public async Task ModelNameRemovesModelSuffixWithoutSpaces()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;

        await Assert.That(page.PublicModelName).IsEqualTo("PurchaseOrder");
    }

    [Test]
    public async Task ModelNameUsesTypeNameWhenSuffixIsNotRecognized()
    {
        using var context = CreateContext();

        var page = context.Render<TestSupplierPage>().Instance;

        await Assert.That(page.PublicModelName).IsEqualTo("Supplier");
    }

    [Test]
    public async Task PageTitleUsesModelLabelOnlyWhenNothingElseIsSet()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;

        await Assert.That(page.PublicPageTitle()).IsEqualTo("Purchase Order");
    }

    [Test]
    public async Task PageTitleAppendsSuffix()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;

        await Assert.That(page.PublicPageTitle("Edit")).IsEqualTo("Purchase Order Edit");
    }

    [Test]
    public async Task PageTitleAppendsModelDisplay()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        page.DisplayName = "PO-10432";

        await Assert.That(page.PublicPageTitle("Edit")).IsEqualTo("Purchase Order Edit - PO-10432");
    }

    [Test]
    public async Task PageTitleAppendsAsteriskWhenDirty()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        page.DisplayName = "PO-10432";
        page.Dirty = true;

        await Assert.That(page.PublicPageTitle()).IsEqualTo("Purchase Order - PO-10432 *");
    }

    [Test]
    public async Task CancellationTokenIsNotCanceledBeforeDispose()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;

        await Assert.That(page.PublicCancellationToken.IsCancellationRequested).IsFalse();
    }

    [Test]
    public async Task DisposeCancelsTheCancellationToken()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        var token = page.PublicCancellationToken;

        page.Dispose();

        await Assert.That(token.IsCancellationRequested).IsTrue();
        await Assert.That(page.PublicIsDisposed).IsTrue();
    }

    [Test]
    public async Task CancellationTokenIsAlreadyCanceledAfterDispose()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        page.Dispose();

        await Assert.That(page.PublicCancellationToken.IsCancellationRequested).IsTrue();
    }

    [Test]
    public async Task DisposeIsIdempotent()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        page.Dispose();

        var action = page.Dispose;

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task ObserveRunsTheOperation()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        var completion = new TaskCompletionSource();

        page.PublicObserve(_ =>
        {
            completion.SetResult();
            return Task.CompletedTask;
        });

        await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task ObserveDoesNotRunTheOperationAfterDispose()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        page.Dispose();

        var invoked = false;
        page.PublicObserve(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await Assert.That(invoked).IsFalse();
    }

    [Test]
    public async Task ObserveNotifiesTheUserWhenTheOperationFails()
    {
        var toaster = new FakeToaster();
        using var context = CreateContext(toaster);

        var page = context.Render<TestModelPage>().Instance;

        var exception = new InvalidOperationException("boom");
        page.PublicObserve(_ => Task.FromException(exception));

        await WaitForAsync(() => toaster.Toasts.Count > 0);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Error);
    }

    [Test]
    public async Task ObserveDoesNotNotifyTheUserWhenNotifyErrorIsFalse()
    {
        var toaster = new FakeToaster();
        using var context = CreateContext(toaster);

        var page = context.Render<TestModelPage>().Instance;
        var completion = new TaskCompletionSource();
        var exception = new InvalidOperationException("boom");

        page.PublicObserve(
            _ =>
            {
                completion.SetResult();
                return Task.FromException(exception);
            },
            notifyError: false);

        await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(toaster.Toasts).IsEmpty();
    }

    [Test]
    public async Task ObserveIgnoresCancellation()
    {
        var toaster = new FakeToaster();
        using var context = CreateContext(toaster);

        var page = context.Render<TestModelPage>().Instance;
        var completion = new TaskCompletionSource();
        var canceledToken = new CancellationToken(canceled: true);

        page.PublicObserve(_ =>
        {
            completion.SetResult();
            return Task.FromCanceled(canceledToken);
        });

        await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(toaster.Toasts).IsEmpty();
    }

    [Test]
    public async Task HandleModelChangeDoesNotThrowAfterDispose()
    {
        using var context = CreateContext();

        var page = context.Render<TestModelPage>().Instance;
        page.Dispose();

        var action = page.RaiseModelChange;

        await Assert.That(action).ThrowsNothing();
    }

    private static BunitContext CreateContext(FakeToaster? toaster = null)
    {
        var context = new BunitContext();
        var toasterInstance = toaster ?? new FakeToaster();

        context.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        context.Services.AddAuthorizationCore();
        context.Services.AddSingleton<IToaster>(toasterInstance);
        context.Services.AddSingleton<INotificationService, NotificationService>();

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
