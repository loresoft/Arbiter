using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Notifications;

using Microsoft.EntityFrameworkCore;

namespace Arbiter.CommandQuery.EntityFramework.Pipeline;

/// <summary>
/// Defines a modifier that can change an entity, or related data, before a command saves it.
/// </summary>
/// <typeparam name="TEntity">The type of entity being saved.</typeparam>
/// <remarks>
/// <para>
/// Entity modifiers are the save side counterpart of <see cref="IQueryModifier{TEntity}"/>. They are invoked by the
/// Entity Framework create, update, patch, and delete command handlers after the entity has been mapped and audit
/// metadata applied, but before changes are saved. Any changes made by a modifier, including new related entities
/// added to the <see cref="DbContext"/>, are saved in the same transaction as the entity.
/// </para>
/// <para>
/// Modifiers are executed in priority order, with lower priority numbers executing first. Modifiers can be
/// registered as standard services (applied when no pipeline name is specified) or as keyed services
/// (applied only to the named pipeline matching the command filter name).
/// </para>
/// <example>
/// Example of a modifier that adds the creating user as the owner of a new entity:
/// <code>
/// public class ProjectOwnerModifier : IEntityModifier&lt;Project&gt;
/// {
///     public int Priority =&gt; 10;
///
///     public ValueTask Apply(
///         Project entity,
///         DbContext context,
///         EntityChangeOperation operation,
///         IRequestPrincipal? principal = null,
///         CancellationToken cancellationToken = default)
///     {
///         if (operation != EntityChangeOperation.Created)
///             return ValueTask.CompletedTask;
///
///         var userId = principal?.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
///         if (userId is not null)
///             entity.Members.Add(new ProjectMember { UserId = int.Parse(userId), IsOwner = true });
///
///         return ValueTask.CompletedTask;
///     }
/// }
/// </code>
/// </example>
/// <example>
/// Registering a standard entity modifier (applied when no pipeline name is specified):
/// <code>
/// services.AddQueryPipeline();
/// services.AddScoped&lt;IEntityModifier&lt;Project&gt;, ProjectOwnerModifier&gt;();
/// </code>
/// </example>
/// <example>
/// Registering a named entity modifier (applied only when the command filter name matches):
/// <code>
/// services.AddKeyedScoped&lt;IEntityModifier&lt;Project&gt;, ProjectOwnerModifier&gt;("admin-pipeline");
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="IQueryPipeline"/>
/// <seealso cref="IQueryModifier{TEntity}"/>
public interface IEntityModifier<TEntity> where TEntity : class
{
    /// <summary>
    /// Applies modifications to the specified entity before it is saved.
    /// </summary>
    /// <param name="entity">The entity being saved.</param>
    /// <param name="context">The database context the entity is being saved with.</param>
    /// <param name="operation">The change operation being performed on the entity.</param>
    /// <param name="principal">The request principal representing the current user and activation information, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Apply(
        TEntity entity,
        DbContext context,
        EntityChangeOperation operation,
        IRequestPrincipal? principal = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the priority of this modifier. Lower numbers execute first.
    /// </summary>
    /// <value>The priority value where lower numbers indicate higher priority.</value>
    int Priority { get; }
}
