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
/// Base class for entity query endpoints.
/// </summary>
/// <typeparam name="TKey">The type of the key for entity</typeparam>
/// <typeparam name="TListModel">The type of the list model</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public abstract class EntityQueryEndpointBase<TKey, TListModel, TReadModel> : IFeatureEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQueryEndpointBase{TKey, TListModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this endpoint.</param>
    /// <param name="entityName">The name of the entity for this endpoint</param>
    /// <param name="routePrefix">The route prefix for this endpoint.  <paramref name="entityName"/> used if not set.</param>
    protected EntityQueryEndpointBase(ILoggerFactory loggerFactory, string entityName, string? routePrefix = null)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentException.ThrowIfNullOrEmpty(entityName);

        Logger = loggerFactory.CreateLogger(GetType());

        EntityName = entityName;
        RoutePrefix = routePrefix ?? EntityName;
    }

    /// <summary>
    /// The name of the entity for this endpoint.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// The route prefix for this endpoint.
    /// </summary>
    public string RoutePrefix { get; }

    /// <summary>
    /// The logger for this endpoint.
    /// </summary>
    protected ILogger Logger { get; }


    /// <inheritdoc/>
    public void AddRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(RoutePrefix);

        MapGroup(group);
    }

    /// <summary>
    /// Maps the group of endpoints for this entity.
    /// </summary>
    /// <param name="group">Group of endpoints to map</param>
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
    /// Gets the result from the mediator service for a single entity by id.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="id">The identifier for the entity</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
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
    /// Gets the result from the mediator service for a page of entities.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="q">The raw query expression</param>
    /// <param name="sort">The sort expression</param>
    /// <param name="page">The page number for the query</param>
    /// <param name="size">The size of the page for the query</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
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
    /// Gets the result from the mediator service for a page of entities based on specified entity query.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="entityQuery">The entity query for selecting entities with a filter, sort and pagination</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
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
    /// Gets the result from the mediator service for a list of entities.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="q">The raw query expression</param>
    /// <param name="sort">The sort expression</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
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
    /// Gets the result from the mediator service for a list of entities based on specified entity select.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="entitySelect">The entity select for selecting entities with a filter and sort</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
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
