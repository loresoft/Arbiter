using System.Globalization;
using System.Net.Mime;
using System.Text.Json;

using CsvHelper;
using CsvHelper.Configuration;

using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Arbiter.Mediation;

namespace Arbiter.CommandQuery.Mvc;

/// <summary>
/// Provides a base class for API controllers that handle entity queries, including filtering, sorting, pagination, and exporting data.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify entities.</typeparam>
/// <typeparam name="TListModel">The type of the list model returned for queries.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned for individual entity queries.</typeparam>
/// <remarks>
/// This class simplifies the implementation of controllers in a CQRS (Command Query Responsibility Segregation) pattern
/// by providing common query operations such as retrieving, paging, and exporting entities.
/// </remarks>
[Produces(MediaTypeNames.Application.Json)]
public abstract class EntityQueryControllerBase<TKey, TListModel, TReadModel> : MediatorControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQueryControllerBase{TKey, TListModel, TReadModel}"/> class.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> used to send queries and handle responses.</param>
    protected EntityQueryControllerBase(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The entity read model if found; otherwise, <c>null</c>.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<TReadModel?>> Get(
        [FromRoute] TKey id,
        CancellationToken cancellationToken = default)
    {
        return await GetQuery(id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a paged list of entities based on the specified query.
    /// </summary>
    /// <param name="query">The query containing filtering, sorting, and pagination criteria.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A paged result containing the list of entities.</returns>
    [HttpPost("page")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<EntityPagedResult<TListModel>?>> Page(
        [FromBody] EntityQuery query,
        CancellationToken cancellationToken = default)
    {
        return await PagedQuery(query, cancellationToken);
    }

    /// <summary>
    /// Retrieves a paged list of entities based on query parameters.
    /// </summary>
    /// <param name="q">The raw query expression.</param>
    /// <param name="sort">The sort expression.</param>
    /// <param name="page">The page number for the query.</param>
    /// <param name="size">The size of the page for the query.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A paged result containing the list of entities.</returns>
    [HttpGet("page")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<EntityPagedResult<TListModel>?>> Page(
        [FromQuery] string? q = null,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new EntityQuery(q, page, size, sort);
        return await PagedQuery(query, cancellationToken);
    }

    /// <summary>
    /// Retrieves a list of entities based on the specified query.
    /// </summary>
    /// <param name="query">The query containing filtering and sorting criteria.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A list of entities matching the query.</returns>
    [HttpPost("query")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<IReadOnlyCollection<TListModel>?>> Query(
        [FromBody] EntitySelect query,
        CancellationToken cancellationToken = default)
    {
        var results = await SelectQuery(query, cancellationToken);
        return results?.ToList();
    }

    /// <summary>
    /// Retrieves a list of entities based on query parameters.
    /// </summary>
    /// <param name="q">The raw query expression.</param>
    /// <param name="sort">The sort expression.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A list of entities matching the query.</returns>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public virtual async Task<ActionResult<IReadOnlyCollection<TListModel>?>> Query(
        [FromQuery] string? q = null,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var query = new EntitySelect(q, sort);
        var results = await SelectQuery(query, cancellationToken);
        return results?.ToList();
    }

    /// <summary>
    /// Exports a list of entities as a CSV file based on the specified query.
    /// </summary>
    /// <param name="query">The query containing filtering and sorting criteria.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A CSV file containing the exported entities.</returns>
    [HttpPost("export")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<ActionResult> Export(
        [FromBody] EntitySelect query,
        CancellationToken cancellationToken = default)
    {
        var results = await SelectQuery(query, cancellationToken);
        results ??= [];

        var csvConfiguration = HttpContext.RequestServices.GetService<CsvConfiguration>()
            ?? new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        await using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await using var csvWriter = new CsvWriter(streamWriter, csvConfiguration);

        WriteExportData(csvWriter, results);
        await streamWriter.FlushAsync(cancellationToken);

        var buffer = memoryStream.ToArray();

        return File(buffer, "text/csv");
    }

    /// <summary>
    /// Exports a list of entities as a CSV file based on query parameters.
    /// </summary>
    /// <param name="encodedQuery">The encoded query string containing filtering and sorting criteria.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A CSV file containing the exported entities.</returns>
    [HttpGet("export")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<ActionResult> Export(
        [FromQuery] string? encodedQuery = null,
        CancellationToken cancellationToken = default)
    {
        var jsonSerializerOptions = HttpContext.RequestServices.GetService<JsonSerializerOptions>()
            ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var query = QueryStringEncoder.Decode<EntitySelect>(encodedQuery, jsonSerializerOptions) ?? new EntitySelect();

        var results = await SelectQuery(query, cancellationToken);
        results ??= [];

        var csvConfiguration = HttpContext.RequestServices.GetService<CsvConfiguration>()
            ?? new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        await using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await using var csvWriter = new CsvWriter(streamWriter, csvConfiguration);

        WriteExportData(csvWriter, results);
        await streamWriter.FlushAsync();

        var buffer = memoryStream.ToArray();

        return File(buffer, "text/csv");
    }

    /// <summary>
    /// Executes a query to retrieve a single entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The entity read model if found; otherwise, <c>null</c>.</returns>
    protected virtual async Task<TReadModel?> GetQuery(TKey id, CancellationToken cancellationToken = default)
    {
        var command = new EntityIdentifierQuery<TKey, TReadModel>(User, id);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Executes a paged query to retrieve a list of entities.
    /// </summary>
    /// <param name="entityQuery">The query containing filtering, sorting, and pagination criteria.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A paged result containing the list of entities.</returns>
    protected virtual async Task<EntityPagedResult<TListModel>?> PagedQuery(EntityQuery entityQuery, CancellationToken cancellationToken = default)
    {
        var command = new EntityPagedQuery<TListModel>(User, entityQuery);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Executes a query to retrieve a list of entities based on filtering and sorting criteria.
    /// </summary>
    /// <param name="entitySelect">The query containing filtering and sorting criteria.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A list of entities matching the query.</returns>
    protected virtual async Task<IReadOnlyCollection<TListModel>?> SelectQuery(EntitySelect entitySelect, CancellationToken cancellationToken = default)
    {
        var command = new EntitySelectQuery<TListModel>(User, entitySelect);
        return await Mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Writes the exported data to a CSV writer.
    /// </summary>
    /// <param name="csvWriter">The CSV writer to write the data to.</param>
    /// <param name="results">The list of entities to export.</param>
    protected virtual void WriteExportData(CsvWriter csvWriter, IReadOnlyCollection<TListModel> results)
    {
        csvWriter.WriteRecords(results);
    }
}
