namespace Arbiter.Functions;

/// <summary>
/// Represents an empty payload used to trigger an Azure Function through the admin API.
/// </summary>
public record FunctionTrigger
{
    /// <summary>
    /// Gets an empty function trigger payload instance.
    /// </summary>
    public static FunctionTrigger Empty { get; } = new();
}
