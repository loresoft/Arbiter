using System.Net;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for authenticating a model against the tenant.
/// </summary>
/// <typeparam name="TKey">The type of the model key</typeparam>
/// <typeparam name="TEntityModel">The type of the model</typeparam>
/// <typeparam name="TResponse">The type of the response</typeparam>
public partial class TenantAuthenticateCommandBehavior<TKey, TEntityModel, TResponse>
    : PipelineBehaviorBase<EntityModelBase<TEntityModel, TResponse>, TResponse>
    where TEntityModel : class
{

    private readonly ITenantResolver<TKey> _tenantResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantAuthenticateCommandBehavior{TKey, TEntityModel, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory"> The logger factory to create an <see cref="ILogger"/> from</param>
    /// <param name="tenantResolver"> The tenant resolver service.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="tenantResolver"/> is null</exception>
    public TenantAuthenticateCommandBehavior(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver)
        : base(loggerFactory)
    {
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
    }

    /// <inheritdoc />
    protected override async ValueTask<TResponse?> Process(
        EntityModelBase<TEntityModel, TResponse> request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        await Authorize(request).ConfigureAwait(false);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask Authorize(EntityModelBase<TEntityModel, TResponse> request)
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

        LogTenantAccessDenied(Logger, principal.Identity?.Name, tenantId);

        throw new DomainException(HttpStatusCode.Forbidden, "User does not have access to specified tenant.");
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "User {UserName} does not have access to specified tenant: {TenantId}")]
    private static partial void LogTenantAccessDenied(ILogger logger, string? userName, TKey? tenantId);
}
