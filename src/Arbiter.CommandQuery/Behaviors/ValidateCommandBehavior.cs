using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// Pipeline behavior that validates a command request before passing it to the next handler.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class ValidateCommandBehavior<TRequest, TResponse>
    : PipelineBehaviorBase<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IValidator<TRequest>? _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateCommandBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="validator">The validator to use for the request. If null, validation is skipped.</param>
    public ValidateCommandBehavior(
        ILoggerFactory loggerFactory,
        IValidator<TRequest>? validator = null)
        : base(loggerFactory)
    {
        _validator = validator;
    }

    /// <summary>
    /// Processes the request by validating it before invoking the next handler in the pipeline.
    /// Throws a <see cref="DomainException"/> if validation fails.
    /// </summary>
    /// <param name="request">The incoming request to validate and process.</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Awaitable task returning the <typeparamref name="TResponse"/> if validation succeeds; otherwise, throws an exception.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> or <paramref name="next"/> is null.</exception>
    /// <exception cref="DomainException">Thrown if validation fails.</exception>
    protected override async ValueTask<TResponse?> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (_validator is null)
            return await next(cancellationToken).ConfigureAwait(false);

        // validate before processing
        var result = await _validator
            .Validate(request, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (result.IsValid)
            return await next(cancellationToken).ConfigureAwait(false);

        throw new DomainException(
            statusCode: System.Net.HttpStatusCode.BadRequest,
            message: $"Validation Error: {result}")
        {
            Errors = result.Errors,
        };
    }
}
