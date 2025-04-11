using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

public class TrackChangeCommandBehavior<TEntityModel, TResponse>
    : PipelineBehaviorBase<EntityModelCommand<TEntityModel, TResponse>, TResponse>
    where TEntityModel : class
{
    private readonly IPrincipalReader _principalReader;

    public TrackChangeCommandBehavior(ILoggerFactory loggerFactory, IPrincipalReader principalReader) : base(loggerFactory)
    {
        _principalReader = principalReader;
    }

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
