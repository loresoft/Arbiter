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

        builder.Services.AddEmailDelivery<MemoryEmailDeliverService>(options =>
        {
            options.FromAddress = "UnitTestACE@mailinator.com";
            options.FromName = "Unit Test";

            options.AddTemplateAssembly<TestApplication>(TemplateNames.TemplateResourceFormat);
        });

        builder.Services.AddSmsDelivery<MemorySmsDeliverService>(options =>
        {
            options.SenderNumber = "+8885551212";
            options.AddTemplateAssembly<TestApplication>(TemplateNames.TemplateResourceFormat);
        });
    }
}
