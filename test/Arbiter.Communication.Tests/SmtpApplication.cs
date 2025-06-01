using Arbiter.Communication.Tests.Templates;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.Communication.Tests;

public class SmtpApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // dotnet user-secrets set "Email:Server" "<value>" --id "8e031d65-7a0b-4b43-a160-3ab9601963b1"
        // dotnet user-secrets set "Email:UserName" "<value>" --id "8e031d65-7a0b-4b43-a160-3ab9601963b1"
        // dotnet user-secrets set "Email:Password" "<value>" --id "8e031d65-7a0b-4b43-a160-3ab9601963b1"
        // dotnet user-secrets set "Email:FromAddress" "<value>" --id "8e031d65-7a0b-4b43-a160-3ab9601963b1"
        // dotnet user-secrets set "Email:FromName" "<value>" --id "8e031d65-7a0b-4b43-a160-3ab9601963b1"
        // dotnet user-secrets set "Email:RecipientAddress" "<value>" --id "8e031d65-7a0b-4b43-a160-3ab9601963b1"
        // dotnet user-secrets set "Email:RecipientName" "<value>" --id "8e031d65-7a0b-4b43-a160-3ab9601963b1"

        builder.Configuration.AddUserSecrets("8e031d65-7a0b-4b43-a160-3ab9601963b1");

        builder.Services.AddSmtpEmailDeliver(
            configureOptions: options => options.AddTemplateAssembly<TestApplication>(TemplateNames.TemplateResourceFormat)
        );
    }
}
