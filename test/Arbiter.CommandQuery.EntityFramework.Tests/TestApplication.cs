using Arbiter.Dispatcher;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Testcontainers.MsSql;

using TestHost.Abstracts;

using TUnit.Core.Interfaces;

namespace Arbiter.CommandQuery.EntityFramework.Tests;

public class TestApplication : TestHostApplication, IAsyncInitializer
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPassword("Bn87bBYhLjYRj%9zRgUc")
        .Build();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // change database from container default
        var connectionBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
        {
            InitialCatalog = "TrackerCommandQuery"
        };

        // override connection string to use docker container
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Tracker"] = connectionBuilder.ToString()
        };

        builder.Configuration.AddInMemoryCollection(configurationData);

        var services = builder.Services;

        services.AddHostedService<DatabaseInitializer>();
        services.AddCommandQuery();
        services.AddQueryPipeline();

        services.AddServerDispatcher();
        services.AddArbiterCommandQueryEntityFrameworkTests();
    }
}
