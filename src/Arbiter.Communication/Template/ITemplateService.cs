using System.Diagnostics.CodeAnalysis;

namespace Arbiter.Communication.Template;

/// <summary>
/// Provides methods for applying templates to models and retrieving named templates.
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
    /// If <paramref name="model"/> is <see langword="null"/>, returns the original <paramref name="source"/>.
    /// </returns>
    /// <remarks>
    /// Implementations should apply the provided model to the template source and return the resulting string.
    /// If the model is <see langword="null"/>, the template is returned as-is.
    /// </remarks>
    string ApplyTemplate<TModel>(string? source, TModel model);

    /// <summary>
    /// Retrieves a template by name and attempts to deserialize or cast it to the specified type.
    /// </summary>
    /// <typeparam name="TTemplate">The type to which the template should be deserialized or cast.</typeparam>
    /// <param name="templateName">The name or key of the template to retrieve.</param>
    /// <param name="template">
    /// When this method returns, contains the template as <typeparamref name="TTemplate"/> if found and successfully loaded; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the template was found and loaded successfully; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Implementations should attempt to locate and load the specified template by name, which may refer to an embedded resource or other storage.
    /// </remarks>
    bool TryGetTemplate<TTemplate>(
        string templateName,
        [NotNullWhen(true)] out TTemplate? template);
}
