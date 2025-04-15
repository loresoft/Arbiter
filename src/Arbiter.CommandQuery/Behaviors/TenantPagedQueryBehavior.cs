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
    : TenantFilterBehaviorBase<TKey, TEntityModel, EntityPagedQuery<TEntityModel>, EntityPagedResult<TEntityModel>>
    where TEntityModel : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantPagedQueryBehavior{TKey, TEntityModel}"/> class.
    /// </summary>
    /// <inheritdoc />
    public TenantPagedQueryBehavior(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver)
        : base(loggerFactory, tenantResolver)
    {
    }

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
}
