using MongoDB.Abstracts;

namespace Tracker.WebService.Data;

public static class DataServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddMongoRepository("Tracker");
    }
}
