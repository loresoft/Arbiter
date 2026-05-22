using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.OpenTelemetry.Monitor.Tests;

public class TestApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        builder.Services.AddLogQuery();

        base.ConfigureApplication(builder);
    }
}
