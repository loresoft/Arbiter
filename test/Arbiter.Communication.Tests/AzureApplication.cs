using Arbiter.Communication.Azure;
using Arbiter.Communication.Tests.Templates;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.Communication.Tests;

public class AzureApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // dotnet user-secrets set "ConnectionStrings:ArbiterCommunication" "<Connection String>" --id "124f13a8-a53b-4af2-accc-6407ba0340e2"
        // dotnet user-secrets set "Email:FromAddress" "<From Address>" --id "124f13a8-a53b-4af2-accc-6407ba0340e2"
        // dotnet user-secrets set "Email:FromName" "<From Name>"--id "124f13a8-a53b-4af2-accc-6407ba0340e2"

        builder.Configuration.AddUserSecrets("124f13a8-a53b-4af2-accc-6407ba0340e2");

        builder.Services.AddAzureEmailDeliver(
            nameOrConnectionString: "ArbiterCommunication",
            configureOptions: options => options.AddTemplateAssembly<TestApplication>(TemplateNames.TemplateResourceFormat)
        );

        builder.Services.AddAzureSmsDeliver(
            nameOrConnectionString: "ArbiterCommunication",
            configureOptions: options => options.AddTemplateAssembly<TestApplication>(TemplateNames.TemplateResourceFormat)
        );
    }
}
