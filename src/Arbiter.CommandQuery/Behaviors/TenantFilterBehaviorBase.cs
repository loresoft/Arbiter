using System.Security.Principal;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

public abstract class TenantFilterBehaviorBase<TKey, TEntityModel, TRequest, TResponse>
    : PipelineBehaviorBase<TRequest, TResponse>
    where TEntityModel : class
    where TRequest : class, IRequest<TResponse>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Lazy<bool> _supportsTenant = new(SupportsTenant);


    protected TenantFilterBehaviorBase(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver) : base(loggerFactory)
    {
        TenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
    }

    protected ITenantResolver<TKey> TenantResolver { get; }


    protected virtual async ValueTask<EntityFilter?> RewriteFilter(EntityFilter? originalFilter, IPrincipal? principal)
    {
        if (!_supportsTenant.Value)
            return originalFilter;

        var tenantId = await TenantResolver.GetTenantId(principal).ConfigureAwait(false);
        if (Equals(tenantId, default(TKey)))
        {
            Logger.LogError("Could not find tenant for the query request.");
            throw new DomainException(500, "Could not find tenant for the query request.");
        }

        var tenantFilter = new EntityFilter
        {
            Name = nameof(IHaveTenant<TKey>.TenantId),
            Value = tenantId,
            Operator = EntityFilterOperators.Equal
        };

        if (originalFilter == null)
            return tenantFilter;

        var boolFilter = new EntityFilter
        {
            Logic = EntityFilterLogic.And,
            Filters = new List<EntityFilter>
                {
                    tenantFilter,
                    originalFilter
                }
        };

        return boolFilter;
    }

    private static bool SupportsTenant()
    {
        var interfaceType = typeof(IHaveTenant<TKey>);
        var entityType = typeof(TEntityModel);

        return interfaceType.IsAssignableFrom(entityType);
    }

}
