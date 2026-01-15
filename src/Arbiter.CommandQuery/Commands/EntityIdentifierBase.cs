using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a base command for operations that require an identifier.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
/// <remarks>
/// This class is typically used in a CQRS (Command Query Responsibility Segregation) pattern to define commands
/// that operate on a specific entity identified by a key.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityIdentifierBase{TKey, TResponse}"/>:
/// <code>
/// public record GetEntityByIdCommand : EntityIdentifierCommand&lt;int, ProductReadModel&gt;
/// {
///     public GetEntityByIdCommand(ClaimsPrincipal principal, int id)
///         : base(principal, id)
///     {
///     }
/// }
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new GetEntityByIdCommand(principal, 123);
///
/// // Pass the command to a handler or mediator
/// var result = await mediator.Send(command);
/// Console.WriteLine($"Entity Name: {result?.Name}");
/// </code>
/// </example>
public abstract record EntityIdentifierBase<TKey, TResponse>
    : PrincipalCommandBase<TResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifierBase{TKey, TResponse}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    /// <param name="id">The identifier of the entity for this command.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
    protected EntityIdentifierBase(ClaimsPrincipal? principal, [NotNull] TKey id)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }

    /// <summary>
    /// Gets the identifier for this command.
    /// </summary>
    /// <value>
    /// The identifier of the entity for this command.
    /// </value>
    [NotNull]
    [JsonPropertyName("id")]
    public TKey Id { get; }
}
