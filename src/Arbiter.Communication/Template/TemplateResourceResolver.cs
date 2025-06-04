using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Arbiter.Communication.Template;

/// <summary>
/// Resolves templates from embedded resources within a specified assembly using a resource name format.
/// </summary>
public class TemplateResourceResolver : ITemplateResolver
{
    private readonly Assembly _resourceAssembly;
    private readonly string _resourceNameFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateResourceResolver"/> class.
    /// </summary>
    /// <param name="resourceAssembly">The assembly containing the embedded resource templates.</param>
    /// <param name="resourceNameFormat">
    /// The format string used to construct the resource name, typically using <c>string.Format</c> with the template name. <c>Arbiter.Communication.Templates.{0}.yaml</c> is a common example.
    /// </param>
    /// <param name="priority">
    /// The priority processing order for this resolver. Lower values indicate higher priority and process first. Defaults to 9999.
    /// </param>
    public TemplateResourceResolver(Assembly resourceAssembly, string resourceNameFormat, int priority = 9999)
    {
        ArgumentNullException.ThrowIfNull(resourceAssembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceNameFormat);

        _resourceAssembly = resourceAssembly;
        _resourceNameFormat = resourceNameFormat;
        Priority = priority;
    }

    /// <summary>
    /// Gets the priority processing order for this resolver. Lower values indicate higher priority and process first.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Attempts to resolve a template by name from the embedded resources and deserialize it to the specified type.
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
    /// Uses YamlDotNet to deserialize the embedded resource. The resource name is constructed using the provided format string.
    /// </remarks>
    public bool TryResolveTemplate<TTemplate>(
        string templateName,
        [NotNullWhen(true)] out TTemplate? template)
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);

        var resourceName = string.Format(_resourceNameFormat, templateName);

        using var stream = _resourceAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            template = default;
            return false;
        }

        using var reader = new StreamReader(stream);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        template = deserializer.Deserialize<TTemplate>(reader);
        return template is not null;
    }
}
