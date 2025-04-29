using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a command to update an entity identified by a specific key using the provided update model.
/// The result of the command will be of type <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
/// <typeparam name="TUpdateModel">The type of the update model containing the data for the update.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the command.</typeparam>
/// <remarks>
/// This command is typically used in a CQRS (Command Query Responsibility Segregation) pattern to update an entity
/// and return a read model representing the updated entity or a related result.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}"/>:
/// <code>
/// var updateModel = new ProductUpdateModel
/// {
///     Name = "Updated Product",
///     Description = "Updated description of the product",
///     Price = 29.99m
/// };
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new EntityUpdateCommand&lt;int, ProductUpdateModel, ProductReadModel&gt;(principal, 123, updateModel);
///
/// // Pass the command to a handler or mediator
/// var result = await mediator.Send(command);
/// Console.WriteLine($"Updated product name: {result?.Name}");
/// </code>
/// </example>
public record EntityUpdateCommand<TKey, TUpdateModel, TReadModel>
    : EntityModelCommand<TUpdateModel, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateCommand{TKey, TUpdateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="model">The update model containing the data for the update.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> or <paramref name="model"/> is <c>null</c>.</exception>
    public EntityUpdateCommand(ClaimsPrincipal? principal, [NotNull] TKey id, TUpdateModel model) : base(principal, model)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }

    /// <summary>
    /// Gets the identifier of the entity to update.
    /// </summary>
    /// <value>
    /// The identifier of the entity to update.
    /// </value>
    [NotNull]
    [JsonPropertyName("id")]
    public TKey Id { get; }

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/>.
    /// </summary>
    /// <returns>The cache tag for the <typeparamref name="TReadModel"/>, or <c>null</c> if no tag is available.</returns>
    /// <example>
    /// The following example demonstrates how to retrieve the cache tag:
    /// <code>
    /// var cacheTag = command.GetCacheTag();
    /// </code>
    /// </example>
    string? ICacheExpire.GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
