using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Models;
using Arbiter.Mediation;

using Microsoft.Extensions.Caching.Hybrid;

namespace Arbiter.Dispatcher.Client;

/// <summary>
/// Provides an abstract base class for remote dispatchers that send requests to a remote HTTP endpoint.
/// Supports optional hybrid caching for cacheable requests implementing <see cref="ICacheResult"/>.
/// </summary>
public abstract class RemoteDispatcherBase : IDispatcher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteDispatcherBase"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests to the remote endpoint.</param>
    /// <param name="hybridCache">Optional hybrid cache for caching responses. When provided, requests implementing <see cref="ICacheResult"/> will be cached.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> is null.</exception>
    protected RemoteDispatcherBase(HttpClient httpClient, HybridCache? hybridCache = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        HttpClient = httpClient;
        HybridCache = hybridCache;
    }

    /// <summary>
    /// Gets the HTTP client used to send requests to the remote endpoint.
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the optional hybrid cache for caching responses.
    /// </summary>
    protected HybridCache? HybridCache { get; }

    /// <summary>
    /// Gets the content type header value used for serialization.
    /// </summary>
    protected abstract MediaTypeHeaderValue ContentType { get; }

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
    public async ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if request supports caching via ICacheResult interface
        var cacheRequest = request as ICacheResult;

        // Bypass cache if caching is disabled or request is not cacheable
        if (HybridCache is null || cacheRequest?.IsCacheable() != true)
            return await SendCore(request, cancellationToken).ConfigureAwait(false);

        // Get cache configuration from request
        var cacheKey = cacheRequest.GetCacheKey();
        var cacheTag = cacheRequest.GetCacheTag();
        var cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = cacheRequest.SlidingExpiration(),
        };

        // Get cached result or execute request and cache the response
        return await HybridCache
            .GetOrCreateAsync(
                key: cacheKey,
                factory: async token => await SendCore(request, token).ConfigureAwait(false),
                options: cacheOptions,
                tags: string.IsNullOrEmpty(cacheTag) ? null : [cacheTag],
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Core implementation that sends a request to the remote dispatcher and returns the response.
    /// Handles serialization, HTTP communication, deserialization, and cache expiration.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation. The task result contains the response, or <c>null</c> if the server returns no content.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails or returns a non-success status code.</exception>
    protected virtual async ValueTask<TResponse?> SendCore<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        // Get portable type name for request identification
        var requestType = request.GetType();
        var requestName = requestType.GetPortableName();

        // Serialize request to byte buffer
        var bufferWriter = new ArrayBufferWriter<byte>();
        Serialize(bufferWriter, request, requestType, cancellationToken);

        // Create HTTP content from serialized buffer
        using var httpContent = new ReadOnlyMemoryContent(bufferWriter.WrittenMemory);
        httpContent.Headers.ContentType = ContentType;

        // Build HTTP request with HTTP/2 preference and request type header
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, DispatcherConstants.DispatcherEndpoint);
        httpRequest.Content = httpContent;
        httpRequest.Version = HttpVersion.Version20;
        httpRequest.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        httpRequest.Headers.Add(DispatcherConstants.RequestTypeHeader, requestName);

        // Send request and start reading response headers immediately
        using var httpResponse = await HttpClient
            .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        // Validate response status code
        await EnsureSuccessStatusCode(httpResponse, cancellationToken).ConfigureAwait(false);

        // Handle NoContent response (204)
        if (httpResponse.StatusCode == HttpStatusCode.NoContent)
            return default;

        // Read response content
        var responseBytes = await httpResponse.Content
            .ReadAsByteArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        // Handle empty response body
        if (responseBytes.Length == 0)
            return default;

        // Deserialize response
        var response = Deserialize<TResponse>(responseBytes, cancellationToken);

        // Invalidate cache entries if request implements ICacheExpire
        await ExpireCache(request, cancellationToken).ConfigureAwait(false);

        return response;
    }

    /// <summary>
    /// Serializes the request to a buffer using the derived class's serialization format.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="bufferWriter">The buffer writer to write the serialized request to.</param>
    /// <param name="request">The request to serialize.</param>
    /// <param name="requestType">The type of the request for proper serialization.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    protected abstract void Serialize<TResponse>(
        IBufferWriter<byte> bufferWriter,
        IRequest<TResponse> request,
        Type requestType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes the response from a buffer using the derived class's deserialization format.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="buffer">The buffer containing the serialized response.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The deserialized response.</returns>
    protected abstract TResponse? Deserialize<TResponse>(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken);

    /// <summary>
    /// Expires cached entries by tag if the request implements <see cref="ICacheExpire"/> and hybrid cache is available.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request that may trigger cache expiration.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task ExpireCache<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (HybridCache is null || request is not ICacheExpire cacheRequest)
            return;

        var cacheTag = cacheRequest.GetCacheTag();
        if (string.IsNullOrEmpty(cacheTag))
            return;

        await HybridCache.RemoveByTagAsync(cacheTag, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Ensures the HTTP response indicates success. If not, throws an exception with problem details if available.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message to check.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">Thrown when the response status code indicates failure.</exception>
    protected async ValueTask EnsureSuccessStatusCode(
        HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default)
    {
        if (responseMessage.IsSuccessStatusCode)
            return;

        await ThrowHttpException(responseMessage, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Throws an HTTP exception with details from the response, including problem details if available.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message containing error information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>This method does not return as it always throws an exception.</returns>
    /// <exception cref="HttpRequestException">Always thrown with details from the response.</exception>
    private async ValueTask ThrowHttpException(
        HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default)
    {
        var message = $"Response status code does not indicate success: {responseMessage.StatusCode} ({responseMessage.ReasonPhrase}).";

        // try to get problem details
        var problemDetails = await TryProblemDetails(responseMessage, cancellationToken).ConfigureAwait(false);
        if (problemDetails != null)
            ThrowProblemDetails(responseMessage, problemDetails);

        // fallback - throw generic exception
        throw new HttpRequestException(message, inner: null, responseMessage.StatusCode);
    }

    /// <summary>
    /// Attempts to deserialize problem details from the HTTP response if the content type and response type match expectations.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message to extract problem details from.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> containing the problem details if successful; otherwise, <c>null</c>.</returns>
    protected abstract ValueTask<ProblemDetails?> TryProblemDetails(
        HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws an HTTP exception with details extracted from problem details.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <param name="problemDetails">The problem details containing error information.</param>
    /// <exception cref="HttpRequestException">Always thrown with details from the problem details.</exception>
    [DoesNotReturn]
    private static void ThrowProblemDetails(
        HttpResponseMessage responseMessage,
        ProblemDetails problemDetails)
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
