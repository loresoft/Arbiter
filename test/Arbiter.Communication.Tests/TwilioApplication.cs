using Arbiter.Communication.Tests.Templates;
using Arbiter.Communication.Twilio;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.Communication.Tests;

public class TwilioApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // dotnet user-secrets set "Email:ServiceKey" "<SendGrid ApiKey>" --id "5b92be6b-0be6-43cb-a944-709792dbba38"
        // dotnet user-secrets set "Sms:SenderNumber" "<Sender Number>" --id "5b92be6b-0be6-43cb-a944-709792dbba38"
        // dotnet user-secrets set "Sms:UserName" "<Twilio Account SID>" --id "5b92be6b-0be6-43cb-a944-709792dbba38"
        // dotnet user-secrets set "Sms:Password" "<Twilio Auth Token>" --id "5b92be6b-0be6-43cb-a944-709792dbba38"

        builder.Configuration.AddUserSecrets("5b92be6b-0be6-43cb-a944-709792dbba38");

        builder.Services.AddSendGridEmailDeliver(options =>
            options.AddTemplateAssembly<TestApplication>(TemplateNames.TemplateResourceFormat)
        );

        builder.Services.AddTwilioSmsDeliver(options =>
            options.AddTemplateAssembly<TestApplication>(TemplateNames.TemplateResourceFormat)
        );
    }
}
