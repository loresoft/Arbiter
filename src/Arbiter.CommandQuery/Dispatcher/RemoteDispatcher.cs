using System.Net.Http.Json;
using System.Text.Json;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Models;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Dispatcher;

/// <summary>
/// A dispatcher that sends requests to a remote service over HTTP. Use for Blazor Interactive WebAssembly rendering mode.
/// </summary>
public class RemoteDispatcher : IDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly DispatcherOptions _dispatcherOptions;
    private readonly HybridCache? _hybridCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteDispatcher"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to send request over</param>
    /// <param name="serializerOptions">The JSON options to use for serialization</param>
    /// <param name="dispatcherOptions">The dispatcher options to use</param>
    /// <param name="hybridCache">The optional hybrid cache to use.</param>
    public RemoteDispatcher(
        HttpClient httpClient,
        JsonSerializerOptions serializerOptions,
        IOptions<DispatcherOptions> dispatcherOptions,
        HybridCache? hybridCache = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(serializerOptions);
        ArgumentNullException.ThrowIfNull(dispatcherOptions);

        _httpClient = httpClient;
        _serializerOptions = serializerOptions;
        _dispatcherOptions = dispatcherOptions.Value;
        _hybridCache = hybridCache;
    }

    /// <inheritdoc />
    public ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        return Send(request, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
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

    private async ValueTask<TResponse?> SendCore<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestUri = _dispatcherOptions.FeaturePrefix
            .Combine(_dispatcherOptions.DispatcherPrefix)
            .Combine(_dispatcherOptions.SendRoute);

        var dispatchRequest = new DispatchRequest { Request = request };

        var responseMessage = await _httpClient
            .PostAsJsonAsync(
                requestUri: requestUri,
                value: dispatchRequest,
                options: _serializerOptions,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await EnsureSuccessStatusCode(responseMessage, cancellationToken).ConfigureAwait(false);

        using var stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        // no content, return null
        if (stream.Length == 0)
            return default;

        var response = await JsonSerializer.DeserializeAsync<TResponse>(stream, _serializerOptions, cancellationToken).ConfigureAwait(false);

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

        var message = $"Response status code does not indicate success: {responseMessage.StatusCode} ({responseMessage.ReasonPhrase}).";

        var mediaType = responseMessage.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase))
            throw new HttpRequestException(message, inner: null, responseMessage.StatusCode);

        var problemDetails = await responseMessage.Content
            .ReadFromJsonAsync<ProblemDetails>(
                options: _serializerOptions,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (problemDetails == null)
            throw new HttpRequestException(message, inner: null, responseMessage.StatusCode);

        var status = (System.Net.HttpStatusCode?)problemDetails.Status;
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
