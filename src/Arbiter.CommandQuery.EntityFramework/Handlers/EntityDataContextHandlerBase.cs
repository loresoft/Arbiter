using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

public abstract class EntityDataContextHandlerBase<TContext, TEntity, TKey, TReadModel, TRequest, TResponse>
    : DataContextHandlerBase<TContext, TRequest, TResponse>
    where TContext : DbContext
    where TEntity : class, IHaveIdentifier<TKey>, new()
    where TRequest : IRequest<TResponse>
{
    protected EntityDataContextHandlerBase(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory, dataContext, mapper)
    {
    }

    protected virtual async ValueTask<TReadModel?> Read([NotNull] TKey key, CancellationToken cancellationToken = default)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        var query = DataContext
            .Set<TEntity>()
            .AsNoTracking()
            .TagWith($"EntityDataContextHandlerBase; Context:{typeof(TContext).Name}, Entity:{typeof(TEntity).Name}, Model:{typeof(TReadModel).Name}")
            .TagWithCallSite()
            .Where(p => Equals(p.Id, key));

        var projected = Mapper.ProjectTo<TEntity, TReadModel>(query);

        return await projected
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
