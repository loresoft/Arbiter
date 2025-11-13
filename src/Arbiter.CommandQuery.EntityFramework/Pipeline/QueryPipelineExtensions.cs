using System.Security.Claims;

using Arbiter.CommandQuery.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Arbiter.CommandQuery.EntityFramework.Pipeline;

/// <summary>
/// Extension methods for applying query pipelines to Entity Framework queries.
/// </summary>
public static class QueryPipelineExtensions
{
    /// <summary>
    /// Applies the configured query pipeline modifiers to the specified query.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <param name="query">The queryable to apply the pipeline to.</param>
    /// <param name="pipeline">The query pipeline containing the modifiers to apply.</param>
    /// <param name="context">The database context containing the pipeline configuration.</param>
    /// <param name="pipelineName">Optional name of a specific pipeline to apply. If not provided, the default pipeline will be used.</param>
    /// <param name="principal">Optional claims principal for security-based query modifications.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, containing the modified queryable.</returns>
    /// <remarks>
    /// If no pipeline is configured in the context, the original query is returned unmodified.
    /// When a pipeline name is specified, only modifiers registered for that named pipeline are applied.
    /// </remarks>
    public static async ValueTask<IQueryable<TEntity>> ApplyPipeline<TEntity>(
        this IQueryable<TEntity> query,
        IQueryPipeline? pipeline,
        DbContext context,
        string? pipelineName = null,
        ClaimsPrincipal? principal = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        if (pipeline is null)
            return query;

        return pipelineName.HasValue()
            ? await pipeline.ApplyModifiers(query, context, pipelineName, principal, cancellationToken).ConfigureAwait(false)
            : await pipeline.ApplyModifiers(query, context, principal, cancellationToken).ConfigureAwait(false);
    }

}
