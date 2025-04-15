using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for tracking the changes of a model.
/// </summary>
/// <typeparam name="TEntityModel">The type of the model</typeparam>
/// <typeparam name="TResponse">The type of the response</typeparam>
public class TrackChangeCommandBehavior<TEntityModel, TResponse>
    : PipelineBehaviorBase<EntityModelCommand<TEntityModel, TResponse>, TResponse>
    where TEntityModel : class
{
    private readonly IPrincipalReader _principalReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackChangeCommandBehavior{TEntityModel, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory"> The logger factory to create an <see cref="ILogger"/> from</param>
    /// <param name="principalReader">The claims principal reader service.</param>
    public TrackChangeCommandBehavior(ILoggerFactory loggerFactory, IPrincipalReader principalReader) : base(loggerFactory)
    {
        _principalReader = principalReader ?? throw new ArgumentNullException(nameof(principalReader));
    }

    /// <inheritdoc />
    protected override async ValueTask<TResponse?> Process(
        EntityModelCommand<TEntityModel, TResponse> request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        TrackChange(request);

        // continue pipeline
        return await next(cancellationToken).ConfigureAwait(false);
    }

    private void TrackChange(EntityModelCommand<TEntityModel, TResponse> request)
    {
        var identityName = _principalReader.GetIdentifier(request.Principal);
        var model = request.Model;

        if (model is ITrackCreated createdModel)
        {
            if (createdModel.Created == default)
                createdModel.Created = DateTimeOffset.UtcNow;

            if (string.IsNullOrEmpty(createdModel.CreatedBy))
                createdModel.CreatedBy = identityName;
        }

        if (model is ITrackUpdated updatedModel)
        {
            updatedModel.Updated = DateTimeOffset.UtcNow;
            updatedModel.UpdatedBy = identityName;
        }
    }
}
