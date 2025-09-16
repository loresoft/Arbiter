using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a base command for operations that require a list of identifiers.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entities.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
/// <remarks>
/// This class is typically used in a CQRS (Command Query Responsibility Segregation) pattern to define commands
/// that operate on multiple entities identified by a collection of keys.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityIdentifiersCommand{TKey, TResponse}"/>:
/// <code>
/// public record DeleteEntitiesCommand : EntityIdentifiersCommand&lt;int, ProductReadModel&gt;
/// {
///     public DeleteEntitiesCommand(ClaimsPrincipal principal, IReadOnlyCollection&lt;int&gt; ids)
///         : base(principal, ids)
///     {
///     }
/// }
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new DeleteEntitiesCommand(principal, new List&lt;int&gt; { 1, 2, 3 });
///
/// // Pass the command to a handler or mediator
/// var result = await mediator.Send(command);
/// Console.WriteLine($"Entities deleted: {result}");
/// </code>
/// </example>
public abstract record EntityIdentifiersCommand<TKey, TResponse>
    : PrincipalCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifiersCommand{TKey, TResponse}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    /// <param name="ids">The collection of identifiers for this command.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <see langword="null"/>.</exception>
    protected EntityIdentifiersCommand(ClaimsPrincipal? principal, [NotNull] IReadOnlyCollection<TKey> ids)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(ids);

        Ids = ids;
    }

    /// <summary>
    /// Gets the collection of identifiers for this command.
    /// </summary>
    /// <value>
    /// The collection of identifiers for this command.
    /// </value>
    [JsonPropertyName("ids")]
    public IReadOnlyCollection<TKey> Ids { get; }
}
