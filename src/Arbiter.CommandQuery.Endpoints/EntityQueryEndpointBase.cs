using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Queries;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Provides a base class for defining RESTful query endpoints for an entity, including single, paged, and list queries.
/// </summary>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
/// <typeparam name="TListModel">The type of the list model returned by queries.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned by single-entity queries.</typeparam>
/// <remarks>
/// This class is intended for use in Blazor and WebAssembly applications to standardize entity query API patterns.
/// It supports mapping endpoints for retrieving entities by ID, paged results, and filtered lists.
/// </remarks>
public abstract partial class EntityQueryEndpointBase<TKey, TListModel, TReadModel> : IEndpointRoute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQueryEndpointBase{TKey, TListModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this endpoint.</param>
    /// <param name="entityName">The name of the entity for this endpoint.</param>
    /// <param name="routePrefix">The route prefix for this endpoint. If not set, <paramref name="entityName"/> is used.</param>
    protected EntityQueryEndpointBase(ILoggerFactory loggerFactory, string entityName, string? routePrefix = null)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentException.ThrowIfNullOrEmpty(entityName);

        Logger = loggerFactory.CreateLogger(GetType());

        EntityName = entityName;
        RoutePrefix = routePrefix ?? EntityName;
    }

    /// <summary>
    /// Gets the name of the entity for this endpoint.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the route prefix for this endpoint.
    /// </summary>
    public string RoutePrefix { get; }

    /// <summary>
    /// Gets the logger for this endpoint.
    /// </summary>
    protected ILogger Logger { get; }

    /// <inheritdoc/>
    public void AddRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(RoutePrefix);

        MapGroup(group);
    }

    /// <summary>
    /// Maps the group of query endpoints for this entity, including single, paged, and list queries.
    /// </summary>
    /// <param name="group">The <see cref="RouteGroupBuilder"/> used to define the endpoint group.</param>
    /// <remarks>
    /// This method adds endpoints for:
    /// <list type="bullet">
    /// <item><description>GET {id} - Retrieve an entity by ID</description></item>
    /// <item><description>GET page - Retrieve a paged result of entities</description></item>
    /// <item><description>POST page - Retrieve a paged result of entities using a query object</description></item>
    /// <item><description>GET (root) - Retrieve a list of entities by query</description></item>
    /// <item><description>POST query - Retrieve a list of entities using a select object</description></item>
    /// </list>
    /// </remarks>
    protected virtual void MapGroup(RouteGroupBuilder group)
    {
        group
            .MapGet("{id}", GetQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Get{EntityName}")
            .WithSummary("Get an entity by id")
            .WithDescription("Get an entity by id");

        group
            .MapGet("", GetPagedQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Get{EntityName}Page")
            .WithSummary("Get a page of entities")
            .WithDescription("Get a page of entities");

        group
            .MapPost("query", PostPagedQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Query{EntityName}Page")
            .WithSummary("Get a page of entities")
            .WithDescription("Get a page of entities");
    }

    /// <summary>
    /// Retrieves a single entity by its identifier using the mediator service.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="Ok{TReadModel}"/> with the entity or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
    protected virtual async Task<Results<Ok<TReadModel>, ProblemHttpResult>> GetQuery(
        [FromServices] IMediator mediator,
        [FromRoute] TKey id,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityIdentifierQuery<TKey, TReadModel>(user, id);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            LogError(Logger, ex, nameof(GetQuery), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Retrieves a paged result of entities using query string parameters.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="q">The raw query expression.</param>
    /// <param name="sort">The sort expression.</param>
    /// <param name="page">The page number for the query. The default is 1.</param>
    /// <param name="size">The size of the page for the query. The default is 20.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="EntityPagedResult{TListModel}"/> with the paged result or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
    protected virtual async Task<Results<Ok<EntityPagedResult<TListModel>>, ProblemHttpResult>> GetPagedQuery(
        [FromServices] IMediator mediator,
        [FromQuery] string? q = null,
        [FromQuery] string? sort = null,
        [FromQuery] int? page = null,
        [FromQuery] int? size = null,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entityQuery = new EntityQuery { Query = q, Page = page, PageSize = size, };
            entityQuery.AddSort(sort);

            var command = new EntityPagedQuery<TListModel>(user, entityQuery);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            LogError(Logger, ex, nameof(GetPagedQuery), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Retrieves a paged result of entities using a posted <see cref="EntityQuery"/> object.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="entityQuery">The entity query specifying filter, sort, and pagination.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="EntityPagedResult{TListModel}"/> with the paged result or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
    protected virtual async Task<Results<Ok<EntityPagedResult<TListModel>>, ProblemHttpResult>> PostPagedQuery(
        [FromServices] IMediator mediator,
        [FromBody] EntityQuery entityQuery,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityPagedQuery<TListModel>(user, entityQuery);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            LogError(Logger, ex, nameof(PostPagedQuery), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    [LoggerMessage(LogLevel.Error, "Error {methodName}: {errorMessage}")]
    private static partial void LogError(ILogger logger, Exception exception, string methodName, string errorMessage);
}
