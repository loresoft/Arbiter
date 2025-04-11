using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

public class TenantDefaultCommandBehavior<TKey, TEntityModel, TResponse>
    : PipelineBehaviorBase<EntityModelCommand<TEntityModel, TResponse>, TResponse>
    where TEntityModel : class
{
    private readonly ITenantResolver<TKey> _tenantResolver;


    public TenantDefaultCommandBehavior(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver) : base(loggerFactory)
    {
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
    }

    protected override async ValueTask<TResponse?> Process(
        EntityModelCommand<TEntityModel, TResponse> request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        await SetTenantId(request).ConfigureAwait(false);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask SetTenantId(EntityModelCommand<TEntityModel, TResponse> request)
    {
        if (request.Model is not IHaveTenant<TKey> tenantModel)
            return;

        if (!Equals(tenantModel.TenantId, default(TKey)))
            return;

        var tenantId = await _tenantResolver.GetTenantId(request.Principal).ConfigureAwait(false);
        tenantModel.TenantId = tenantId!;
    }
}
