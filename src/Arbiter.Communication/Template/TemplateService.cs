using System.Reflection;

using Fluid;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Arbiter.Communication.Template;

/// <summary>
/// Provides services for applying text templates to models and retrieving embedded resource templates.
/// </summary>
/// <remarks>
/// This service uses Fluid for template parsing and rendering, and YamlDotNet for deserializing resource templates.
/// </remarks>
public class TemplateService : ITemplateService
{
    private readonly FluidParser _fluidParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateService"/> class.
    /// </summary>
    /// <param name="fluidParser">The Fluid parser used for template parsing and rendering.</param>
    public TemplateService(FluidParser fluidParser)
    {
        _fluidParser = fluidParser;
    }

    /// <summary>
    /// Applies the specified template source to the given model and returns the rendered result.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to use for template binding.</typeparam>
    /// <param name="source">The template source as a string. If <see langword="null"/> or whitespace, an empty string is returned.</param>
    /// <param name="model">The model to bind to the template.</param>
    /// <returns>The rendered template as a string.</returns>
    public string ApplyTemplate<TModel>(string? source, TModel model)
    {
        if (string.IsNullOrEmpty(source))
            return string.Empty;

        if (model is null)
            return source ?? string.Empty;

        var options = new TemplateOptions();
        options.MemberAccessStrategy = new UnsafeMemberAccessStrategy();

        var context = new TemplateContext(model, options, allowModelMembers: true);

        var template = _fluidParser.Parse(source);
        return template.Render(context);
    }

    /// <summary>
    /// Retrieves an embedded resource template from the specified assembly and resource name, and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="TTemplate">The type to which the resource template should be deserialized.</typeparam>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourceName">The name of the embedded resource.</param>
    /// <returns>
    /// The resource template as <typeparamref name="TTemplate"/> if found and successfully loaded; otherwise, <see langword="null"/>.
    /// </returns>
    public TTemplate? GetResourceTemplate<TTemplate>(Assembly assembly, string resourceName)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrEmpty(resourceName);

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return default;

        using var reader = new StreamReader(stream);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<TTemplate>(reader);
    }
}
