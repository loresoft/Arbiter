using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A pipeline behavior that validates the entity model before processing the command.
/// </summary>
/// <typeparam name="TEntityModel">The type of the entity model to validate.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class ValidateEntityModelCommandBehavior<TEntityModel, TResponse>
    : PipelineBehaviorBase<EntityModelBase<TEntityModel, TResponse>, TResponse>
    where TEntityModel : class
{
    private readonly IValidator<TEntityModel>? _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateEntityModelCommandBehavior{TEntityModel, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from.</param>
    /// <param name="validator">The validator to use for the entity model.</param>
    public ValidateEntityModelCommandBehavior(ILoggerFactory loggerFactory, IValidator<TEntityModel>? validator = null)
        : base(loggerFactory)
    {
        _validator = validator;
    }

    /// <inheritdoc />
    protected override async ValueTask<TResponse?> Process(
        EntityModelBase<TEntityModel, TResponse> request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (_validator is null)
            return await next(cancellationToken).ConfigureAwait(false);

        // validate before processing
        var result = await _validator
            .Validate(request.Model, cancellationToken: cancellationToken)
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
