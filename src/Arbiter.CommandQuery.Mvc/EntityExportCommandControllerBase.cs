using System.Net.Mime;
using System.Text.Json;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;

using Foundatio.Mediator;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.Mvc;

/// <summary>
/// Provides a base class for API controllers that handle entity commands and exports, including create, update, delete, and patch operations, with CSV export functionality.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify entities.</typeparam>
/// <typeparam name="TListModel">The type of the list model returned for queries.  Must implement <see cref="ISupportWriter{TListModel}"/> for CSV export.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned for individual entity queries.</typeparam>
/// <typeparam name="TCreateModel">The type of the model used to create new entities.</typeparam>
/// <typeparam name="TUpdateModel">The type of the model used to update existing entities.</typeparam>
/// <remarks>
/// This class extends <see cref="EntityCommandControllerBase{TKey, TListModel, TReadModel, TCreateModel, TUpdateModel}"/> and provides CSV export capabilities for entities.
/// It simplifies the implementation of controllers in a CQRS pattern by providing common command operations and export functionality.
/// </remarks>
[Produces(MediaTypeNames.Application.Json)]
public abstract class EntityExportCommandControllerBase<TKey, TListModel, TReadModel, TCreateModel, TUpdateModel>
    : EntityCommandControllerBase<TKey, TListModel, TReadModel, TCreateModel, TUpdateModel>
    where TListModel : ISupportWriter<TListModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityExportCommandControllerBase{TKey, TListModel, TReadModel, TCreateModel, TUpdateModel}"/> class.
    /// </summary>
    /// <param name="mediator">The <see cref="IMediator"/> used to send queries and handle responses.</param>
    protected EntityExportCommandControllerBase(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Exports a list of entities as a CSV file based on the specified query.
    /// </summary>
    /// <param name="query">The query containing filtering and sorting criteria.</param>
    /// <param name="fileName">The suggested download file name.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A CSV file containing the exported entities.</returns>
    [HttpPost("export")]
    [Produces(MediaTypeNames.Text.Csv)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<ActionResult> Export(
        [FromBody] EntitySelect query,
        [FromQuery] string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        var results = await SelectQuery(query, cancellationToken);
        results ??= [];

        await using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);

        await CsvWriter.WriteAsync(streamWriter, results, cancellationToken);

        await streamWriter.FlushAsync(cancellationToken);

        var buffer = memoryStream.ToArray();

        return File(buffer, "text/csv", fileName);
    }

    /// <summary>
    /// Exports a list of entities as a CSV file based on query parameters.
    /// </summary>
    /// <param name="encodedQuery">The encoded query string containing filtering and sorting criteria.</param>
    /// <param name="fileName">The suggested download file name.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A CSV file containing the exported entities.</returns>
    [HttpGet("export")]
    [Produces(MediaTypeNames.Text.Csv)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<ActionResult> Export(
        [FromQuery] string? encodedQuery = null,
        [FromQuery] string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        var jsonSerializerOptions = HttpContext.RequestServices.GetService<JsonSerializerOptions>()
            ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var query = QueryStringEncoder.Decode<EntitySelect>(encodedQuery, jsonSerializerOptions) ?? new EntitySelect();

        var results = await SelectQuery(query, cancellationToken);
        results ??= [];

        await using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);

        var csvContent = await CsvWriter.WriteAsync(results, cancellationToken);

        await CsvWriter.WriteAsync(streamWriter, results, cancellationToken);

        await streamWriter.FlushAsync(cancellationToken);

        var buffer = memoryStream.ToArray();

        return File(buffer, "text/csv", fileName);
    }
}
