using Arbiter.CommandQuery.EntityFramework.Pipeline;
using Arbiter.CommandQuery.EntityFramework.Tests.Pipeline;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Context = Arbiter.CommandQuery.EntityFramework.Tests.Data;
using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain;

public static class AuditServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.Audit, int, Models.AuditReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.Audit, int, Models.AuditReadModel, Models.AuditCreateModel, Models.AuditUpdateModel>();
        #endregion

        // entity modifiers, registered out of priority order to verify ordering
        services.TryAddSingleton<AuditModifierRecorder>();
        services.AddScoped<IEntityModifier<Entities.Audit>, AuditDefaultModifier>();
        services.AddScoped<IEntityModifier<Entities.Audit>, AuditFirstModifier>();
        services.AddKeyedScoped<IEntityModifier<Entities.Audit>, AuditNamedModifier>(AuditModifierRecorder.PipelineName);
    }
}
