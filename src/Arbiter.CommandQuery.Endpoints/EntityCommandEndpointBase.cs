using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Queries;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using SystemTextJsonPatch;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Base class for entity command endpoints.
/// </summary>
/// <typeparam name="TKey">The type of the key for entity</typeparam>
/// <typeparam name="TListModel">The type of the list model</typeparam>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
/// <typeparam name="TCreateModel">The type of the create model</typeparam>
/// <typeparam name="TUpdateModel">The type of the update model</typeparam>
public abstract class EntityCommandEndpointBase<TKey, TListModel, TReadModel, TCreateModel, TUpdateModel>
    : EntityQueryEndpointBase<TKey, TListModel, TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCommandEndpointBase{TKey, TListModel, TReadModel, TCreateModel, TUpdateModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this endpoint.</param>
    /// <param name="entityName">The name of the entity for this endpoint</param>
    /// <param name="routePrefix">The route prefix for this endpoint.  <paramref name="entityName"/> used if not set.</param>
    protected EntityCommandEndpointBase(ILoggerFactory loggerFactory, string entityName, string? routePrefix = null)
        : base(loggerFactory, entityName, routePrefix)
    {
    }

    /// <inheritdoc/>
    protected override void MapGroup(RouteGroupBuilder group)
    {
        base.MapGroup(group);

        group
            .MapGet("{id}/update", GetUpdateQuery)
            .WithEntityMetadata(EntityName)
            .WithName($"Get{EntityName}Update")
            .WithSummary("Get an entity for update by id")
            .WithDescription("Get an entity for update by id");

        group
            .MapPost("", CreateCommand)
            .WithEntityMetadata(EntityName)
            .WithName($"Create{EntityName}")
            .WithSummary("Create new entity")
            .WithDescription("Create new entity");

        group
            .MapPost("{id}", UpsertCommand)
            .WithEntityMetadata(EntityName)
            .WithName($"Upsert{EntityName}")
            .WithSummary("Create new or update entity")
            .WithDescription("Create new or update entity");

        group
            .MapPut("{id}", UpdateCommand)
            .WithEntityMetadata(EntityName)
            .WithName($"Update{EntityName}")
            .WithSummary("Update entity")
            .WithDescription("Update entity");

        group
            .MapPatch("{id}", PatchCommand)
            .WithEntityMetadata(EntityName)
            .WithName($"Patch{EntityName}")
            .WithSummary("Patch entity")
            .WithDescription("Patch entity");

        group
            .MapDelete("{id}", DeleteCommand)
            .WithEntityMetadata(EntityName)
            .WithName($"Delete{EntityName}")
            .WithSummary("Delete entity")
            .WithDescription("Delete entity");
    }

    /// <summary>
    /// Gets the result from the mediator service for a single entity by id.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="id">The identifier for the entity</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
    protected virtual async Task<Results<Ok<TUpdateModel>, ProblemHttpResult>> GetUpdateQuery(
        [FromServices] IMediator mediator,
        [FromRoute] TKey id,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityIdentifierQuery<TKey, TUpdateModel>(user, id);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error GetUpdateQuery: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Creates a new entity using the mediator service with the specified create model.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="createModel">The model being created</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
    protected virtual async Task<Results<Ok<TReadModel>, ProblemHttpResult>> CreateCommand(
        [FromServices] IMediator mediator,
        [FromBody] TCreateModel createModel,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityCreateCommand<TCreateModel, TReadModel>(user, createModel);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error CreateCommand: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Updates an existing entity using the mediator service with the specified update model.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="id">The identifier for the entity</param>
    /// <param name="updateModel">The update model to apply</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
    protected virtual async Task<Results<Ok<TReadModel>, ProblemHttpResult>> UpdateCommand(
        [FromServices] IMediator mediator,
        [FromRoute] TKey id,
        [FromBody] TUpdateModel updateModel,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityUpdateCommand<TKey, TUpdateModel, TReadModel>(user, id, updateModel);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error UpdateCommand: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Creates or updates an entity using the mediator service with the specified update model.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="id">The identifier for the entity</param>
    /// <param name="updateModel">The update model to apply</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
    protected virtual async Task<Results<Ok<TReadModel>, ProblemHttpResult>> UpsertCommand(
        [FromServices] IMediator mediator,
        [FromRoute] TKey id,
        [FromBody] TUpdateModel updateModel,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityUpsertCommand<TKey, TUpdateModel, TReadModel>(user, id, updateModel);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error UpsertCommand: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Patches an existing entity using the mediator service with the specified JSON patch document.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="id">The identifier for the entity</param>
    /// <param name="jsonPatch">The JSON patch document to apply</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
    protected virtual async Task<Results<Ok<TReadModel>, ProblemHttpResult>> PatchCommand(
        [FromServices] IMediator mediator,
        [FromRoute] TKey id,
        [FromBody] JsonPatchDocument jsonPatch,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityPatchCommand<TKey, TReadModel>(user, id, jsonPatch);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error PatchCommand: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Deletes an existing entity using the mediator service with the specified identifier.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="id">The identifier for the entity</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The request cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
    protected virtual async Task<Results<Ok<TReadModel>, ProblemHttpResult>> DeleteCommand(
        [FromServices] IMediator mediator,
        [FromRoute] TKey id,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EntityDeleteCommand<TKey, TReadModel>(user, id);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error DeleteCommand: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

}
