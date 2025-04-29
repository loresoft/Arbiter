using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a base command that uses a view model to perform an operation.
/// </summary>
/// <typeparam name="TEntityModel">The type of the model used as input for the command.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the command.</typeparam>
/// <remarks>
/// This class is typically used in a CQRS (Command Query Responsibility Segregation) pattern to define commands
/// that operate on an entity using a model and return a read model as the result.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityModelCommand{TEntityModel, TReadModel}"/>:
/// <code>
/// public record CreateProductCommand : EntityModelCommand&lt;ProductCreateModel, ProductReadModel&gt;
/// {
///     public CreateProductCommand(ClaimsPrincipal principal, ProductCreateModel model)
///         : base(principal, model)
///     {
///     }
/// }
///
/// var createModel = new ProductCreateModel
/// {
///     Name = "New Product",
///     Description = "A description of the new product",
///     Price = 19.99m
/// };
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new CreateProductCommand(principal, createModel);
///
/// // Pass the command to a handler or mediator
/// var result = await mediator.Send(command);
/// Console.WriteLine($"Created product: {result?.Name}");
/// </code>
/// </example>
public abstract record EntityModelCommand<TEntityModel, TReadModel>
    : PrincipalCommandBase<TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityModelCommand{TEntityModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    /// <param name="model">The model containing the data for the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <c>null</c>.</exception>
    protected EntityModelCommand(ClaimsPrincipal? principal, [NotNull] TEntityModel model)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(model);

        Model = model;
    }

    /// <summary>
    /// Gets the view model used for this command.
    /// </summary>
    /// <value>
    /// The view model containing the data for the operation.
    /// </value>
    [NotNull]
    [JsonPropertyName("model")]
    public TEntityModel Model { get; }
}
