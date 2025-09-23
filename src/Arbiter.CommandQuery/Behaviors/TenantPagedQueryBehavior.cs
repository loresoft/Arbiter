using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for appending tenant filter to a paged query.
/// </summary>
/// <typeparam name="TKey">The type of the model key</typeparam>
/// <typeparam name="TEntityModel">The type of the model</typeparam>
public class TenantPagedQueryBehavior<TKey, TEntityModel>
    : PipelineBehaviorBase<EntityPagedQuery<TEntityModel>, EntityPagedResult<TEntityModel>>
    where TEntityModel : class
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Lazy<bool> _supportsTenant = new(SupportsTenant);

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantPagedQueryBehavior{TKey, TEntityModel}"/> class.
    /// </summary>
    /// <inheritdoc />
    public TenantPagedQueryBehavior(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver)
        : base(loggerFactory)
    {
        TenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
    }

    /// <summary>
    /// Gets the tenant resolver service.
    /// </summary>
    protected ITenantResolver<TKey> TenantResolver { get; }

    /// <inheritdoc />
    protected override async ValueTask<EntityPagedResult<TEntityModel>?> Process(
        EntityPagedQuery<TEntityModel> request,
        RequestHandlerDelegate<EntityPagedResult<TEntityModel>> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        // add tenant filter
        request.Query.Filter = await RewriteFilter(request.Query?.Filter, request.Principal).ConfigureAwait(false);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }

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
