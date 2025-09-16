using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Handlers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Handlers;

/// <summary>
/// A base handler for a request that requires the specified <see cref="DbContext"/>.
/// </summary>
/// <typeparam name="TContext">The type of <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public abstract class DataContextHandlerBase<TContext, TRequest, TResponse>
    : RequestHandlerBase<TRequest, TResponse>
    where TContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContextHandlerBase{TContext, TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this handler.</param>
    /// <param name="dataContext">The <see cref="DbContext"/> for this handler.</param>
    /// <param name="mapper"> The <see cref="IMapper"/> for this handler.</param>
    protected DataContextHandlerBase(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
        : base(loggerFactory)
    {
        DataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Gets the <see cref="DbContext"/> for this handler.
    /// </summary>
    protected TContext DataContext { get; }

    /// <summary>
    /// Gets the <see cref="IMapper"/> for this handler.
    /// </summary>
    protected IMapper Mapper { get; }
}
