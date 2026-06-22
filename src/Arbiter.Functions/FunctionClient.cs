using System.Net.Http.Json;


namespace Arbiter.Functions;

/// <summary>
/// Provides access to Azure Functions host management endpoints.
/// </summary>
public class FunctionClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to call the Functions host admin API.</param>
    public FunctionClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        HttpClient = httpClient;
    }

    /// <summary>
    /// Gets the HTTP client used to call the Functions host admin API.
    /// </summary>
    public HttpClient HttpClient { get; }


    /// <summary>
    /// Gets host runtime status information.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that returns the host status, or <see langword="null"/> when no status is returned.</returns>
    public Task<FunctionStatus?> CheckHostStatus(CancellationToken cancellationToken = default)
    {
        return HttpClient.GetFromJsonAsync(
            requestUri: "admin/host/status/",
            jsonTypeInfo: FunctionContext.Default.FunctionStatus,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets all function definitions available on the host.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that returns the function definitions, or <see langword="null"/> when no data is returned.</returns>
    public Task<IReadOnlyList<FunctionDefinition>?> GetFunctions(CancellationToken cancellationToken = default)
    {
        return HttpClient.GetFromJsonAsync(
            requestUri: "admin/functions",
            jsonTypeInfo: FunctionContext.Default.IReadOnlyListFunctionDefinition,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the definition for a specific function.
    /// </summary>
    /// <param name="functionName">The function name.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that returns the function definition, or <see langword="null"/> when no data is returned.</returns>
    public Task<FunctionDefinition?> GetDefinition(string functionName, CancellationToken cancellationToken = default)
    {
        return HttpClient.GetFromJsonAsync(
            requestUri: $"admin/functions/{functionName}",
            jsonTypeInfo: FunctionContext.Default.FunctionDefinition,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Triggers a specific function.
    /// </summary>
    /// <param name="functionName">The function name.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that returns the HTTP response from the trigger request.</returns>
    public Task<HttpResponseMessage> TriggerFunction(string functionName, CancellationToken cancellationToken = default)
    {
        return HttpClient.PostAsJsonAsync(
            requestUri: $"admin/functions/{functionName}",
            value: FunctionTrigger.Empty,
            jsonTypeInfo: FunctionContext.Default.FunctionTrigger,
            cancellationToken: cancellationToken);
    }
}
