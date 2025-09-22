using System.Security.Claims;
using System.Security.Principal;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Filters;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for appending tenant filter to a query.
/// </summary>
/// <typeparam name="TKey">The type of the model key</typeparam>
/// <typeparam name="TEntityModel">The type of the model</typeparam>
/// <typeparam name="TRequest">The type of the request</typeparam>
/// <typeparam name="TResponse">The type of the response</typeparam>
public abstract class TenantFilterBehaviorBase<TKey, TEntityModel, TRequest, TResponse>
    : PipelineBehaviorBase<TRequest, TResponse>
    where TEntityModel : class
    where TRequest : class, IRequest<TResponse>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Lazy<bool> _supportsTenant = new(SupportsTenant);

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantFilterBehaviorBase{TKey, TEntityModel, TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory"> The logger factory to create an <see cref="ILogger"/> from</param>
    /// <param name="tenantResolver"> The tenant resolver service.</param>
    /// <exception cref="ArgumentNullException"></exception>
    protected TenantFilterBehaviorBase(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver) : base(loggerFactory)
    {
        TenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
    }

    /// <summary>
    /// Gets the tenant resolver service.
    /// </summary>
    protected ITenantResolver<TKey> TenantResolver { get; }

    /// <summary>
    /// Rewrites the filter to include the tenant filter.
    /// </summary>
    /// <param name="originalFilter">The original filter to add the tenant filter to</param>
    /// <param name="principal">The claims principal for this behavior.</param>
    /// <returns>An <see cref="EntityFilter"/> with tenant filter added</returns>
    /// <exception cref="DomainException">When failed find a tenant for the request</exception>
    protected virtual async ValueTask<EntityFilter?> RewriteFilter(EntityFilter? originalFilter, ClaimsPrincipal? principal)
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
            Operator = FilterOperators.Equal,
        };

        if (originalFilter == null)
            return tenantFilter;

        return new EntityFilter
        {
            Logic = FilterLogic.And,
            Filters = [tenantFilter, originalFilter],
        };
    }

    private static bool SupportsTenant()
    {
        var interfaceType = typeof(IHaveTenant<TKey>);
        var entityType = typeof(TEntityModel);

        return interfaceType.IsAssignableFrom(entityType);
    }
}
