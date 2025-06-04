using Arbiter.Communication.Email;
using Arbiter.Communication.Sms;
using Arbiter.Communication.Template;

using AwesomeAssertions;

using Fluid;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arbiter.Communication.Tests;

public class DependencyInjectionTests : TestBase
{
    [Test]
    public void VerifyTemplateServiceRegistration()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();
        templateService.Should().BeOfType<TemplateService>();

        // Verify that the service is registered as a singleton
        Services.GetService<ITemplateService>().Should().BeSameAs(templateService);

    }

    [Test]
    public void VerifyFluidParserRegistration()
    {
        var fluidParser = Services.GetRequiredService<FluidParser>();
        fluidParser.Should().NotBeNull();

        // Verify that the parser is registered as a singleton
        Services.GetService<FluidParser>().Should().BeSameAs(fluidParser);
    }

    [Test]
    public void VerifyEmailTemplateServiceRegistration()
    {
        var emailTemplateService = Services.GetRequiredService<IEmailTemplateService>();
        emailTemplateService.Should().NotBeNull();
        emailTemplateService.Should().BeOfType<EmailTemplateService>();

        // Verify that the service is registered as a singleton
        Services.GetService<IEmailTemplateService>().Should().BeSameAs(emailTemplateService);
    }

    [Test]
    public void VerifySmsTemplateServiceRegistration()
    {
        var smsTemplateService = Services.GetRequiredService<ISmsTemplateService>();
        smsTemplateService.Should().NotBeNull();
        smsTemplateService.Should().BeOfType<SmsTemplateService>();

        // Verify that the service is registered as a singleton
        Services.GetService<ISmsTemplateService>().Should().BeSameAs(smsTemplateService);
    }

    [Test]
    public void VerifyEmailDeliveryServiceRegistration()
    {
        var emailDeliveryService = Services.GetRequiredService<IEmailDeliveryService>();
        emailDeliveryService.Should().NotBeNull();

        // Assuming MemoryEmailDeliverService is the default implementation
        emailDeliveryService.Should().BeOfType<MemoryEmailDeliverService>();

        // Verify that the service is registered as a singleton
        Services.GetService<IEmailDeliveryService>().Should().BeSameAs(emailDeliveryService);
    }

    [Test]
    public void VerifySmsDeliveryServiceRegistration()
    {
        var smsDeliveryService = Services.GetRequiredService<ISmsDeliveryService>();
        smsDeliveryService.Should().NotBeNull();
        // Assuming MemorySmsDeliverService is the default implementation
        smsDeliveryService.Should().BeOfType<MemorySmsDeliverService>();
        // Verify that the service is registered as a singleton
        Services.GetService<ISmsDeliveryService>().Should().BeSameAs(smsDeliveryService);
    }

    [Test]
    public void VerifyEmailConfigurationRegistration()
    {
        var emailConfig = Services.GetRequiredService<IOptions<EmailConfiguration>>();
        emailConfig.Should().NotBeNull();
        emailConfig.Value.Should().NotBeNull();
        emailConfig.Value.FromAddress.Should().Be("UnitTestACE@mailinator.com");
        emailConfig.Value.FromName.Should().Be("Unit Test");
    }

    [Test]
    public void VerifySmsConfigurationRegistration()
    {
        var smsConfig = Services.GetRequiredService<IOptions<SmsConfiguration>>();
        smsConfig.Should().NotBeNull();
        smsConfig.Value.Should().NotBeNull();
        smsConfig.Value.SenderNumber.Should().Be("+8885551212");
    }

    [Test]
    public void VerifyTemplateResourceResolverRegistration()
    {
        var resolver = Services.GetServices<ITemplateResolver>();
        resolver.Should().NotBeNull();
    }

}
