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
public class QueryPipeline : IQueryPipeline
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPipeline"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve query modifiers.</param>
    public QueryPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> ApplyModifiers<TEntity>(
        IQueryable<TEntity> query,
        DbContext context,
        ClaimsPrincipal? principal = null)
        where TEntity : class
    {
        // Get all registered modifiers for this entity type
        var modifiers = _serviceProvider
            .GetServices<IQueryModifier<TEntity>>()
            .OrderBy(m => m.Priority)
            .ToList();

        // Apply each modifier in order, passing the context and principal
        foreach (var modifier in modifiers)
            query = modifier.Apply(query, context, principal);

        return query;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> ApplyModifiers<TEntity>(
        IQueryable<TEntity> query,
        DbContext context,
        string pipelineName,
        ClaimsPrincipal? principal = null)
        where TEntity : class
    {
        // Get all registered modifiers for this entity type using the named service key
        var modifiers = _serviceProvider
            .GetKeyedServices<IQueryModifier<TEntity>>(pipelineName)
            .OrderBy(m => m.Priority)
            .ToList();

        // Apply each modifier in order, passing the context and principal
        foreach (var modifier in modifiers)
            query = modifier.Apply(query, context, principal);

        return query;
    }
}
