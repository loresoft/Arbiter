using MongoDB.Abstracts;

using Tracker.WebService.Data.Entities;

using Task = System.Threading.Tasks.Task;

namespace Tracker.WebService.Services;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append)]
public class DatabaseInitializer : IHostedService
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializer(ILogger<DatabaseInitializer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var priorityRepository = _serviceProvider.GetRequiredService<IMongoEntityRepository<Priority>>();
        await priorityRepository.UpsertAsync(Constants.PriorityConstants.High, cancellationToken);
        await priorityRepository.UpsertAsync(Constants.PriorityConstants.Normal, cancellationToken);
        await priorityRepository.UpsertAsync(Constants.PriorityConstants.Low, cancellationToken);

        var statusRepository = _serviceProvider.GetRequiredService<IMongoEntityRepository<Status>>();
        await statusRepository.UpsertAsync(Constants.StatusConstants.NotStarted, cancellationToken);
        await statusRepository.UpsertAsync(Constants.StatusConstants.InProgress, cancellationToken);
        await statusRepository.UpsertAsync(Constants.StatusConstants.Completed, cancellationToken);
        await statusRepository.UpsertAsync(Constants.StatusConstants.Blocked, cancellationToken);
        await statusRepository.UpsertAsync(Constants.StatusConstants.Deferred, cancellationToken);
        await statusRepository.UpsertAsync(Constants.StatusConstants.Done, cancellationToken);

        var tenantRepository = _serviceProvider.GetRequiredService<IMongoEntityRepository<Tenant>>();
        await tenantRepository.UpsertAsync(Constants.TenantConstants.Battlestar, cancellationToken);
        await tenantRepository.UpsertAsync(Constants.TenantConstants.Cylons, cancellationToken);

        var userRepository = _serviceProvider.GetRequiredService<IMongoEntityRepository<User>>();
        await userRepository.UpsertAsync(Constants.UserConstants.WilliamAdama, cancellationToken);
        await userRepository.UpsertAsync(Constants.UserConstants.LauraRoslin, cancellationToken);
        await userRepository.UpsertAsync(Constants.UserConstants.KaraThrace, cancellationToken);
        await userRepository.UpsertAsync(Constants.UserConstants.LeeAdama, cancellationToken);
        await userRepository.UpsertAsync(Constants.UserConstants.GaiusBaltar, cancellationToken);
        await userRepository.UpsertAsync(Constants.UserConstants.SaulTigh, cancellationToken);
        await userRepository.UpsertAsync(Constants.UserConstants.NumberSix, cancellationToken);

        var taskRepository = _serviceProvider.GetRequiredService<IMongoEntityRepository<Data.Entities.Task>>();
        await taskRepository.UpsertAsync(Constants.TaskConstants.Earth, cancellationToken);
        await taskRepository.UpsertAsync(Constants.TaskConstants.Destroy, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
