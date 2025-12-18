using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;

using MessagePack;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a base command type that uses a specified <see cref="ClaimsPrincipal"/> to execute operations.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
/// <remarks>
/// This class is typically used in a CQRS (Command Query Responsibility Segregation) pattern to define commands
/// that require user context, such as authentication or authorization, provided by a <see cref="ClaimsPrincipal"/>.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="PrincipalCommandBase{TResponse}"/>:
/// <code>
/// public record GetUserDetailsCommand : PrincipalCommandBase&lt;UserDetails&gt;
/// {
///     public GetUserDetailsCommand(ClaimsPrincipal principal) : base(principal)
///     {
///     }
/// }
///
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var command = new GetUserDetailsCommand(principal);
///
/// // Pass the command to a handler or mediator
/// var result = await mediator.Send(command);
/// Console.WriteLine($"User Name: {result?.Name}");
/// </code>
/// </example>
public abstract record PrincipalCommandBase<TResponse> : IRequest<TResponse>, IRequestPrincipal
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrincipalCommandBase{TResponse}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    protected PrincipalCommandBase(ClaimsPrincipal? principal)
    {
        Principal = principal;

        Activated = DateTimeOffset.UtcNow;
        ActivatedBy = principal?.Identity?.Name ?? "system";
    }

    /// <summary>
    /// Gets the <see cref="ClaimsPrincipal"/> representing the user executing the command.
    /// </summary>
    /// <value>
    /// The <see cref="ClaimsPrincipal"/> representing the user executing the command.
    /// </value>
    [JsonIgnore]
    [IgnoreMember]
    public ClaimsPrincipal? Principal { get; private set; }

    /// <summary>
    /// Gets the timestamp indicating when this command was activated.
    /// </summary>
    /// <value>
    /// The timestamp indicating when this command was activated.
    /// </value>
    [JsonIgnore]
    [IgnoreMember]
    public DateTimeOffset Activated { get; private set; }

    /// <summary>
    /// Gets the user name of the individual who activated this command.
    /// Extracted from the specified <see cref="Principal"/>.
    /// </summary>
    /// <value>
    /// The user name of the individual who activated this command.
    /// </value>
    /// <remarks>
    /// If the <see cref="Principal"/> is <see langword="null"/>, the value defaults to "system".
    /// </remarks>
    /// <see cref="ClaimsIdentity.Name"/>
    [JsonIgnore]
    [IgnoreMember]
    public string? ActivatedBy { get; private set; }

    /// <summary>
    /// Applies the specified <see cref="ClaimsPrincipal"/> to the command.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the command.</param>
    void IRequestPrincipal.ApplyPrincipal(ClaimsPrincipal? principal)
    {
        Principal = principal;
        Activated = DateTimeOffset.UtcNow;
        ActivatedBy = principal?.Identity?.Name ?? "system";
    }
}
