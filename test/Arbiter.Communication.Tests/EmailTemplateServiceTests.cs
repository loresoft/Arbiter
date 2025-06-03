using Arbiter.Communication.Email;
using Arbiter.Communication.Tests.Models;
using Arbiter.Communication.Tests.Templates;

using AwesomeAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Communication.Tests;

public class EmailTemplateServiceTests : TestBase
{
    [Test]
    public async Task SendPasswordResetTest()
    {
        var templateService = Services.GetRequiredService<IEmailTemplateService>();
        templateService.Should().NotBeNull();

        var emailModel = new ResetPasswordEmail
        {
            ProductName = "Arbiter",
            CompanyName = "Arbiter Inc.",
            Link = "https://example.com/reset-password?token=12345"
        };

        var recipients = EmailBuilder.Create()
            .To("recipient@example.com", "Test Recipient")
            .BuildRecipients();

        recipients.Should().NotBeNull();
        recipients.To.Should().NotBeEmpty();
        recipients.To[0].Address.Should().Be("recipient@example.com");
        recipients.To[0].DisplayName.Should().Be("Test Recipient");

        var result = await templateService.Send(TemplateNames.ResetPasswordEmail, emailModel, recipients);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();

        var deliveryService = Services.GetRequiredService<IEmailDeliveryService>();
        deliveryService.Should().NotBeNull();

        var memoryDeliveryService = deliveryService as MemoryEmailDeliverService;
        memoryDeliveryService.Should().NotBeNull();

        memoryDeliveryService.Messages.Should().NotBeEmpty();

        var message = memoryDeliveryService.Messages.FirstOrDefault();
        message.Should().NotBeNull();

        message.Senders.From.Should().NotBeNull();

        message.Content.Subject.Should().NotBeNull();
        message.Content.Subject.Should().Be("Reset Password for Arbiter");
        message.Content.HtmlBody.Should().NotBeNullOrEmpty();
        message.Content.TextBody.Should().NotBeNullOrEmpty();
    }
}
