using System.Diagnostics.CodeAnalysis;

namespace Arbiter.Communication.Template;

/// <summary>
/// Defines a contract for resolving templates by name from various sources (such as embedded resources, files, or other storage).
/// </summary>
public interface ITemplateResolver
{
    /// <summary>
    /// Gets the priority processing order for this resolver.  Lower values indicate higher priority and process first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Attempts to resolve a template by name and deserialize it to the specified type.
    /// </summary>
    /// <typeparam name="TTemplate">The type to which the template should be deserialized.</typeparam>
    /// <param name="templateName">The name or key of the template to resolve.</param>
    /// <param name="template">
    /// When this method returns, contains the template as <typeparamref name="TTemplate"/> if found and successfully loaded; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the template was found and loaded successfully; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Implementations should attempt to locate and load the specified template by name, which may refer to an embedded resource, file, or other storage mechanism.
    /// </remarks>
    bool TryResolveTemplate<TTemplate>(
        string templateName,
        [NotNullWhen(true)] out TTemplate? template);
}
