using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Models;
using Arbiter.CommandQuery.Services;

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
    : EntityIdentifierCommand<TKey, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPatchCommand{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    /// <param name="id">The identifier of the entity to which the JSON patch will be applied.</param>
    /// <param name="patch">The JSON patch document containing the updates to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> or <paramref name="patch"/> is <see langword="null"/>.</exception>
    public EntityPatchCommand(ClaimsPrincipal? principal, [NotNull] TKey id, [NotNull] IReadOnlyList<JsonPatchOperation> patch)
        : base(principal, id)
    {
        ArgumentNullException.ThrowIfNull(patch);

        Patch = patch;
    }

    /// <summary>
    /// Gets the JSON patch document to apply to the entity with the specified identifier.
    /// </summary>
    /// <value>
    /// The JSON patch document containing the updates.
    /// </value>
    [JsonPropertyName("patch")]
    public IReadOnlyList<JsonPatchOperation> Patch { get; }

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/>.
    /// </summary>
    /// <returns>The cache tag for the <typeparamref name="TReadModel"/>, or <see langword="null"/> if no tag is available.</returns>
    string? ICacheExpire.GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
