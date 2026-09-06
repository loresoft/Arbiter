using Arbiter.Components.Services;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Arbiter.Components.Tests.Services;

public class NotificationServiceTests
{
    [Test]
    public async Task ConstructorThrowsWhenToasterIsNull()
    {
        var action = () => new NotificationService(NullLogger<NotificationService>.Instance, null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowInformationDisplaysMessage()
    {
        var toaster = new FakeToaster();
        var service = CreateService(toaster);

        service.ShowInformation("loaded");

        await Assert.That(toaster.Toasts.Count).IsEqualTo(1);
        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Information);
        await Assert.That(toaster.Toasts[0].Message).IsEqualTo("loaded");
    }

    [Test]
    public async Task ShowSuccessDisplaysMessage()
    {
        var toaster = new FakeToaster();
        var service = CreateService(toaster);

        service.ShowSuccess("saved");

        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Success);
        await Assert.That(toaster.Toasts[0].Message).IsEqualTo("saved");
    }

    [Test]
    public async Task ShowErrorUsesConfiguredTimeout()
    {
        var toaster = new FakeToaster();
        var options = new NotificationServiceOptions { ErrorTimeout = 42 };
        var service = CreateService(toaster, options);

        service.ShowError("failed");

        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Error);
        await Assert.That(toaster.Toasts[0].Settings.Timeout).IsEqualTo(42);
    }

    [Test]
    public async Task ShowWarningUsesConfiguredTimeout()
    {
        var toaster = new FakeToaster();
        var options = new NotificationServiceOptions { WarningTimeout = 7 };
        var service = CreateService(toaster, options);

        service.ShowWarning("careful");

        await Assert.That(toaster.Toasts[0].Level).IsEqualTo(ToastLevel.Warning);
        await Assert.That(toaster.Toasts[0].Settings.Timeout).IsEqualTo(7);
    }

    [Test]
    public async Task ShowErrorThrowsWhenExceptionIsNull()
    {
        var toaster = new FakeToaster();
        var service = CreateService(toaster);

        var action = () => service.ShowError((Exception)null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowWarningThrowsWhenExceptionIsNull()
    {
        var toaster = new FakeToaster();
        var service = CreateService(toaster);

        var action = () => service.ShowWarning((Exception)null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShowErrorHidesExceptionDetailByDefault()
    {
        var toaster = new FakeToaster();
        var options = new NotificationServiceOptions { ErrorMessage = "Something went wrong." };
        var service = CreateService(toaster, options);

        var exception = new InvalidOperationException("connection string is invalid");
        service.ShowError(exception);

        await Assert.That(toaster.Toasts[0].Message).IsEqualTo("Something went wrong.");
    }

    [Test]
    public async Task ShowErrorWithMessageHidesExceptionDetailByDefault()
    {
        var toaster = new FakeToaster();
        var service = CreateService(toaster);

        var exception = new InvalidOperationException("connection string is invalid");
        service.ShowError(exception, "Unable to save the record");

        await Assert.That(toaster.Toasts[0].Message).IsEqualTo("Unable to save the record");
    }

    [Test]
    public async Task ShowErrorIncludesExceptionDetailWhenEnabled()
    {
        var toaster = new FakeToaster();
        var options = new NotificationServiceOptions { ShowExceptionDetails = true };
        var service = CreateService(toaster, options);

        var exception = new InvalidOperationException("boom");
        service.ShowError(exception, "Unable to save the record");

        await Assert.That(toaster.Toasts[0].Message).IsEqualTo("Unable to save the record: boom");
    }

    [Test]
    public async Task ShowErrorUsesExceptionMessageWhenDetailEnabledAndNoMessage()
    {
        var toaster = new FakeToaster();
        var options = new NotificationServiceOptions { ShowExceptionDetails = true };
        var service = CreateService(toaster, options);

        var exception = new InvalidOperationException("boom");
        service.ShowError(exception);

        await Assert.That(toaster.Toasts[0].Message).IsEqualTo("boom");
    }

    [Test]
    public async Task ShowWarningHidesExceptionDetailByDefault()
    {
        var toaster = new FakeToaster();
        var options = new NotificationServiceOptions { WarningMessage = "Did not complete." };
        var service = CreateService(toaster, options);

        var exception = new InvalidOperationException("internal detail");
        service.ShowWarning(exception);

        await Assert.That(toaster.Toasts[0].Message).IsEqualTo("Did not complete.");
    }

    [Test]
    public async Task ClearRemovesNotifications()
    {
        var toaster = new FakeToaster();
        var service = CreateService(toaster);

        service.Clear();

        await Assert.That(toaster.ClearCount).IsEqualTo(1);
    }

    private static NotificationService CreateService(FakeToaster toaster, NotificationServiceOptions? options = null)
    {
        return new NotificationService(
            NullLogger<NotificationService>.Instance,
            toaster,
            options == null ? null : Options.Create(options));
    }
}
