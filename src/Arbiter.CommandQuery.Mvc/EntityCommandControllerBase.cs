// Ignore Spelling: Upsert json

using System.Net.Mime;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Queries;
using Arbiter.Mediation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SystemTextJsonPatch;

namespace Arbiter.CommandQuery.Mvc;

/// <summary>
/// Provides a base class for API controllers that handle entity commands, including create, update, delete, and patch operations.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify entities.</typeparam>
/// <typeparam name="TListModel">The type of the list model returned for queries.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned for individual entity queries.</typeparam>
/// <typeparam name="TCreateModel">The type of the model used to create new entities.</typeparam>
/// <typeparam name="TUpdateModel">The type of the model used to update existing entities.</typeparam>
/// <remarks>
/// This class simplifies the implementation of controllers in a CQRS (Command Query Responsibility Segregation) pattern
/// by providing common command operations such as create, update, delete, and patch for entities.
/// </remarks>
[Produces(MediaTypeNames.Application.Json)]
public abstract class EntityCommandControllerBase<TKey, TListModel, TReadModel, TCreateModel, TUpdateModel>
    : EntityQueryControllerBase<TKey, TListModel, TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCommandControllerBase{TKey, TListModel, TReadModel, TCreateModel, TUpdateModel}"/> class.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> used to send commands and handle responses.</param>
    protected EntityCommandControllerBase(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Retrieves the update model for an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve the update model for.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The update model for the entity if found; otherwise, <see langword="null"/>.</returns>
    [HttpGet("{id}/update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<TUpdateModel?>> GetUpdate(
        [FromRoute] TKey id,
        CancellationToken cancellationToken = default)
    {
        return await GetUpdateQuery(id, cancellationToken);
    }

    /// <summary>
    /// Creates a new entity using the specified create model.
    /// </summary>
    /// <param name="createModel">The model containing the data for the new entity.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the created entity.</returns>
    [HttpPost("")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<TReadModel?>> Create(
        [FromBody] TCreateModel createModel,
        CancellationToken cancellationToken = default)
    {
        return await CreateCommand(createModel, cancellationToken);
    }

    /// <summary>
    /// Creates or updates an entity using the specified update model.
    /// </summary>
    /// <param name="id">The identifier of the entity to create or update.</param>
    /// <param name="updateModel">The model containing the data for the entity.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the created or updated entity.</returns>
    [HttpPost("{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<TReadModel?>> Upsert(
        [FromRoute] TKey id,
        [FromBody] TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
    {
        return await UpsertCommand(id, updateModel, cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity using the specified update model.
    /// </summary>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="updateModel">The model containing the updated data for the entity.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the updated entity.</returns>
    [HttpPut("{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<TReadModel?>> Update(
        [FromRoute] TKey id,
        [FromBody] TUpdateModel updateModel,
        CancellationToken cancellationToken = default)
    {
        return await UpdateCommand(id, updateModel, cancellationToken);
    }

    /// <summary>
    /// Applies a JSON patch to an existing entity.
    /// </summary>
    /// <param name="id">The identifier of the entity to patch.</param>
    /// <param name="jsonPatch">The JSON patch document containing the changes to apply.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the patched entity.</returns>
    [HttpPatch("{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<TReadModel?>> Patch(
        [FromRoute] TKey id,
        [FromBody] JsonPatchDocument jsonPatch,
        CancellationToken cancellationToken = default)
    {
        return await PatchCommand(id, jsonPatch, cancellationToken);
    }

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the deleted entity.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<TReadModel?>> Delete(
        [FromRoute] TKey id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteCommand(id, cancellationToken);
    }

    /// <summary>
    /// Executes a query to retrieve the update model for an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve the update model for.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The update model for the entity if found; otherwise, <see langword="null"/>.</returns>
    protected virtual async Task<TUpdateModel?> GetUpdateQuery(TKey id, CancellationToken cancellationToken = default)
    {
        var command = new EntityIdentifierQuery<TKey, TUpdateModel>(User, id);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Executes a command to create a new entity using the specified create model.
    /// </summary>
    /// <param name="createModel">The model containing the data for the new entity.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the created entity.</returns>
    protected virtual async Task<TReadModel?> CreateCommand(TCreateModel createModel, CancellationToken cancellationToken = default)
    {
        var command = new EntityCreateCommand<TCreateModel, TReadModel>(User, createModel);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Executes a command to update an existing entity using the specified update model.
    /// </summary>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="updateModel">The model containing the updated data for the entity.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the updated entity.</returns>
    protected virtual async Task<TReadModel?> UpdateCommand(TKey id, TUpdateModel updateModel, CancellationToken cancellationToken = default)
    {
        var command = new EntityUpdateCommand<TKey, TUpdateModel, TReadModel>(User, id, updateModel);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Executes a command to create or update an entity using the specified update model.
    /// </summary>
    /// <param name="id">The identifier of the entity to create or update.</param>
    /// <param name="updateModel">The model containing the data for the entity.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the created or updated entity.</returns>
    protected virtual async Task<TReadModel?> UpsertCommand(TKey id, TUpdateModel updateModel, CancellationToken cancellationToken = default)
    {
        var command = new EntityUpsertCommand<TKey, TUpdateModel, TReadModel>(User, id, updateModel);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Executes a command to apply a JSON patch to an existing entity.
    /// </summary>
    /// <param name="id">The identifier of the entity to patch.</param>
    /// <param name="jsonPatch">The JSON patch document containing the changes to apply.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the patched entity.</returns>
    protected virtual async Task<TReadModel?> PatchCommand(TKey id, JsonPatchDocument jsonPatch, CancellationToken cancellationToken = default)
    {
        var command = new EntityPatchCommand<TKey, TReadModel>(User, id, jsonPatch);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Executes a command to delete an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The read model for the deleted entity.</returns>
    protected virtual async Task<TReadModel?> DeleteCommand(TKey id, CancellationToken cancellationToken = default)
    {
        var command = new EntityDeleteCommand<TKey, TReadModel>(User, id);
        return await Mediator.Send(command, cancellationToken);
    }
}
