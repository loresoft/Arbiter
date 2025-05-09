using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A base behavior for appending soft delete (IsDeleted) filter
/// </summary>
/// <typeparam name="TEntityModel">The type of the entity model.</typeparam>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class DeletedFilterBehaviorBase<TEntityModel, TRequest, TResponse>
    : PipelineBehaviorBase<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Lazy<bool> _supportsDelete = new(SupportsDelete);

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedFilterBehaviorBase{TEntityModel, TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from</param>
    protected DeletedFilterBehaviorBase(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
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
            Operator = EntityFilterOperators.Equal,
        };

        if (originalFilter == null)
            return deletedFilter;

        var boolFilter = new EntityFilter
        {
            Logic = EntityFilterLogic.And,
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
        var entityType = typeof(TEntityModel);

        return interfaceType.IsAssignableFrom(entityType);
    }
}
