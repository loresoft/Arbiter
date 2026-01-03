using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;
using Arbiter.Services;

using MessagePack;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a command to delete an entity identified by a specific key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entity to be deleted.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned after the command execution, typically representing the deleted entity or operation result.</typeparam>
/// <remarks>
/// <para>
/// This command is used in a CQRS (Command Query Responsibility Segregation) pattern to perform delete operations
/// on entities. The command supports both hard and soft delete scenarios, depending on the handler implementation.
/// </para>
/// <para>
/// The command implements <see cref="ICacheExpire"/> to support automatic cache invalidation when an entity is deleted,
/// ensuring cache consistency across the application.
/// </para>
/// <para>
/// The <see cref="FilterName"/> property enables the use of named query pipelines during the delete operation,
/// allowing different deletion strategies or security policies to be applied based on the execution context.
/// </para>
/// </remarks>
/// <example>
/// Example demonstrating a simple delete operation:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new EntityDeleteCommand&lt;int, ProductReadModel&gt;(principal, 123);
///
/// // Send the command through the mediator
/// var result = await mediator.Send(command);
///
/// if (result != null)
/// {
///     Console.WriteLine($"Deleted product: {result.Name}");
/// }
/// </code>
/// </example>
/// <example>
/// Example using a named filter pipeline for administrative deletion:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));
/// var command = new EntityDeleteCommand&lt;int, ProductReadModel&gt;(
///     principal,
///     456,
///     filterName: "admin-delete");
///
/// // This might bypass certain protections or perform additional audit logging
/// var result = await mediator.Send(command);
/// </code>
/// </example>
/// <seealso cref="EntityIdentifierBase{TKey, TResponse}"/>
/// <seealso cref="ICacheExpire"/>
/// <seealso cref="EntityCreateCommand{TKey, TReadModel}"/>
/// <seealso cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}"/>
[MessagePackObject]
public partial record EntityDeleteCommand<TKey, TReadModel>
    : EntityIdentifierBase<TKey, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDeleteCommand{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the delete command. Used for authorization and audit logging.</param>
    /// <param name="id">The identifier of the entity to be deleted. This value cannot be <see langword="null"/>.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during the delete operation. This allows different deletion strategies or security policies to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="principal"/> parameter is used for authorization checks and audit trail purposes.
    /// If <see langword="null"/>, the operation is considered a system-level action.
    /// </para>
    /// <para>
    /// The <paramref name="filterName"/> parameter enables named query pipeline scenarios, such as:
    /// <list type="bullet">
    /// <item><description>Administrative deletion that bypasses certain protections</description></item>
    /// <item><description>Soft delete vs. hard delete strategies</description></item>
    /// <item><description>Cascading delete with specific rules</description></item>
    /// <item><description>Audit-enhanced deletion with additional logging</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public EntityDeleteCommand(
        ClaimsPrincipal? principal,
        [NotNull] TKey id,
        string? filterName = null) : base(principal, id)
    {
        FilterName = filterName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDeleteCommand{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="id">The identifier of the entity to be deleted. This value cannot be <see langword="null"/>.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during the delete operation. This allows different deletion strategies or security policies to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
    [JsonConstructor]
    [SerializationConstructor]
    public EntityDeleteCommand(
        [NotNull] TKey id,
        string? filterName = null)
        : this(principal: null, id, filterName)
    { }

    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during the delete operation.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if the default deletion strategy should be used.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property allows for named query modification strategies to be applied during entity deletion,
    /// enabling different filtering, security policies, or data transformations based on the execution context.
    /// </para>
    /// <para>
    /// The specific behavior depends on the registered query pipeline modifiers in the application.
    /// Common scenarios include applying different soft delete strategies, cascading delete rules,
    /// or authorization policies based on the pipeline name.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the default deletion pipeline (if any) will be applied.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using a named pipeline for administrative hard delete
    /// var command = new EntityDeleteCommand&lt;int, ProductReadModel&gt;(
    ///     adminPrincipal,
    ///     productId,
    ///     filterName: "hard-delete");
    /// </code>
    /// </example>
    [Key(1)]
    [JsonPropertyName("filterName")]
    public string? FilterName { get; }

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/> entity type for cache invalidation.
    /// </summary>
    /// <returns>
    /// The cache tag for the entity type, or <see langword="null"/> if no cache tag is available.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called automatically by the caching infrastructure to invalidate cached queries
    /// when an entity is deleted. All cached queries tagged with the returned value will be invalidated,
    /// ensuring that subsequent queries reflect the deletion.
    /// </para>
    /// <para>
    /// The cache tag is generated using the <see cref="CacheTagger"/> utility based on the
    /// <typeparamref name="TReadModel"/> type, allowing all queries for that entity type to be
    /// invalidated together when any instance is deleted.
    /// </para>
    /// </remarks>
    string? ICacheExpire.GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
