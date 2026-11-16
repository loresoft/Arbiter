using Arbiter.Communication.Email;
using Arbiter.Communication.Sms;
using Arbiter.Communication.Template;
using Arbiter.Communication.Tests.Models;
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

        var result = templateService.TryGetTemplate<EmailTemplate>(TemplateNames.ResetPasswordEmail, out var template);
        result.Should().BeTrue();

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

        var result = templateService.TryGetTemplate<SmsTemplate>(TemplateNames.VerificationCode, out var template);
        result.Should().BeTrue();

        template.Should().NotBeNull();
        template.Message.Should().Be("Your verification code{% if ProductName != blank %} for {{ ProductName }}{% endif %} is {{ Code }}.");

        return Task.CompletedTask;
    }

    [Test]
    public Task GetResourceTemplateNotFound()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();

        var result = templateService.TryGetTemplate<EmailTemplate>("non-existent-template", out var template);
        result.Should().BeFalse();

        template.Subject.Should().BeNull();
        template.HtmlBody.Should().BeNull();
        template.TextBody.Should().BeNull();

        return Task.CompletedTask;
    }

    [Test]
    public Task ApplyTemplateWithModel()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();

        var model = new ResetPasswordEmail
        {
            Link = "https://example.com/reset",
            ProductName = "Arbiter",
        };

        const string source = "Reset at {{ Link }}{% if ProductName != blank %} for {{ ProductName }}{% endif %} within {{ ExpireHours }} hours";

        var result = templateService.ApplyTemplate(nameof(ApplyTemplateWithModel), source, model);
        result.Should().Be("Reset at https://example.com/reset for Arbiter within 24 hours");

        return Task.CompletedTask;
    }

    [Test]
    public Task ApplyTemplateWithNestedModel()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();

        var user = new { Name = "Test User", Codes = new[] { "123", "456" } };
        var model = new { User = user };

        const string source = "{{ User.Name }}: {% for code in User.Codes %}{{ code }} {% endfor %}";

        var result = templateService.ApplyTemplate(nameof(ApplyTemplateWithNestedModel), source, model);
        result.Should().Be("Test User: 123 456 ");

        return Task.CompletedTask;
    }

    [Test]
    public Task ApplyTemplateCachesByName()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();

        var model = new { Name = "Test" };

        var first = templateService.ApplyTemplate(nameof(ApplyTemplateCachesByName), "First {{ Name }}", model);
        first.Should().Be("First Test");

        // same name returns the cached template, ignoring the new source
        var second = templateService.ApplyTemplate(nameof(ApplyTemplateCachesByName), "Second {{ Name }}", model);
        second.Should().Be("First Test");

        return Task.CompletedTask;
    }

    [Test]
    public Task ApplyTemplateWithoutNameIsNotCached()
    {
        var templateService = Services.GetRequiredService<ITemplateService>();
        templateService.Should().NotBeNull();

        var model = new { Name = "Test" };

        var first = templateService.ApplyTemplate(null, "First {{ Name }}", model);
        first.Should().Be("First Test");

        var second = templateService.ApplyTemplate(null, "Second {{ Name }}", model);
        second.Should().Be("Second Test");

        return Task.CompletedTask;
    }
}
