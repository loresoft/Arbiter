using System.Net.Mime;
using System.Text.Json;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;
using Arbiter.Mediation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.Mvc;

/// <summary>
/// Provides a base controller for exporting entities as CSV files.
/// </summary>
/// <typeparam name="TKey">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TListModel">The type of the list model used for entity representation, which must implement <see cref="ISupportWriter{T}"/>.</typeparam>
/// <typeparam name="TReadModel">The type of the read model used for detailed entity representation.</typeparam>
/// <remarks>
/// This controller includes functionality for exporting entities based on query parameters or a query
/// object. Derived classes can extend or override the provided export methods to customize behavior.
/// It inherits from <see cref="EntityQueryControllerBase{TKey, TListModel, TReadModel}"/> and adds CSV export capabilities.
/// </remarks>
[Produces(MediaTypeNames.Application.Json)]
public abstract class EntityExportQueryControllerBase<TKey, TListModel, TReadModel>
    : EntityQueryControllerBase<TKey, TListModel, TReadModel>
    where TListModel : ISupportWriter<TListModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityExportQueryControllerBase{TKey, TListModel, TReadModel}"/> class with the specified mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance used to handle requests and notifications.</param>
    protected EntityExportQueryControllerBase(IMediator mediator) : base(mediator)
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
