using Arbiter.CommandQuery;

namespace Tracker.WebService.Domain;

public static class DomainServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DomainServiceRegistration).Assembly);
        services.AddCommandQuery();
    }
}
