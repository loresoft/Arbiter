using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.EntityFramework.Pipeline;

/// <summary>
/// Default implementation of <see cref="IQueryPipeline"/> that applies query modifiers in priority order.
/// </summary>
/// <remarks>
/// <para>
/// This implementation retrieves all registered query modifiers for the specified entity type,
/// sorts them by priority (lower numbers execute first), and applies them sequentially to the query.
/// Modifiers are resolved from the dependency injection container and can be registered either as
/// standard services or as keyed services for named pipelines.
/// </para>
/// <para>
/// The pipeline supports two modes of operation:
/// <list type="bullet">
/// <item><description>Standard mode: Applies all modifiers registered without a service key</description></item>
/// <item><description>Named mode: Applies only modifiers registered with a specific service key</description></item>
/// </list>
/// </para>
/// <example>
/// Example registration in a startup or configuration class:
/// <code>
/// // Register the query pipeline
/// services.AddScoped&lt;IQueryPipeline, QueryPipeline&gt;();
///
/// // Register standard query modifiers (no key)
/// services.AddScoped&lt;IQueryModifier&lt;Product&gt;, SoftDeleteModifier&lt;Product&gt;&gt;();
/// services.AddScoped&lt;IQueryModifier&lt;Order&gt;, TenantFilterModifier&lt;Order&gt;&gt;();
///
/// // Register named query modifiers (with key)
/// services.AddKeyedScoped&lt;IQueryModifier&lt;Product&gt;, AdminFilterModifier&gt;("admin-only");
/// services.AddKeyedScoped&lt;IQueryModifier&lt;Product&gt;, AuditModifier&gt;("admin-only");
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="IQueryPipeline"/>
/// <seealso cref="IQueryModifier{TEntity}"/>
public class QueryPipeline : IQueryPipeline
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPipeline"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve query modifiers from the dependency injection container.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public QueryPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Applies all registered standard query modifiers for the specified entity type in priority order.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being queried. Must be a reference type.</typeparam>
    /// <param name="query">The queryable to modify.</param>
    /// <param name="context">The database context associated with the query, providing access to entity metadata and configuration.</param>
    /// <param name="principal">The claims principal representing the current user, if available. This enables security-based query modifications.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, containing the modified queryable
    /// after all standard modifiers have been applied in priority order.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method resolves all <see cref="IQueryModifier{TEntity}"/> instances registered without a service key
    /// and applies them sequentially in ascending priority order (lower values first).
    /// </para>
    /// <para>
    /// If no modifiers are registered for the entity type, the original query is returned unchanged.
    /// </para>
    /// <para>
    /// Each modifier is awaited sequentially to maintain the proper order of operations and to support
    /// asynchronous modifier implementations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var query = context.Set&lt;Product&gt;().AsQueryable();
    ///
    /// // Apply all standard modifiers (e.g., soft delete filter, tenant filter)
    /// query = await pipeline.ApplyModifiers(query, context, currentUser);
    ///
    /// var products = await query.ToListAsync();
    /// </code>
    /// </example>
    public async ValueTask<IQueryable<TEntity>> ApplyModifiers<TEntity>(
        IQueryable<TEntity> query,
        DbContext context,
        ClaimsPrincipal? principal = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        // Get all registered modifiers for this entity type
        var modifiers = _serviceProvider
            .GetServices<IQueryModifier<TEntity>>()
            .OrderBy(m => m.Priority)
            .ToList();

        if (modifiers.Count == 0)
            return query;

        // Apply each modifier in order, passing the context and principal
        foreach (var modifier in modifiers)
            query = await modifier.Apply(query, context, principal, cancellationToken).ConfigureAwait(false);

        return query;
    }

    /// <summary>
    /// Applies all registered named query modifiers for the specified entity type in priority order.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being queried. Must be a reference type.</typeparam>
    /// <param name="query">The queryable to modify.</param>
    /// <param name="context">The database context associated with the query, providing access to entity metadata and configuration.</param>
    /// <param name="pipelineName">The name of the pipeline used as a service key to resolve query modifiers. Only modifiers registered with this exact key will be applied.</param>
    /// <param name="principal">The claims principal representing the current user, if available. This enables security-based query modifications.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, containing the modified queryable
    /// after all named modifiers have been applied in priority order.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method resolves all <see cref="IQueryModifier{TEntity}"/> instances registered with the specified
    /// <paramref name="pipelineName"/> as a service key and applies them sequentially in ascending priority order (lower values first).
    /// </para>
    /// <para>
    /// Named pipelines enable different sets of modifiers to be applied based on context. For example, you might have
    /// an "admin" pipeline with relaxed security filters and a "public" pipeline with stricter filters.
    /// </para>
    /// <para>
    /// If no modifiers are registered for the specified pipeline name and entity type, the original query is returned unchanged.
    /// </para>
    /// <para>
    /// Each modifier is awaited sequentially to maintain the proper order of operations and to support
    /// asynchronous modifier implementations.
    /// </para>
    /// </remarks>
    /// <example>
    /// Example using a named pipeline for administrative users:
    /// <code>
    /// var query = context.Set&lt;Product&gt;().AsQueryable();
    ///
    /// // Apply only modifiers registered with the "admin-pipeline" key
    /// query = await pipeline.ApplyModifiers(query, context, "admin-pipeline", currentUser);
    ///
    /// // This might include archived items that would normally be filtered out
    /// var allProducts = await query.ToListAsync();
    /// </code>
    /// </example>
    public async ValueTask<IQueryable<TEntity>> ApplyModifiers<TEntity>(
        IQueryable<TEntity> query,
        DbContext context,
        string pipelineName,
        ClaimsPrincipal? principal = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        // Get all registered modifiers for this entity type using the named service key
        var modifiers = _serviceProvider
            .GetKeyedServices<IQueryModifier<TEntity>>(pipelineName)
            .OrderBy(m => m.Priority)
            .ToList();

        if (modifiers.Count == 0)
            return query;

        // Apply each modifier in order, passing the context and principal
        foreach (var modifier in modifiers)
            query = await modifier.Apply(query, context, principal, cancellationToken).ConfigureAwait(false);

        return query;
    }
}
