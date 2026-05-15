using Arbiter.Communication.Email;
using Arbiter.Communication.Graph;
using Arbiter.Communication.Tests.Models;
using Arbiter.Communication.Tests.Templates;

using AwesomeAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Communication.Tests;

public class GraphTests
{
    [ClassDataSource<GraphApplication>(Shared = SharedType.PerAssembly)]
    public required GraphApplication Application { get; init; }

    public IServiceProvider Services => Application.Services;

    [Test, Skip("Local Only")]
    public async Task SendPasswordResetTest()
    {
        var templateService = Services.GetRequiredService<IEmailTemplateService>();
        templateService.Should().NotBeNull();

        var deliveryService = Services.GetRequiredService<IEmailDeliveryService>();
        deliveryService.Should().NotBeNull();

        var graphDeliveryService = deliveryService as GraphEmailDeliverService;
        graphDeliveryService.Should().NotBeNull();

        var emailModel = new ResetPasswordEmail
        {
            ProductName = "Arbiter",
            CompanyName = "Arbiter Inc.",
            Link = "https://example.com/reset-password?token=12345"
        };

        var configuration = Services.GetRequiredService<IConfiguration>();
        configuration.Should().NotBeNull();

        // dotnet user-secrets set "Email:RecipientAddress" "<Recipient Address>" --id "9a61b9e0-1c1b-4d1d-ae0d-8f6520e96158"
        // dotnet user-secrets set "Email:RecipientName" "<Recipient Name>" --id "9a61b9e0-1c1b-4d1d-ae0d-8f6520e96158"

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
