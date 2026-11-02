using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Notifications;

using Microsoft.EntityFrameworkCore;

namespace Arbiter.CommandQuery.EntityFramework.Pipeline;

/// <summary>
/// Defines a pipeline that applies registered query modifiers to entity queries and registered entity modifiers
/// to entities before they are saved.
/// </summary>
/// <remarks>
/// <para>
/// The query pipeline supports both standard modifiers (registered without a key) and named modifiers
/// (registered with a service key). This allows different sets of modifiers to be applied based on context.
/// </para>
/// <example>
/// Example of using the standard pipeline:
/// <code>
/// public class MyQueryHandler
/// {
///     private readonly IQueryPipeline _pipeline;
///     private readonly MyDbContext _context;
///
///     public async Task&lt;List&lt;MyEntity&gt;&gt; HandleAsync()
///     {
///         var query = _context.Set&lt;MyEntity&gt;().AsQueryable();
///
///         // Apply all registered standard modifiers
///         query = _pipeline.ApplyModifiers(query, _context);
///
///         return await query.ToListAsync();
///     }
/// }
/// </code>
/// </example>
/// <example>
/// Example of using a named pipeline:
/// <code>
/// public class MyQueryHandler
/// {
///     private readonly IQueryPipeline _pipeline;
///     private readonly MyDbContext _context;
///
///     public async Task&lt;List&lt;MyEntity&gt;&gt; HandleAsync(ClaimsPrincipal user)
///     {
///         var query = _context.Set&lt;MyEntity&gt;().AsQueryable();
///
///         // Apply modifiers registered with "admin-pipeline" key
///         query = _pipeline.ApplyModifiers(query, _context, "admin-pipeline", user);
///
///         return await query.ToListAsync();
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface IQueryPipeline
{
    /// <summary>
    /// Applies all registered modifiers for the specified entity type in priority order.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <param name="query">The query to modify.</param>
    /// <param name="context">The database context associated with the query.</param>
    /// <param name="principal">The claims principal representing the current user, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query after all modifiers have been applied.</returns>
    ValueTask<IQueryable<TEntity>> ApplyModifiers<TEntity>(
        IQueryable<TEntity> query,
        DbContext context,
        ClaimsPrincipal? principal = null,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Applies all registered modifiers for the specified entity type in priority order using a named pipeline.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <param name="query">The query to modify.</param>
    /// <param name="context">The database context associated with the query.</param>
    /// <param name="pipelineName">The name of the pipeline to use for resolving query modifiers.</param>
    /// <param name="principal">The claims principal representing the current user, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query after all modifiers have been applied.</returns>
    /// <remarks>
    /// <para>
    /// This method resolves query modifiers that were registered with the specified <paramref name="pipelineName"/>
    /// as a service key. Only modifiers registered with this exact key will be applied.
    /// </para>
    /// <para>
    /// To register named query modifiers, use the keyed service registration methods:
    /// </para>
    /// <code>
    /// services.AddKeyedScoped&lt;IQueryModifier&lt;MyEntity&gt;, MyModifier&gt;("pipeline-name");
    /// </code>
    /// </remarks>
    ValueTask<IQueryable<TEntity>> ApplyModifiers<TEntity>(
        IQueryable<TEntity> query,
        DbContext context,
        string pipelineName,
        ClaimsPrincipal? principal = null,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Applies all registered entity modifiers for the specified entity type in priority order.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being saved.</typeparam>
    /// <param name="entity">The entity being saved.</param>
    /// <param name="context">The database context the entity is being saved with.</param>
    /// <param name="operation">The change operation being performed on the entity.</param>
    /// <param name="principal">The request principal representing the current user and activation information, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// Applies all <see cref="IEntityModifier{TEntity}"/> instances registered without a service key.
    /// </para>
    /// <para>
    /// The Entity Framework command handlers call this method before changes are saved, so changes made by the
    /// modifiers are persisted in the same <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> call.
    /// </para>
    /// <example>
    /// Example of applying entity modifiers in a custom command handler:
    /// <code>
    /// await pipeline.ApplyModifiers(entity, dataContext, EntityChangeOperation.Created, request, cancellationToken);
    /// await dataContext.SaveChangesAsync(cancellationToken);
    /// </code>
    /// </example>
    /// </remarks>
    ValueTask ApplyModifiers<TEntity>(
        TEntity entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Applies all registered entity modifiers for the specified entity type in priority order using a named pipeline.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being saved.</typeparam>
    /// <param name="entity">The entity being saved.</param>
    /// <param name="context">The database context the entity is being saved with.</param>
    /// <param name="operation">The change operation being performed on the entity.</param>
    /// <param name="pipelineName">The name of the pipeline to use for resolving entity modifiers.</param>
    /// <param name="principal">The request principal representing the current user and activation information, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method resolves entity modifiers that were registered with the specified <paramref name="pipelineName"/>
    /// as a service key. Only modifiers registered with this exact key will be applied.
    /// </para>
    /// <para>
    /// To register named entity modifiers, use the keyed service registration methods:
    /// </para>
    /// <code>
    /// services.AddKeyedScoped&lt;IEntityModifier&lt;MyEntity&gt;, MyModifier&gt;("pipeline-name");
    /// </code>
    /// </remarks>
    ValueTask ApplyModifiers<TEntity>(
        TEntity entity,
        DbContext context,
        EntityChangeOperation operation,
        string pipelineName,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default)
        where TEntity : class;
}
