using Arbiter.Communication.Sms;
using Arbiter.Communication.Tests.Models;
using Arbiter.Communication.Tests.Templates;

using AwesomeAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Communication.Tests;

public class SmsTemplateServiceTests : TestBase
{
    [Test]
    public async Task SendVerificationCodeTest()
    {
        var templateService = Services.GetRequiredService<ISmsTemplateService>();
        templateService.Should().NotBeNull();

        var securityCodeMessage = new SecurityCodeMessage
        {
            ProductName = "Arbiter",
            Code = "123456"
        };

        var result = await templateService.Send(TemplateNames.VerificationCode, securityCodeMessage, "+2225551212");

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();

        var deliveryService = Services.GetRequiredService<ISmsDeliveryService>();
        deliveryService.Should().NotBeNull();

        var memoryDeliveryService = deliveryService as MemorySmsDeliverService;
        memoryDeliveryService.Should().NotBeNull();

        memoryDeliveryService.Messages.Should().NotBeEmpty();

        var message = memoryDeliveryService.Messages.FirstOrDefault();
        message.Should().NotBeNull();

        message.Recipient.Should().NotBeNull();
        message.Recipient.Should().Be("+2225551212");

        message.Message.Should().NotBeNullOrEmpty();
        message.Message.Should().Be("Your verification code for Arbiter is 123456.");
    }
}
