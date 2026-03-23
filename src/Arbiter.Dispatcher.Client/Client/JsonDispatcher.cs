using System.Buffers;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Models;
using Arbiter.Mediation;

using Microsoft.Extensions.Caching.Hybrid;

namespace Arbiter.Dispatcher.Client;

/// <summary>
/// Implements a remote dispatcher that sends requests to a remote HTTP endpoint using JSON serialization.
/// Supports optional hybrid caching for cacheable requests.
/// </summary>
public class JsonDispatcher : RemoteDispatcherBase
{
    private static readonly MediaTypeHeaderValue _mediaTypeHeader = new(MediaTypeNames.Application.Json);
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDispatcher"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests to the remote endpoint.</param>
    /// <param name="options">The JSON serialization options.</param>
    /// <param name="hybridCache">Optional hybrid cache for caching responses. When provided, requests implementing <see cref="ICacheResult"/> will be cached.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> or <paramref name="options"/> is null.</exception>
    public JsonDispatcher(HttpClient httpClient, JsonSerializerOptions options, HybridCache? hybridCache = null)
        : base(httpClient, hybridCache)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc/>
    protected override MediaTypeHeaderValue ContentType { get; } = _mediaTypeHeader;

    /// <inheritdoc/>
    protected override void Serialize<TResponse>(
        IBufferWriter<byte> bufferWriter,
        IRequest<TResponse> request,
        Type requestType,
        CancellationToken cancellationToken)
    {
        using var writer = new Utf8JsonWriter(bufferWriter);
        JsonSerializer.Serialize(writer, request, requestType, _options);
    }

    /// <inheritdoc/>
    protected override TResponse? Deserialize<TResponse>(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken)
        where TResponse : default
    {
        return JsonSerializer.Deserialize<TResponse>(buffer.Span, _options);
    }

    /// <inheritdoc/>
    protected override async ValueTask<ProblemDetails?> TryProblemDetails(
        HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default)
    {
        // check content type is JSON
        var mediaType = responseMessage.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, MediaTypeNames.Application.ProblemJson, StringComparison.OrdinalIgnoreCase))
            return null;

        // deserialize problem details from buffer
        var responseBytes = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ProblemDetails>(responseBytes, _options);
    }
}
