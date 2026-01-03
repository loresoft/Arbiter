using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;
using Arbiter.Services;

using MessagePack;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a command to create a new entity using the specified create model.
/// </summary>
/// <typeparam name="TCreateModel">The type of the create model used to provide data for the new entity.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the command, typically representing the created entity.</typeparam>
/// <remarks>
/// <para>
/// This command is used in a CQRS (Command Query Responsibility Segregation) pattern to perform create operations
/// on entities. The command uses a dedicated create model (<typeparamref name="TCreateModel"/>) to encapsulate
/// the data required for entity creation, and returns a read model (<typeparamref name="TReadModel"/>) representing
/// the newly created entity.
/// </para>
/// <para>
/// The command implements <see cref="ICacheExpire"/> to support automatic cache invalidation when an entity is created,
/// ensuring cache consistency across the application. All cached queries for the entity type are invalidated when
/// a new entity is created.
/// </para>
/// <para>
/// The <see cref="FilterName"/> property enables the use of named query pipelines during the creation operation,
/// allowing different creation strategies, validation rules, or data enrichment policies to be applied based on the execution context.
/// </para>
/// <para>
/// By separating create models from read models, this command supports:
/// <list type="bullet">
/// <item><description>Different validation rules and required fields for creation vs. updates</description></item>
/// <item><description>Prevention of over-posting attacks by controlling which properties can be set during creation</description></item>
/// <item><description>Automatic generation of system fields (timestamps, identifiers) not present in the create model</description></item>
/// <item><description>Cleaner API contracts with input models optimized for creation scenarios</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Example demonstrating a standard entity creation:
/// <code>
/// var createModel = new ProductCreateModel
/// {
///     Name = "New Product",
///     Description = "A description of the new product",
///     Price = 19.99m,
///     Category = "Electronics"
/// };
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
///
/// var command = new EntityCreateCommand&lt;ProductCreateModel, ProductReadModel&gt;(principal, createModel);
///
/// // Send the command through the mediator
/// var result = await mediator.Send(command);
///
/// if (result != null)
/// {
///     Console.WriteLine($"Created product with ID: {result.Id}");
///     Console.WriteLine($"Product Name: {result.Name}");
/// }
/// </code>
/// </example>
/// <example>
/// Example using a named filter pipeline for bulk import creation:
/// <code>
/// var createModel = new ProductCreateModel { Name = "Imported Product", Price = 99.99m };
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));
///
/// var command = new EntityCreateCommand&lt;ProductCreateModel, ProductReadModel&gt;(
///     principal,
///     createModel,
///     filterName: "bulk-import");
///
/// // This might skip certain validation rules or apply different default values
/// var result = await mediator.Send(command);
/// </code>
/// </example>
/// <seealso cref="EntityModelBase{TEntityModel, TReadModel}"/>
/// <seealso cref="ICacheExpire"/>
/// <seealso cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}"/>
/// <seealso cref="EntityDeleteCommand{TKey, TReadModel}"/>
[MessagePackObject]
public partial record EntityCreateCommand<TCreateModel, TReadModel>
    : EntityModelBase<TCreateModel, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateCommand{TCreateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the create command. Used for authorization and audit logging.</param>
    /// <param name="model">The create model containing the data for the new entity. This value cannot be <see langword="null"/>.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during the creation operation. This allows different creation strategies or validation rules to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="principal"/> parameter is used for authorization checks and audit trail purposes.
    /// If <see langword="null"/>, the operation is considered a system-level action.
    /// </para>
    /// <para>
    /// The <paramref name="model"/> parameter contains all the data required to create the new entity.
    /// The create model typically excludes system-generated fields such as identifiers, timestamps, and audit fields,
    /// which are automatically populated by the handler.
    /// </para>
    /// <para>
    /// The <paramref name="filterName"/> parameter enables named query pipeline scenarios during creation, such as:
    /// <list type="bullet">
    /// <item><description>Bulk import operations with relaxed validation rules</description></item>
    /// <item><description>Administrative creation that allows setting additional system fields</description></item>
    /// <item><description>Different default value strategies based on the creation context</description></item>
    /// <item><description>Enhanced audit logging for sensitive entity creation</description></item>
    /// <item><description>Data enrichment pipelines that augment the create model with additional information</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public EntityCreateCommand(
        ClaimsPrincipal? principal,
        [NotNull] TCreateModel model,
        string? filterName = null)
        : base(principal, model)
    {
        FilterName = filterName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateCommand{TCreateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="model">The create model containing the data for the new entity. This value cannot be <see langword="null"/>.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during the creation operation. This allows different creation strategies or validation rules to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
    [JsonConstructor]
    [SerializationConstructor]
    public EntityCreateCommand(
        [NotNull] TCreateModel model,
        string? filterName = null)
        : this(principal: null, model, filterName)
    { }


    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during the creation operation.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if the default creation strategy should be used.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property allows for named query modification strategies to be applied during entity creation,
    /// enabling different validation rules, data enrichment, security policies, or default value assignments
    /// based on the execution context.
    /// </para>
    /// <para>
    /// The specific behavior depends on the registered query pipeline modifiers in the application.
    /// Common scenarios include:
    /// <list type="bullet">
    /// <item><description>Applying different validation strategies for bulk imports vs. manual creation</description></item>
    /// <item><description>Enabling administrative creation that bypasses certain business rules</description></item>
    /// <item><description>Adding automatic data enrichment (e.g., geocoding addresses, validating data against external services)</description></item>
    /// <item><description>Applying different default values based on the creation context or user role</description></item>
    /// <item><description>Enhanced audit logging or workflow triggering for sensitive entity types</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the default creation pipeline (if any) will be applied.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using a named pipeline for bulk import with relaxed validation
    /// var command = new EntityCreateCommand&lt;ProductCreateModel, ProductReadModel&gt;(
    ///     systemPrincipal,
    ///     createModel,
    ///     filterName: "bulk-import");
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
    /// when a new entity is created. All cached queries tagged with the returned value will be invalidated,
    /// ensuring that subsequent queries reflect the new entity.
    /// </para>
    /// <para>
    /// The cache tag is generated using the <see cref="CacheTagger"/> utility based on the
    /// <typeparamref name="TReadModel"/> type, allowing all queries for that entity type (including
    /// identifier queries, paged queries, batch queries, etc.) to be invalidated together when any new instance is created.
    /// </para>
    /// <para>
    /// This ensures cache consistency across the application, as queries that previously returned a complete
    /// list or filtered subset of entities will be refreshed to include the newly created entity.
    /// </para>
    /// </remarks>
    string? ICacheExpire.GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
