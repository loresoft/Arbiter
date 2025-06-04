using System.Diagnostics.CodeAnalysis;
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
    /// <param name="source">The template source as a string. If <see langword="null"/>, an empty string is returned.</param>
    /// <param name="model">The model to bind to the template. May be <see langword="null"/> if the template does not require data.</param>
    /// <returns>
    /// The rendered template as a string. If <paramref name="source"/> is <see langword="null"/>, returns an empty string.
    /// </returns>
    /// <remarks>
    /// Implementations should apply the provided model to the template source and return the resulting string.
    /// </remarks>
    string ApplyTemplate<TModel>(string? source, TModel model);

    /// <summary>
    /// Retrieves an embedded resource template from the specified assembly and resource name.
    /// </summary>
    /// <typeparam name="TTemplate">The type to which the resource template should be deserialized or cast.</typeparam>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourceName">The name of the embedded resource.</param>
    /// <param name="template">
    /// When this method returns, contains the resource template as <typeparamref name="TTemplate"/> if found and successfully loaded; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource template was found and loaded successfully; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Implementations should attempt to locate and load the specified embedded resource from the given assembly.
    /// </remarks>
    bool TryGetResourceTemplate<TTemplate>(
        Assembly assembly,
        string resourceName,
        [NotNullWhen(true)] out TTemplate? template);
}
