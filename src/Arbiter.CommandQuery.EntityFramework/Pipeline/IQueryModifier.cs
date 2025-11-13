using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.EntityFramework.Pipeline;

/// <summary>
/// Defines a modifier that can transform an <see cref="IQueryable{TEntity}"/> query for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of entity being queried.</typeparam>
/// <remarks>
/// <para>
/// Query modifiers are used to apply cross-cutting concerns to entity queries, such as soft delete filtering,
/// tenant isolation, security filtering, or auditing. Modifiers are executed in priority order, with lower
/// priority numbers executing first.
/// </para>
/// <para>
/// Query modifiers can be registered as standard services (applied to all queries) or as keyed services
/// (applied only to named pipelines).
/// </para>
/// <example>
/// Example of a simple soft delete filter modifier:
/// <code>
/// public class SoftDeleteModifier&lt;TEntity&gt; : IQueryModifier&lt;TEntity&gt;
///     where TEntity : class, ISoftDelete
/// {
///     public int Priority =&gt; 10;
///
///     public IQueryable&lt;TEntity&gt; Apply(IQueryable&lt;TEntity&gt; query, DbContext context, ClaimsPrincipal? principal = null)
///     {
///         return query.Where(e =&gt; !e.IsDeleted);
///     }
/// }
/// </code>
/// </example>
/// <example>
/// Registering a standard query modifier (applied to all queries):
/// <code>
/// services.AddScoped&lt;IQueryModifier&lt;MyEntity&gt;, SoftDeleteModifier&lt;MyEntity&gt;&gt;();
/// </code>
/// </example>
/// <example>
/// Registering a named query modifier (applied only when using a named pipeline):
/// <code>
/// // Register modifiers with a service key
/// services.AddKeyedScoped&lt;IQueryModifier&lt;MyEntity&gt;, TenantFilterModifier&gt;("tenant-pipeline");
/// services.AddKeyedScoped&lt;IQueryModifier&lt;MyEntity&gt;, SoftDeleteModifier&lt;MyEntity&gt;&gt;("tenant-pipeline");
///
/// // Use the named pipeline
/// var query = pipeline.ApplyModifiers(baseQuery, context, "tenant-pipeline", principal);
/// </code>
/// </example>
/// </remarks>
public interface IQueryModifier<TEntity> where TEntity : class
{
    /// <summary>
    /// Applies modifications to the specified query.
    /// </summary>
    /// <param name="query">The query to modify.</param>
    /// <param name="context">The database context associated with the query.</param>
    /// <param name="principal">The claims principal representing the current user, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The modified query.</returns>
    ValueTask<IQueryable<TEntity>> Apply(
        IQueryable<TEntity> query,
        DbContext context,
        ClaimsPrincipal? principal = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the priority of this modifier. Lower numbers execute first.
    /// </summary>
    /// <value>The priority value where lower numbers indicate higher priority.</value>
    int Priority { get; }
}
