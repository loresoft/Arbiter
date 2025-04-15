using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for setting the default tenant id of a model.
/// </summary>
/// <typeparam name="TKey">The type of the model key</typeparam>
/// <typeparam name="TEntityModel">The type of the model</typeparam>
/// <typeparam name="TResponse">The type of the response</typeparam>
public class TenantDefaultCommandBehavior<TKey, TEntityModel, TResponse>
    : PipelineBehaviorBase<EntityModelCommand<TEntityModel, TResponse>, TResponse>
    where TEntityModel : class
{
    private readonly ITenantResolver<TKey> _tenantResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDefaultCommandBehavior{TKey, TEntityModel, TResponse}"/> class.
    /// </summary>
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">When <paramref name="tenantResolver"/> is null</exception>
    public TenantDefaultCommandBehavior(ILoggerFactory loggerFactory, ITenantResolver<TKey> tenantResolver) : base(loggerFactory)
    {
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
    }

    /// <inheritdoc />
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
