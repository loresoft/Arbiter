using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Extensions;
using Arbiter.Mediation;

using MemoryPack;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.MemoryPack.Dispatcher;

/// <summary>
/// Dispatcher implementation using MemoryPack for binary serialization and HTTP for remote communication.
/// Supports optional caching via HybridCache for improved performance.
/// </summary>
/// <remarks>
/// Designed for scenarios requiring high-performance remote request/response over HTTP.
/// Caching is enabled if a HybridCache is provided and the request supports caching interfaces.
/// </remarks>
public partial class MemoryPackDispatcher : IDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly DispatcherOptions _dispatcherOptions;
    private readonly HybridCache? _hybridCache;
    private readonly ILogger<MemoryPackDispatcher> _logger;
    private readonly string _sendRoute;

    /// <summary>
    /// Initializes a new instance of <see cref="MemoryPackDispatcher"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and debugging.</param>
    /// <param name="httpClient">HTTP client for sending requests to remote endpoints.</param>
    /// <param name="dispatcherOptions">Configuration options for the dispatcher.</param>
    /// <param name="hybridCache">Optional cache for caching responses. If null, caching is disabled.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="dispatcherOptions"/> is null.</exception>
    public MemoryPackDispatcher(
        ILogger<MemoryPackDispatcher> logger,
        HttpClient httpClient,
        IOptions<DispatcherOptions> dispatcherOptions,
        HybridCache? hybridCache = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(dispatcherOptions);

        _logger = logger;
        _httpClient = httpClient;
        _dispatcherOptions = dispatcherOptions.Value;
        _hybridCache = hybridCache;
        _sendRoute = _dispatcherOptions.SendRoute + MemoryPackConstants.RouteSuffix;
    }

    /// <summary>
    /// Sends a strongly-typed request to the dispatcher and returns the response.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request object.</typeparam>
    /// <typeparam name="TResponse">Type of the expected response.</typeparam>
    /// <param name="request">Request object implementing <see cref="IRequest{TResponse}"/>.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Asynchronous operation returning the response.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        => Send(request, cancellationToken);

    /// <summary>
    /// Sends a request to the dispatcher and returns the response, with optional caching support.
    /// </summary>
    /// <typeparam name="TResponse">Type of the expected response.</typeparam>
    /// <param name="request">Request object implementing <see cref="IRequest{TResponse}"/>.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Asynchronous operation returning the response.</returns>
    /// <remarks>
    /// If the request implements <see cref="ICacheResult"/> and caching is enabled, the response is cached.
    /// If the request implements <see cref="ICacheExpire"/>, cache invalidation is performed after processing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the response type cannot be deserialized.</exception>
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

    /// <summary>
    /// Core logic for sending a request and deserializing the response.
    /// Handles cache expiration if applicable.
    /// </summary>
    /// <typeparam name="TResponse">Type of the expected response.</typeparam>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Deserialized response object, or default if response is empty.</returns>
    private async ValueTask<TResponse?> SendCore<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var requestData = MemoryPackSerializer.Serialize(requestType, request);

        var requestContent = new ByteArrayContent(requestData);
        requestContent.Headers.Add(MemoryPackConstants.DataTypeHeader, requestType.AssemblyQualifiedName);
        requestContent.Headers.ContentType = new(MemoryPackConstants.MemoryPackMediaType);

        var requestUri = _dispatcherOptions.FeaturePrefix
            .Combine(_dispatcherOptions.DispatcherPrefix)
            .Combine(_sendRoute);

        var httpResponse = await _httpClient
            .PostAsync(requestUri, requestContent, cancellationToken)
            .ConfigureAwait(false);

        httpResponse.EnsureSuccessStatusCode();

        if (httpResponse.Content.Headers.ContentLength == 0)
        {
            LogEmptyResponseContent(requestType.Name);
            return default;
        }

        var responseData = await httpResponse.Content
            .ReadAsByteArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (responseData.Length == 0)
        {
            LogEmptyResponseData(requestType.Name);
            return default;
        }

        var response = MemoryPackSerializer.Deserialize<TResponse?>(responseData);

        // expire cache
        if (_hybridCache is null || request is not ICacheExpire cacheRequest)
            return response;

        var cacheTag = cacheRequest.GetCacheTag();
        if (!string.IsNullOrEmpty(cacheTag))
            await _hybridCache.RemoveByTagAsync(cacheTag, cancellationToken).ConfigureAwait(false);

        return response;
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Received empty response content for type: {RequestType}")]
    private partial void LogEmptyResponseContent(string requestType);

    [LoggerMessage(EventId = 1002,Level = LogLevel.Debug,Message = "Received empty response data for type: {RequestType}")]
    private partial void LogEmptyResponseData(string requestType);
}
