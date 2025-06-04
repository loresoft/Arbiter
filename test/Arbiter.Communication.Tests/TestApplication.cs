using Arbiter.Communication.Email;
using Arbiter.Communication.Sms;
using Arbiter.Communication.Tests.Templates;

using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.Communication.Tests;

public class TestApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        builder.Services
            .AddTemplateResourceResolver(TemplateNames.TemplateAssembly, TemplateNames.TemplateResourceFormat)
            .AddTemplateResolver<MockTemplateResolver>()
            .AddEmailDelivery<MemoryEmailDeliverService>(options =>
            {
                options.FromAddress = "UnitTestACE@mailinator.com";
                options.FromName = "Unit Test";
            })
            .AddSmsDelivery<MemorySmsDeliverService>(options => options.SenderNumber = "+8885551212");
    }
}
