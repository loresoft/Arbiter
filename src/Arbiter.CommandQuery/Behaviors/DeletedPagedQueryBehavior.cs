using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for appending soft delete (IsDeleted) filter
/// </summary>
/// <typeparam name="TModel">The type of the entity model.</typeparam>
public class DeletedPagedQueryBehavior<TModel>
    : PipelineBehaviorBase<EntityPagedQuery<TModel>, EntityPagedResult<TModel>>
    where TModel : class
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Lazy<bool> _supportsDelete = new(SupportsDelete);

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedPagedQueryBehavior{TEntityModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from</param>
    public DeletedPagedQueryBehavior(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<EntityPagedResult<TModel>?> Process(
        EntityPagedQuery<TModel> request,
        RequestHandlerDelegate<EntityPagedResult<TModel>> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        // add delete filter
        request.Query.Filter = RewriteFilter(request.Query?.Filter, request.Principal);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Rewrites the specified filter to include a soft delete (IsDeleted) filter.
    /// </summary>
    /// <param name="originalFilter">The original filter to rewrite.</param>
    /// <param name="principal">The claims principal for this behavior.</param>
    /// <returns>An <see cref="EntityFilter"/> with soft delete added</returns>
    protected virtual EntityFilter? RewriteFilter(EntityFilter? originalFilter, ClaimsPrincipal? principal)
    {
        if (!_supportsDelete.Value)
            return originalFilter;

        // don't rewrite if already has filter
        if (HasDeletedFilter(originalFilter))
            return originalFilter;

        var deletedFilter = new EntityFilter
        {
            Name = nameof(ITrackDeleted.IsDeleted),
            Value = false,
            Operator = FilterOperators.Equal,
        };

        if (originalFilter == null)
            return deletedFilter;

        var boolFilter = new EntityFilter
        {
            Logic = FilterLogic.And,
            Filters = [deletedFilter, originalFilter],
        };

        return boolFilter;
    }

    /// <summary>
    /// Determines whether the specified original filter has a soft delete (IsDeleted) filter.
    /// </summary>
    /// <param name="originalFilter">The original filter to check.</param>
    /// <returns>
    ///   <see langword="true"/> if the specified original filter has a soft delete filter; otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool HasDeletedFilter(EntityFilter? originalFilter)
    {
        if (originalFilter == null)
            return false;

        var stack = new Stack<EntityFilter>();
        stack.Push(originalFilter);

        while (stack.Count > 0)
        {
            var filter = stack.Pop();
            if (!string.IsNullOrEmpty(filter.Name) && string.Equals(filter.Name, nameof(ITrackDeleted.IsDeleted), StringComparison.Ordinal))
                return true;

            if (filter.Filters == null)
                continue;

            foreach (var innerFilter in filter.Filters)
                stack.Push(innerFilter);
        }

        return false;
    }

    private static bool SupportsDelete()
    {
        var interfaceType = typeof(ITrackDeleted);
        var entityType = typeof(TModel);

        return interfaceType.IsAssignableFrom(entityType);
    }
}
