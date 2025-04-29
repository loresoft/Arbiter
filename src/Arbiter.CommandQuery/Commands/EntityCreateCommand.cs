using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a command to create a new entity using the specified <typeparamref name="TCreateModel"/>.
/// The result of the command will be of type <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TCreateModel">The type of the create model used to provide data for the new entity.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the command.</typeparam>
/// <remarks>
/// This command is typically used in a CQRS (Command Query Responsibility Segregation) pattern to create a new entity
/// and return a read model representing the created entity or a related result.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityCreateCommand{TCreateModel, TReadModel}"/>:
/// <code>
/// var createModel = new ProductCreateModel
/// {
///     Name = "New Product",
///     Description = "A description of the new product",
///     Price = 19.99m
/// };
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
///
/// var command = new EntityCreateCommand&lt;ProductCreateModel, ProductReadModel&gt;(principal, createModel);
///
/// // Pass the command to the mediator for execution
/// var result = await mediator.Send(command);
/// Console.WriteLine($"Created product: {result?.Name}");
/// </code>
/// </example>
public record EntityCreateCommand<TCreateModel, TReadModel>
    : EntityModelCommand<TCreateModel, TReadModel>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCreateCommand{TCreateModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user for whom this command is executed.</param>
    /// <param name="model">The create model containing the data for the new entity.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <c>null</c>.</exception>
    public EntityCreateCommand(ClaimsPrincipal? principal, [NotNull] TCreateModel model)
        : base(principal, model)
    {
    }

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
