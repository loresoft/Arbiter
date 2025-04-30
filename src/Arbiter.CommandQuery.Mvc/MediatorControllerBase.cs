
using Arbiter.Mediation;

using Microsoft.AspNetCore.Mvc;

namespace Arbiter.CommandQuery.Mvc;

/// <summary>
/// Provides a base class for API controllers that use the <see cref="IMediator"/> to handle requests and responses.
/// </summary>
/// <remarks>
/// This class simplifies the implementation of controllers in a CQRS (Command Query Responsibility Segregation) pattern
/// by providing access to the <see cref="IMediator"/> for sending commands, queries, and notifications.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public abstract class MediatorControllerBase : ControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorControllerBase"/> class.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> used to send requests and handle responses.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediator"/> is <c>null</c>.</exception>
    protected MediatorControllerBase(IMediator mediator)
    {
        Mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets the <see cref="IMediator"/> used to send requests and handle responses.
    /// </summary>
    /// <value>
    /// The <see cref="IMediator"/> instance provided to the controller.
    /// </value>
    public IMediator Mediator { get; }
}
