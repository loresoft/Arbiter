using System.Security.Claims;

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
public abstract class EntityQueryEndpointBase<TKey, TListModel, TReadModel> : IEndpointRoute
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
            .MapGet("page", GetPagedQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Get{EntityName}Page")
            .WithSummary("Get a page of entities")
            .WithDescription("Get a page of entities");

        group
            .MapPost("page", PostPagedQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Query{EntityName}Page")
            .WithSummary("Get a page of entities")
            .WithDescription("Get a page of entities");

        group
            .MapGet("", GetSelectQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Get{EntityName}List")
            .WithSummary("Get entities by query")
            .WithDescription("Get entities by query");

        group
            .MapPost("query", PostSelectQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Query{EntityName}List")
            .WithSummary("Get entities by query")
            .WithDescription("Get entities by query");

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
            Logger.LogError(ex, "Error GetQuery: {ErrorMessage}", ex.Message);

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
        [FromQuery] int? page = 1,
        [FromQuery] int? size = 20,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entityQuery = new EntityQuery(q, page ?? 1, size ?? 20, sort);
            var command = new EntityPagedQuery<TListModel>(user, entityQuery);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error GetPagedQuery: {ErrorMessage}", ex.Message);

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
            Logger.LogError(ex, "Error PostPagedQuery: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }

    }

    /// <summary>
    /// Retrieves a list of entities using query string parameters.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="q">The raw query expression.</param>
    /// <param name="sort">The sort expression.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="IReadOnlyCollection{TListModel}"/> with the result list or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
    protected virtual async Task<Results<Ok<IReadOnlyCollection<TListModel>>, ProblemHttpResult>> GetSelectQuery(
        [FromServices] IMediator mediator,
        [FromQuery] string? q = null,
        [FromQuery] string? sort = null,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entitySelect = new EntitySelect(q, sort);

            var command = new EntitySelectQuery<TListModel>(user, entitySelect);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error GetSelectQuery: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }

    }

    /// <summary>
    /// Retrieves a list of entities using a posted <see cref="EntitySelect"/> object.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="entitySelect">The entity select specifying filter and sort criteria.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="IReadOnlyCollection{TListModel}"/> with the result list or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
    protected virtual async Task<Results<Ok<IReadOnlyCollection<TListModel>>, ProblemHttpResult>> PostSelectQuery(
        [FromServices] IMediator mediator,
        [FromBody] EntitySelect entitySelect,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntitySelectQuery<TListModel>(user, entitySelect);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error PostSelectQuery: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }

    }
}
