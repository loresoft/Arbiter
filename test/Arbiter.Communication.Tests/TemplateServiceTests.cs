using Arbiter.Communication.Email;
using Arbiter.Communication.Sms;
using Arbiter.Communication.Template;
using Arbiter.Communication.Tests.Templates;

using AwesomeAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Communication.Tests;

public class TemplateServiceTests : TestBase
{
    [Test]
    public Task GetResourceTemplateEmail()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();

        var resourceName = TemplateNames.GetResourceName(TemplateNames.ResetPasswordEmail);
        var template = templateService.GetResourceTemplate<EmailTemplate>(typeof(TestApplication).Assembly, resourceName);

        template.Should().NotBeNull();
        template.Subject.Should().Be("Reset Password{% if ProductName != blank %} for {{ ProductName }}{% endif %}");
        template.HtmlBody.Should().NotBeEmpty();
        template.TextBody.Should().NotBeEmpty();

        return Task.CompletedTask;
    }

    [Test]
    public Task GetResourceTemplateSms()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();

        var resourceName = TemplateNames.GetResourceName(TemplateNames.VerificationCode);
        var template = templateService.GetResourceTemplate<SmsTemplate>(typeof(TestApplication).Assembly, resourceName);

        template.Should().NotBeNull();
        template.Message.Should().Be("Your verification code{% if ProductName != blank %} for {{ ProductName }}{% endif %} is {{ Code }}.");

        return Task.CompletedTask;
    }
}
