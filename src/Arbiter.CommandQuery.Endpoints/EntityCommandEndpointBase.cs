using System.Security.Claims;

using Arbiter.CommandQuery.Commands;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using SystemTextJsonPatch;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Provides a base class for defining RESTful command endpoints for an entity, including create, update, upsert, patch, and delete operations.
/// </summary>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
/// <typeparam name="TListModel">The type of the list model returned by queries.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned by single-entity queries and commands.</typeparam>
/// <typeparam name="TCreateModel">The type of the model used to create a new entity.</typeparam>
/// <typeparam name="TUpdateModel">The type of the model used to update or patch an entity.</typeparam>
/// <remarks>
/// This class extends <see cref="EntityQueryEndpointBase{TKey, TListModel, TReadModel}"/> to provide endpoints for entity command operations.
/// It is intended for use in applications to standardize CRUD API patterns.
/// </remarks>
public abstract partial class EntityCommandEndpointBase<TKey, TListModel, TReadModel, TCreateModel, TUpdateModel>
    : EntityQueryEndpointBase<TKey, TListModel, TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCommandEndpointBase{TKey, TListModel, TReadModel, TCreateModel, TUpdateModel}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> for this endpoint.</param>
    /// <param name="entityName">The name of the entity for this endpoint.</param>
    /// <param name="routePrefix">The route prefix for this endpoint. If not set, <paramref name="entityName"/> is used.</param>
    protected EntityCommandEndpointBase(ILoggerFactory loggerFactory, string entityName, string? routePrefix = null)
        : base(loggerFactory, entityName, routePrefix)
    {
    }

    /// <summary>
    /// Maps the command endpoints for the entity, including create, update, upsert, patch, and delete operations.
    /// </summary>
    /// <param name="group">The <see cref="RouteGroupBuilder"/> used to define the endpoint group.</param>
    /// <remarks>
    /// This method adds endpoints for:
    /// <list type="bullet">
    /// <item><description>GET {id}/update - Retrieve an entity for update</description></item>
    /// <item><description>POST - Create a new entity</description></item>
    /// <item><description>POST {id} - Upsert (create or update) an entity</description></item>
    /// <item><description>PUT {id} - Update an entity</description></item>
    /// <item><description>PATCH {id} - Patch an entity using a JSON patch document</description></item>
    /// <item><description>DELETE {id} - Delete an entity</description></item>
    /// </list>
    /// </remarks>
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
    /// Retrieves an entity for update by its identifier using the mediator service.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="Ok{TUpdateModel}"/> with the update model or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
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
            LogError(Logger, ex, nameof(GetUpdateQuery), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Creates a new entity using the provided create model and the mediator service.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="createModel">The model containing data for the new entity.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="Ok{TReadModel}"/> with the created entity or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
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
            LogError(Logger, ex, nameof(CreateCommand), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Updates an existing entity using the provided update model and the mediator service.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="updateModel">The model containing updated data for the entity.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="Ok{TReadModel}"/> with the updated entity or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
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
            LogError(Logger, ex, nameof(UpdateCommand), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Applies a JSON patch document to an existing entity using the mediator service.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="id">The identifier of the entity to patch.</param>
    /// <param name="jsonPatch">The JSON patch document describing the changes.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="Ok{TReadModel}"/> with the patched entity or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
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
            LogError(Logger, ex, nameof(PatchCommand), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Deletes an existing entity by its identifier using the mediator service.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> to send the request to.</param>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="user">The current security claims principal.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// An awaitable task returning either <see cref="Ok{TReadModel}"/> with the deleted entity or <see cref="ProblemHttpResult"/> on error.
    /// </returns>
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
            LogError(Logger, ex, nameof(DeleteCommand), ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    [LoggerMessage(LogLevel.Error, "Error {methodName}: {errorMessage}")]
    private static partial void LogError(ILogger logger, Exception exception, string methodName, string errorMessage);
}
