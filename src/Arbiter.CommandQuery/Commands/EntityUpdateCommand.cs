using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;
using Arbiter.Services;

using MessagePack;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a command to update an entity identified by a specific key using the provided update model.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entity to be updated.</typeparam>
/// <typeparam name="TUpdateModel">The type of the update model containing the data for the update operation.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned after the command execution, typically representing the updated entity.</typeparam>
/// <remarks>
/// <para>
/// This command is used in a CQRS (Command Query Responsibility Segregation) pattern to perform update operations
/// on entities. The command supports both standard update and upsert (update-or-insert) scenarios through the
/// <see cref="Upsert"/> property.
/// </para>
/// <para>
/// The command implements <see cref="ICacheExpire"/> to support automatic cache invalidation when an entity is updated,
/// ensuring cache consistency across the application. All cached queries for the entity type are invalidated when
/// an update occurs.
/// </para>
/// <para>
/// The <see cref="FilterName"/> property enables the use of named query pipelines during the update operation,
/// allowing different update strategies, validation rules, or security policies to be applied based on the execution context.
/// </para>
/// <para>
/// When <see cref="Upsert"/> is <see langword="true"/>, the command will create a new entity if one with the specified
/// identifier does not exist. This is particularly useful for idempotent operations where the same command can be
/// safely executed multiple times.
/// </para>
/// </remarks>
/// <example>
/// Example demonstrating a standard update operation:
/// <code>
/// var updateModel = new ProductUpdateModel
/// {
///     Name = "Updated Product",
///     Description = "Updated description of the product",
///     Price = 29.99m,
///     Category = "Electronics"
/// };
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new EntityUpdateCommand&lt;int, ProductUpdateModel, ProductReadModel&gt;(principal, 123, updateModel);
///
/// // Send the command through the mediator
/// var result = await mediator.Send(command);
///
/// if (result != null)
/// {
///     Console.WriteLine($"Updated product: {result.Name}");
///     Console.WriteLine($"New price: {result.Price:C}");
/// }
/// </code>
/// </example>
/// <example>
/// Example demonstrating an upsert operation with a named filter pipeline:
/// <code>
/// var updateModel = new ProductUpdateModel { Name = "New or Updated Product", Price = 49.99m };
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));
///
/// var command = new EntityUpdateCommand&lt;Guid, ProductUpdateModel, ProductReadModel&gt;(
///     principal,
///     productGuid,
///     updateModel,
///     upsert: true,
///     filterName: "admin-update");
///
/// // This will update the product if it exists, or create it if it doesn't
/// var result = await mediator.Send(command);
/// </code>
/// </example>
/// <seealso cref="EntityModelBase{TEntityModel, TReadModel}"/>
/// <seealso cref="ICacheExpire"/>
/// <seealso cref="EntityCreateCommand{TKey, TReadModel}"/>
/// <seealso cref="EntityPatchCommand{TKey, TReadModel}"/>
/// <seealso cref="EntityDeleteCommand{TKey, TReadModel}"/>
[MessagePackObject(true)]
public record EntityUpdateCommand<TKey, TUpdateModel, TReadModel>
    : EntityModelBase<TUpdateModel, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the update command. Used for authorization and audit logging.</param>
    /// <param name="id">The identifier of the entity to update. This value cannot be <see langword="null"/>.</param>
    /// <param name="model">The update model containing the data for the update operation. This value cannot be <see langword="null"/>.</param>
    /// <param name="upsert">
    /// <see langword="true"/> to insert a new entity if one with the specified identifier does not exist;
    /// <see langword="false"/> to only update existing entities. The default is <see langword="false"/>.
    /// </param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during the update operation. This allows different update strategies or security policies to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> or <paramref name="model"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="principal"/> parameter is used for authorization checks and audit trail purposes.
    /// If <see langword="null"/>, the operation is considered a system-level action.
    /// </para>
    /// <para>
    /// When <paramref name="upsert"/> is <see langword="true"/>, the behavior changes to an "update-or-insert" operation:
    /// <list type="bullet">
    /// <item><description>If an entity with the specified <paramref name="id"/> exists, it will be updated</description></item>
    /// <item><description>If no entity with the specified <paramref name="id"/> exists, a new one will be created</description></item>
    /// <item><description>This enables idempotent operations where repeated executions produce the same result</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The <paramref name="filterName"/> parameter enables named query pipeline scenarios during the update, such as:
    /// <list type="bullet">
    /// <item><description>Administrative updates that bypass certain validation rules</description></item>
    /// <item><description>Bulk update operations with optimized query strategies</description></item>
    /// <item><description>Audit-enhanced updates with additional logging and change tracking</description></item>
    /// <item><description>Different concurrency control strategies based on user roles or context</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public EntityUpdateCommand(
        ClaimsPrincipal? principal,
        [NotNull] TKey id,
        TUpdateModel model,
        bool upsert = false,
        string? filterName = null)
        : base(principal, model)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        Upsert = upsert;
        FilterName = filterName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="model">The update model containing the data for the update operation.</param>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="upsert">Whether to insert the entity if it does not exist.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during the update operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> or <paramref name="model"/> is <see langword="null"/>.</exception>
    [JsonConstructor]
    [SerializationConstructor]
    public EntityUpdateCommand(
        TUpdateModel model,
        [NotNull] TKey id,
        bool upsert = false,
        string? filterName = null)
        : this(principal: null, id, model, upsert, filterName)
    { }

    /// <summary>
    /// Gets the identifier of the entity to update.
    /// </summary>
    /// <value>
    /// The unique identifier of the entity to be updated. This value is guaranteed to be non-null after construction.
    /// </value>
    /// <remarks>
    /// This identifier is used to locate the specific entity instance to update. If <see cref="Upsert"/> is <see langword="true"/>
    /// and no entity with this identifier exists, a new entity will be created with this identifier.
    /// </remarks>
    [NotNull]
    [JsonPropertyName("id")]
    public TKey Id { get; }

    /// <summary>
    /// Gets a value indicating whether to insert the entity if it does not exist.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the entity should be inserted when it does not exist (upsert behavior);
    /// otherwise, <see langword="false"/> to only update existing entities.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see langword="true"/>, this command implements "upsert" (update-or-insert) semantics:
    /// <list type="bullet">
    /// <item><description>If the entity exists, it will be updated with the provided data</description></item>
    /// <item><description>If the entity does not exist, it will be created with the provided identifier and data</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When <see langword="false"/> (the default), the command will only succeed if the entity already exists.
    /// If the entity is not found, the handler typically returns <see langword="null"/> or throws an exception,
    /// depending on the implementation.
    /// </para>
    /// <para>
    /// Upsert operations are particularly useful in scenarios such as:
    /// <list type="bullet">
    /// <item><description>Synchronization operations where data may or may not already exist</description></item>
    /// <item><description>Idempotent API endpoints that can be safely retried</description></item>
    /// <item><description>Batch import operations where records might already exist</description></item>
    /// <item><description>Configuration updates where defaults should be created if not present</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard update (fails if entity doesn't exist)
    /// var updateCommand = new EntityUpdateCommand&lt;int, ProductUpdateModel, ProductReadModel&gt;(
    ///     principal, productId, updateModel, upsert: false);
    ///
    /// // Upsert (creates if entity doesn't exist)
    /// var upsertCommand = new EntityUpdateCommand&lt;int, ProductUpdateModel, ProductReadModel&gt;(
    ///     principal, productId, updateModel, upsert: true);
    /// </code>
    /// </example>
    [JsonPropertyName("upsert")]
    public bool Upsert { get; }

    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during the update operation.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if the default update strategy should be used.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property allows for named query modification strategies to be applied during entity updates,
    /// enabling different filtering, validation rules, security policies, or data transformations based on the execution context.
    /// </para>
    /// <para>
    /// The specific behavior depends on the registered query pipeline modifiers in the application.
    /// Common scenarios include applying different validation strategies, concurrency control mechanisms,
    /// or authorization policies based on the pipeline name.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the default update pipeline (if any) will be applied.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using a named pipeline for administrative bulk updates
    /// var command = new EntityUpdateCommand&lt;int, ProductUpdateModel, ProductReadModel&gt;(
    ///     adminPrincipal,
    ///     productId,
    ///     updateModel,
    ///     upsert: false,
    ///     filterName: "bulk-update");
    /// </code>
    /// </example>
    [JsonPropertyName("filterName")]
    public string? FilterName { get; }

    /// <summary>
    /// Gets the cache key associated with the <typeparamref name="TReadModel"/> entity type for cache invalidation.
    /// </summary>
    /// <returns>
    /// The cache key for the entity type, or <see langword="null"/> if no cache key is available.
    /// </returns>
    string? ICacheExpire.GetCacheKey()
        => CacheTagger.GetKey<TReadModel, TKey>(CacheTagger.Buckets.Identifier, Id);

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/> entity type for cache invalidation.
    /// </summary>
    /// <returns>
    /// The cache tag for the entity type, or <see langword="null"/> if no cache tag is available.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called automatically by the caching infrastructure to invalidate cached queries
    /// when an entity is updated. All cached queries tagged with the returned value will be invalidated,
    /// ensuring that subsequent queries reflect the update.
    /// </para>
    /// <para>
    /// The cache tag is generated using the <see cref="CacheTagger"/> utility based on the
    /// <typeparamref name="TReadModel"/> type, allowing all queries for that entity type (including
    /// identifier queries, paged queries, batch queries, etc.) to be invalidated together when any instance is updated.
    /// </para>
    /// <para>
    /// When <see cref="Upsert"/> is <see langword="true"/> and a new entity is created, the same cache
    /// invalidation occurs to ensure consistency across all query types.
    /// </para>
    /// </remarks>
    string? ICacheExpire.GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
