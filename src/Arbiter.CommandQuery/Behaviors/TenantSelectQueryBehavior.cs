using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for appending tenant filter to a select query.
/// </summary>
/// <typeparam name="TKey">The type of the model key</typeparam>
/// <typeparam name="TEntityModel">The type of the model</typeparam>
public class TenantSelectQueryBehavior<TKey, TEntityModel>
    : TenantFilterBehaviorBase<TKey, TEntityModel, EntitySelectQuery<TEntityModel>, IReadOnlyCollection<TEntityModel>>
    where TEntityModel : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantSelectQueryBehavior{TKey, TEntityModel}"/> class.
    /// </summary>
    /// <inheritdoc />
    public TenantSelectQueryBehavior(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver)
        : base(loggerFactory, tenantResolver)
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IReadOnlyCollection<TEntityModel>?> Process(
        EntitySelectQuery<TEntityModel> request,
        RequestHandlerDelegate<IReadOnlyCollection<TEntityModel>> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        // add tenant filter
        request.Select.Filter = await RewriteFilter(request.Select?.Filter, request.Principal).ConfigureAwait(false);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }
}
