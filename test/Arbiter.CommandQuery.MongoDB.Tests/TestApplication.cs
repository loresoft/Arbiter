using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Abstracts;
using MongoDB.Driver;

using Testcontainers.MongoDb;

using TestHost.Abstracts;

using TUnit.Core.Interfaces;

namespace Arbiter.CommandQuery.MongoDB.Tests;

public class TestApplication : TestHostApplication, IAsyncInitializer
{
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder()
        .WithUsername(string.Empty)
        .WithPassword(string.Empty)
        .Build();

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoDbContainer.DisposeAsync();
        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // change database from container default
        var connectionBuilder = new MongoUrlBuilder(_mongoDbContainer.GetConnectionString())
        {
            DatabaseName = "CommandQueryTracker"
        };

        // override connection string to use docker container
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Tracker"] = connectionBuilder.ToString()
        };

        builder.Configuration.AddInMemoryCollection(configurationData);

        var services = builder.Services;

        services.AddHostedService<DatabaseInitializer>();
        services.AddMongoRepository("Tracker");
        services.AddCommandQuery();

        services.AddArbiterCommandQueryMongoDBTests();
    }
}
