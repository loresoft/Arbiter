using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;

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
public class RemoteDispatcher : IDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly HybridCache? _hybridCache;
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteDispatcher"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests to the remote endpoint.</param>
    /// <param name="options">The MessagePack serialization options.</param>
    /// <param name="hybridCache">Optional hybrid cache for caching responses. When provided, requests implementing <see cref="ICacheResult"/> will be cached.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> or <paramref name="options"/> is null.</exception>
    public RemoteDispatcher(
        HttpClient httpClient,
        MessagePackSerializerOptions options,
        HybridCache? hybridCache = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options;
        _hybridCache = hybridCache;
    }

    /// <summary>
    /// Sends a request to the remote dispatcher and returns the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    public ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        => Send(request, cancellationToken);

    /// <summary>
    /// Sends a request to the remote dispatcher and returns the response.
    /// If the request implements <see cref="ICacheResult"/> and caching is enabled, the response will be cached.
    /// If the request implements <see cref="ICacheExpire"/>, the cache will be invalidated by tag after sending.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails or returns a non-success status code.</exception>
    public async ValueTask<TResponse?> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // cache only if implements interface
        var cacheRequest = request as ICacheResult;
        if (_hybridCache is null || cacheRequest?.IsCacheable() != true)
            return await SendCore(request, cancellationToken).ConfigureAwait(false);

        var cacheKey = cacheRequest.GetCacheKey();
        var cacheTag = cacheRequest.GetCacheTag();
        var cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = cacheRequest.SlidingExpiration(),
        };

        return await _hybridCache
            .GetOrCreateAsync(
                key: cacheKey,
                factory: async token => await SendCore(request, token).ConfigureAwait(false),
                options: cacheOptions,
                tags: string.IsNullOrEmpty(cacheTag) ? null : [cacheTag],
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<TResponse?> SendCore<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        // Single serialization - directly serialize the request
        var requestType = request.GetType();
        var requestName = requestType.GetPortableName();

        var requestBytes = MessagePackSerializer.Serialize(requestType, request, _options, cancellationToken);

        using ByteArrayContent httpContent = new(requestBytes);
        httpContent.Headers.ContentType = DispatcherConstants.MessagePackMediaTypeHeader;

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, DispatcherConstants.DispatcherEndpoint);
        httpRequest.Content = httpContent;
        httpRequest.Version = HttpVersion.Version20;
        httpRequest.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        httpRequest.Headers.Add(DispatcherConstants.RequestTypeHeader, requestName);

        using var httpResponse = await _httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);

        await EnsureSuccessStatusCode(httpResponse, cancellationToken).ConfigureAwait(false);

        if (httpResponse.StatusCode == HttpStatusCode.NoContent)
            return default;

        var responseBytes = await httpResponse.Content
            .ReadAsByteArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (responseBytes.Length == 0)
            return default;

        var response = MessagePackSerializer.Deserialize<TResponse>(responseBytes, _options, cancellationToken);

        // expire cache
        if (_hybridCache is null || request is not ICacheExpire cacheRequest)
            return response;

        var cacheTag = cacheRequest.GetCacheTag();
        if (!string.IsNullOrEmpty(cacheTag))
            await _hybridCache.RemoveByTagAsync(cacheTag, cancellationToken).ConfigureAwait(false);

        return response;
    }


    private async ValueTask EnsureSuccessStatusCode(HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        if (responseMessage.IsSuccessStatusCode)
            return;

        await ThrowHttpExeption(responseMessage, cancellationToken).ConfigureAwait(false);
    }

    [DoesNotReturn]
    private async ValueTask ThrowHttpExeption(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        var message = $"Response status code does not indicate success: {responseMessage.StatusCode} ({responseMessage.ReasonPhrase}).";

        // try to get problem details
        await TryJsonProblem(responseMessage, message, cancellationToken).ConfigureAwait(false);

        // try messagepack problem details
        await TryMessagePackProblem(responseMessage, message, cancellationToken).ConfigureAwait(false);

        // fallback - throw generic exception
        throw new HttpRequestException(message, inner: null, responseMessage.StatusCode);
    }

    private async ValueTask TryMessagePackProblem(HttpResponseMessage responseMessage, string message, CancellationToken cancellationToken)
    {
        // check content type is messagepack
        var mediaType = responseMessage.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, DispatcherConstants.MessagePackContentType, StringComparison.OrdinalIgnoreCase))
            return;

        // make sure it's a problem details response
        responseMessage.Headers.TryGetValues(DispatcherConstants.ResponseTypeHeader, out var responseTypeValues);
        var responseType = responseTypeValues?.FirstOrDefault();
        if (string.IsNullOrEmpty(responseType) || !string.Equals(responseType, typeof(ProblemDetails).GetPortableName(), StringComparison.OrdinalIgnoreCase))
            return;

        // deserialize problem details
        var responseBytes = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var problemDetails = MessagePackSerializer.Deserialize<ProblemDetails>(responseBytes, _options, cancellationToken);

        // throw problem details
        if (problemDetails == null)
            throw new HttpRequestException(message, inner: null, responseMessage.StatusCode);

        ThrowProblemDetails(responseMessage, problemDetails);
    }

    private static async ValueTask TryJsonProblem(HttpResponseMessage responseMessage, string message, CancellationToken cancellationToken)
    {
        // check content type is problem details
        var mediaType = responseMessage.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, ProblemDetails.ContentType, StringComparison.OrdinalIgnoreCase))
            return;

        // deserialize problem details
        var problemDetails = await responseMessage.Content
            .ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // throw problem details
        if (problemDetails == null)
            throw new HttpRequestException(message, inner: null, responseMessage.StatusCode);

        ThrowProblemDetails(responseMessage, problemDetails);
    }

    [DoesNotReturn]
    private static void ThrowProblemDetails(HttpResponseMessage responseMessage, ProblemDetails problemDetails)
    {
        var status = (HttpStatusCode?)problemDetails.Status;
        status ??= responseMessage.StatusCode;

        var problemMessage = problemDetails.Title
            ?? responseMessage.ReasonPhrase
            ?? "Internal Server Error";

        if (!string.IsNullOrEmpty(problemDetails.Detail))
            problemMessage = $"{problemMessage} {problemDetails.Detail}";

        throw new HttpRequestException(
            message: problemMessage,
            inner: null,
            statusCode: status);
    }
}

