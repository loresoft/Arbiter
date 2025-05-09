using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for appending soft delete (IsDeleted) filter
/// </summary>
/// <typeparam name="TEntityModel">The type of the entity model.</typeparam>
public class DeletedPagedQueryBehavior<TEntityModel>
    : DeletedFilterBehaviorBase<TEntityModel, EntityPagedQuery<TEntityModel>, EntityPagedResult<TEntityModel>>
    where TEntityModel : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedPagedQueryBehavior{TEntityModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from</param>
    public DeletedPagedQueryBehavior(ILoggerFactory loggerFactory) : base(loggerFactory)
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

        // add delete filter
        request.Query.Filter = RewriteFilter(request.Query?.Filter, request.Principal);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }
}
