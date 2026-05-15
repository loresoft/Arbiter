using Arbiter.Communication.Graph;
using Arbiter.Communication.Tests.Templates;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.Communication.Tests;

public class GraphApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // dotnet user-secrets set "Email:TenantId" "<Tenant Id>" --id "9a61b9e0-1c1b-4d1d-ae0d-8f6520e96158"
        // dotnet user-secrets set "Email:ClientId" "<Client Id>" --id "9a61b9e0-1c1b-4d1d-ae0d-8f6520e96158"
        // dotnet user-secrets set "Email:ServiceKey" "<Client Secret>" --id "9a61b9e0-1c1b-4d1d-ae0d-8f6520e96158"
        // dotnet user-secrets set "Email:FromAddress" "<Graph sender address>" --id "9a61b9e0-1c1b-4d1d-ae0d-8f6520e96158"

        builder.Configuration.AddUserSecrets("9a61b9e0-1c1b-4d1d-ae0d-8f6520e96158");

        builder.Services
            .AddTemplateResourceResolver(TemplateNames.TemplateAssembly, TemplateNames.TemplateResourceFormat)
            .AddGraphEmailDeliver();
    }
}
