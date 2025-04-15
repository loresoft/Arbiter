namespace Arbiter.CommandQuery.Dispatcher;

/// <summary>
/// Options for the dispatcher.
/// </summary>
public class DispatcherOptions
{
    /// <summary>
    /// The prefix for the feature routes.
    /// </summary>
    public string FeaturePrefix { get; set; } = "/api";

    /// <summary>
    /// The prefix for the dispatcher routes.
    /// </summary>
    public string DispatcherPrefix { get; set; } = "/dispatcher";

    /// <summary>
    /// The route for the send method.
    /// </summary>
    public string SendRoute { get; set; } = "/send";
}
