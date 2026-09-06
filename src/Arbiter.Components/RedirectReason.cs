namespace Arbiter.Components;

/// <summary>
/// Describes why a page is navigating away from the current model.
/// </summary>
/// <remarks>
/// Used to select the location to navigate to so that each outcome can redirect to a different page.
/// </remarks>
public enum RedirectReason
{
    /// <summary>
    /// The requested model could not be found.
    /// </summary>
    NotFound = 0,

    /// <summary>
    /// The model was saved and was assigned a different identifier, which happens when a new model is created.
    /// </summary>
    Created = 1,

    /// <summary>
    /// The pending changes to the model were discarded.
    /// </summary>
    Canceled = 2,

    /// <summary>
    /// The model was deleted.
    /// </summary>
    Deleted = 3,
}
