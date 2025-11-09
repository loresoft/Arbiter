using Arbiter.Communication.Email;
using Arbiter.Communication.Sms;
using Arbiter.Communication.Tests.Models;
using Arbiter.Communication.Tests.Templates;
using Arbiter.Communication.Twilio;

using AwesomeAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Communication.Tests;

public class TwilioTests
{
    [ClassDataSource<TwilioApplication>(Shared = SharedType.PerAssembly)]
    public required TwilioApplication Application { get; init; }

    public IServiceProvider Services => Application.Services;

    [Test, Skip("Local Only")]
    public async Task SendPasswordResetTest()
    {
        var templateService = Services.GetRequiredService<IEmailTemplateService>();
        templateService.Should().NotBeNull();

        var deliveryService = Services.GetRequiredService<IEmailDeliveryService>();
        deliveryService.Should().NotBeNull();

        var sendGridDeliveryService = deliveryService as SendGridEmailDeliveryService;
        sendGridDeliveryService.Should().NotBeNull();

        var emailModel = new ResetPasswordEmail
        {
            ProductName = "Arbiter",
            CompanyName = "Arbiter Inc.",
            Link = "https://example.com/reset-password?token=12345"
        };

        var recipients = EmailBuilder.Create()
            .To("SendPasswordResetTest@mailinator.com", "Send Grid")
            .BuildRecipients();

        recipients.Should().NotBeNull();
        recipients.To.Should().NotBeEmpty();
        recipients.To[0].Address.Should().Be("SendPasswordResetTest@mailinator.com");

        var result = await templateService.Send(TemplateNames.ResetPasswordEmail, emailModel, recipients);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
    }

    [Test, Skip("Local Only")]
    public async Task SendPasswordResetBccTest()
    {
        var templateService = Services.GetRequiredService<IEmailTemplateService>();
        templateService.Should().NotBeNull();

        var deliveryService = Services.GetRequiredService<IEmailDeliveryService>();
        deliveryService.Should().NotBeNull();

        var sendGridDeliveryService = deliveryService as SendGridEmailDeliveryService;
        sendGridDeliveryService.Should().NotBeNull();

        var emailModel = new ResetPasswordEmail
        {
            ProductName = "Arbiter",
            CompanyName = "Arbiter Inc.",
            Link = "https://example.com/reset-password?token=12345"
        };

        var recipients = EmailBuilder.Create()
            .Bcc("SendPasswordResetTest@mailinator.com", "Send Grid")
            .BuildRecipients();

        recipients.Should().NotBeNull();
        recipients.Bcc.Should().NotBeEmpty();
        recipients.Bcc[0].Address.Should().Be("SendPasswordResetTest@mailinator.com");

        var result = await templateService.Send(TemplateNames.ResetPasswordEmail, emailModel, recipients);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
    }

    [Test, Skip("Local Only")]
    public async Task SendVerificationCodeTest()
    {
        var templateService = Services.GetRequiredService<ISmsTemplateService>();
        templateService.Should().NotBeNull();

        var deliveryService = Services.GetRequiredService<ISmsDeliveryService>();
        deliveryService.Should().NotBeNull();

        var twilioDeliveryService = deliveryService as TwilioSmsDeliveryService;
        twilioDeliveryService.Should().NotBeNull();

        var configuration = Services.GetRequiredService<IConfiguration>();

        var securityCodeMessage = new SecurityCodeMessage
        {
            ProductName = "Arbiter",
            Code = "123456"
        };

        // dotnet user-secrets set "Sms:RecipientNumber" "<Recipient Number>" --id "5b92be6b-0be6-43cb-a944-709792dbba38"
        var recipient = configuration.GetValue<string>("Sms:RecipientNumber");
        recipient.Should().NotBeNullOrEmpty();

        var result = await templateService.Send(TemplateNames.VerificationCode, securityCodeMessage, recipient);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
    }

}
