using Arbiter.Communication.Azure;
using Arbiter.Communication.Email;
using Arbiter.Communication.Tests.Models;
using Arbiter.Communication.Tests.Templates;

using AwesomeAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Communication.Tests;

public class AzureTests
{
    [ClassDataSource<AzureApplication>(Shared = SharedType.PerAssembly)]
    public required AzureApplication Application { get; init; }

    public IServiceProvider Services => Application.Services;

    [Test, Skip("Local Only")]
    public async Task SendPasswordResetTest()
    {
        var templateService = Services.GetRequiredService<IEmailTemplateService>();
        templateService.Should().NotBeNull();

        var deliveryService = Services.GetRequiredService<IEmailDeliveryService>();
        deliveryService.Should().NotBeNull();

        var sendGridDeliveryService = deliveryService as AzureEmailDeliveryService;
        sendGridDeliveryService.Should().NotBeNull();

        var emailModel = new ResetPasswordEmail
        {
            ProductName = "Arbiter",
            CompanyName = "Arbiter Inc.",
            Link = "https://example.com/reset-password?token=12345"
        };

        var configuration = Services.GetRequiredService<IConfiguration>();
        configuration.Should().NotBeNull();

        // dotnet user-secrets set "Email:RecipientAddress" "<Recipient Address>" --id "124f13a8-a53b-4af2-accc-6407ba0340e2"
        // dotnet user-secrets set "Email:RecipientName" "<Recipient Name>" --id "124f13a8-a53b-4af2-accc-6407ba0340e2"

        var recipientAddress = configuration.GetValue<string>("Email:RecipientAddress");
        recipientAddress.Should().NotBeNullOrEmpty();

        var recipientName = configuration.GetValue<string>("Email:RecipientName");
        recipientName.Should().NotBeNullOrEmpty();

        var recipients = EmailBuilder.Create()
            .To(recipientAddress, recipientName)
            .BuildRecipients();

        recipients.Should().NotBeNull();
        recipients.To.Should().NotBeEmpty();
        recipients.To[0].Address.Should().Be(recipientAddress);

        var result = await templateService.Send(TemplateNames.ResetPasswordEmail, emailModel, recipients);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
    }
}
