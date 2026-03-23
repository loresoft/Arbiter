using DbUp;
using DbUp.Engine.Output;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Task = System.Threading.Tasks.Task;

namespace Arbiter.CommandQuery.EntityFramework.Tests;

public class DatabaseInitializer(ILogger<DatabaseInitializer> logger, IConfiguration configuration)
    : IHostedService, IUpgradeLog
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("Tracker");

        // create database
        EnsureDatabase.For.SqlDatabase(connectionString, this);

        var upgradeEngine = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly())
            .LogTo(this)
            .Build();

        var result = upgradeEngine.PerformUpgrade();

        return result.Successful
            ? Task.CompletedTask
            : Task.FromException(result.Error);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;


    public void LogDebug(string format, params object[] args)
        => logger.LogDebug(format, args);

    public void LogError(string format, params object[] args)
        => logger.LogError(format, args);

    public void LogError(Exception ex, string format, params object[] args)
        => logger.LogError(ex, format, args);

    public void LogInformation(string format, params object[] args)
        => logger.LogInformation(format, args);

    public void LogTrace(string format, params object[] args)
        => logger.LogInformation(format, args);

    public void LogWarning(string format, params object[] args)
        => logger.LogWarning(format, args);
}
