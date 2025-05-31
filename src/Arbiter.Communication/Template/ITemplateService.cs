using System.Reflection;

namespace Arbiter.Communication.Template;

/// <summary>
/// Provides methods for applying templates to models and retrieving embedded resource templates.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Applies the specified template source to the given model and returns the rendered result.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to use for template binding.</typeparam>
    /// <param name="source">The template source as a string. If <c>null</c>, an empty string is returned.</param>
    /// <param name="model">The model to bind to the template.</param>
    /// <returns>The rendered template as a string.</returns>
    string ApplyTemplate<TModel>(string? source, TModel model);

    /// <summary>
    /// Retrieves an embedded resource template from the specified assembly and resource name.
    /// </summary>
    /// <typeparam name="TTemplate">The type to which the resource template should be deserialized or cast.</typeparam>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourceName">The name of the embedded resource.</param>
    /// <returns>
    /// The resource template as <typeparamref name="TTemplate"/> if found and successfully loaded; otherwise, <c>null</c>.
    /// </returns>
    TTemplate? GetResourceTemplate<TTemplate>(Assembly assembly, string resourceName);
}
