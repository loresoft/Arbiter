using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;
using Arbiter.Services;

using SystemTextJsonPatch;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a command to apply a JSON patch to an entity identified by a specific key.
/// The result of the command will be of type <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the command.</typeparam>
/// <remarks>
/// This command is typically used in a CQRS (Command Query Responsibility Segregation) pattern to apply partial updates
/// to an entity using a JSON patch document.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityPatchCommand{TKey, TReadModel}"/>:
/// <code>
/// var patchDocument = new JsonPatchDocument();
/// patchDocument.Replace("/Name", "Updated Name");
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new EntityPatchCommand&lt;int, ProductReadModel&gt;(principal, 123, patchDocument);
///
/// // Pass the command to a handler or mediator
/// var result = await mediator.Send(command);
/// Console.WriteLine($"Updated product name: {result?.Name}");
/// </code>
/// </example>
public record EntityPatchCommand<TKey, TReadModel>
    : EntityIdentifierBase<TKey, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPatchCommand{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    /// <param name="id">The identifier of the entity to which the JSON patch will be applied.</param>
    /// <param name="patch">The JSON patch document containing the updates to apply.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows for named query modification strategies to be applied.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> or <paramref name="patch"/> is <see langword="null"/>.</exception>
    public EntityPatchCommand(
        ClaimsPrincipal? principal,
        [NotNull] TKey id,
        [NotNull] JsonPatchDocument patch,
        string? filterName = null)
        : base(principal, id)
    {
        ArgumentNullException.ThrowIfNull(patch);

        Patch = patch;
        FilterName = filterName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPatchCommand{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="id">The identifier of the entity to which the JSON patch will be applied.</param>
    /// <param name="patch">The JSON patch document containing the updates to apply.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows different query modification strategies to be applied based on context.</param>
    /// <remarks>
    /// <para>
    /// If <paramref name="id"/> is <see langword="null"/>, an <see cref="ArgumentNullException"/> will be thrown.
    /// </para>
    /// </remarks>
    [JsonConstructor]
    public EntityPatchCommand(
        [NotNull] TKey id,
        [NotNull] JsonPatchDocument patch,
        string? filterName = null)
        : this(principal: null, id, patch, filterName: filterName)
    { }

    /// <summary>
    /// Gets the JSON patch document to apply to the entity with the specified identifier.
    /// </summary>
    /// <value>
    /// The JSON patch document containing the updates.
    /// </value>
    [JsonPropertyName("patch")]
    public JsonPatchDocument Patch { get; }

    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during query execution.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if no specific pipeline is specified.
    /// </value>
    /// <remarks>
    /// This property allows for named query modification strategies to be applied, enabling different filtering,
    /// security policies, or data transformations based on the execution context. The specific behavior depends
    /// on the registered query pipeline modifiers in the application.
    /// </remarks>
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
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/>.
    /// </summary>
    /// <returns>The cache tag for the <typeparamref name="TReadModel"/>, or <see langword="null"/> if no tag is available.</returns>
    string? ICacheExpire.GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
