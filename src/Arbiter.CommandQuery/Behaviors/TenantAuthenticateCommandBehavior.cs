using System.Net;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

public class TenantAuthenticateCommandBehavior<TKey, TEntityModel, TResponse>
    : PipelineBehaviorBase<EntityModelCommand<TEntityModel, TResponse>, TResponse>
    where TEntityModel : class
{

    private readonly ITenantResolver<TKey> _tenantResolver;

    public TenantAuthenticateCommandBehavior(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver)
        : base(loggerFactory)
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

        await Authorize(request).ConfigureAwait(false);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask Authorize(EntityModelCommand<TEntityModel, TResponse> request)
    {
        var principal = request.Principal;
        if (principal == null)
            return;

        // check principal tenant is same as model tenant
        if (request.Model is not IHaveTenant<TKey> tenantModel)
            return;

        var tenantId = await _tenantResolver.GetTenantId(principal).ConfigureAwait(false);
        if (Equals(tenantId, tenantModel.TenantId))
            return;

        Logger.LogError("User {UserName} does not have access to specified tenant: {TenantId}", principal.Identity?.Name, tenantId);

        throw new DomainException(HttpStatusCode.Forbidden, "User does not have access to specified tenant.");
    }
}
