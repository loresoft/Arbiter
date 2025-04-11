using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

public class EntityIdentifierQueryHandler<TContext, TEntity, TKey, TReadModel>
    : EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, EntityIdentifierQuery<TKey, TReadModel>, TReadModel>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
{

    public EntityIdentifierQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }


    protected override async ValueTask<TReadModel?> Process(EntityIdentifierQuery<TKey, TReadModel> request, CancellationToken cancellationToken = default)
    {
        return await Read(request.Id, cancellationToken).ConfigureAwait(false);
    }
}
