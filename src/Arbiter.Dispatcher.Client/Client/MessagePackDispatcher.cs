using System.Buffers;
using System.Net.Http.Headers;

using Arbiter.CommandQuery;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Models;
using Arbiter.Mediation;

using MessagePack;

using Microsoft.Extensions.Caching.Hybrid;

namespace Arbiter.Dispatcher.Client;

/// <summary>
/// Implements a remote dispatcher that sends requests to a remote HTTP endpoint using MessagePack serialization.
/// Supports optional hybrid caching for cacheable requests.
/// </summary>
public class MessagePackDispatcher : RemoteDispatcherBase
{
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackDispatcher"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests to the remote endpoint.</param>
    /// <param name="options">The MessagePack serialization options.</param>
    /// <param name="hybridCache">Optional hybrid cache for caching responses. When provided, requests implementing <see cref="ICacheResult"/> will be cached.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> or <paramref name="options"/> is null.</exception>
    public MessagePackDispatcher(
        HttpClient httpClient,
        MessagePackSerializerOptions options,
        HybridCache? hybridCache = null) : base(httpClient, hybridCache)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc/>
    protected override MediaTypeHeaderValue ContentType { get; } = DispatcherConstants.MessagePackMediaTypeHeader;

    /// <inheritdoc/>
    protected override void Serialize<TResponse>(IBufferWriter<byte> bufferWriter, IRequest<TResponse> request, Type requestType, CancellationToken cancellationToken)
    {
        MessagePackSerializer.Serialize(requestType, bufferWriter, request, _options);
    }

    /// <inheritdoc/>
    protected override TResponse? Deserialize<TResponse>(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        where TResponse : default
    {
        return MessagePackSerializer.Deserialize<TResponse>(buffer, _options, cancellationToken);
    }

    /// <inheritdoc/>
    protected override async ValueTask<ProblemDetails?> TryProblemDetails(HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        // check content type is MessagePack
        var mediaType = responseMessage.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, MessagePackDefaults.MessagePackContentType, StringComparison.OrdinalIgnoreCase))
            return null;

        // make sure it's a problem details response
        responseMessage.Headers.TryGetValues(DispatcherConstants.ResponseTypeHeader, out var responseTypeValues);
        var responseType = responseTypeValues?.FirstOrDefault();
        if (string.IsNullOrEmpty(responseType) || !string.Equals(responseType, typeof(ProblemDetails).GetPortableName(), StringComparison.OrdinalIgnoreCase))
            return null;

        // deserialize problem details from buffer
        var responseBytes = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        return MessagePackSerializer.Deserialize<ProblemDetails>(responseBytes, _options, cancellationToken);
    }
}
