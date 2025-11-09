using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Arbiter.Communication.Extensions;

using Fluid;

namespace Arbiter.Communication.Template;

/// <summary>
/// Provides services for applying text templates to models and retrieving named templates using registered template resolvers.
/// </summary>
/// <remarks>
/// This service uses Fluid for template parsing and rendering, and supports extensible template resolution via registered <see cref="ITemplateResolver"/> implementations.
/// </remarks>
public class TemplateService : ITemplateService
{
    private readonly ConcurrentDictionary<string, IFluidTemplate> _templateCache = [];
    private readonly FluidParser _fluidParser;
    private readonly IReadOnlyList<ITemplateResolver> _templateResolvers;
    private readonly TemplateOptions _templateOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateService"/> class.
    /// </summary>
    /// <param name="fluidParser">The Fluid parser used for template parsing and rendering.</param>
    /// <param name="templateResolvers">A collection of template resolvers for locating and loading templates by name.</param>
    /// <param name="templateOptions">The options to use when rendering templates.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="fluidParser"/> or <paramref name="templateResolvers"/> is <see langword="null"/>.
    /// </exception>
    public TemplateService(FluidParser fluidParser, IEnumerable<ITemplateResolver> templateResolvers, TemplateOptions templateOptions)
    {
        ArgumentNullException.ThrowIfNull(fluidParser);
        ArgumentNullException.ThrowIfNull(templateResolvers);

        _fluidParser = fluidParser;
        _templateResolvers = [.. templateResolvers.OrderBy(p => p.Priority)];
        _templateOptions = templateOptions;
    }

    /// <summary>
    /// Applies the specified template source to the given model and returns the rendered result.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to use for template binding.</typeparam>
    /// <param name="source">The template source as a string. If <see langword="null"/> or whitespace, an empty string is returned.</param>
    /// <param name="model">The model to bind to the template. May be <see langword="null"/> if the template does not require data.</param>
    /// <returns>
    /// The rendered template as a string. If <paramref name="source"/> is <see langword="null"/> or whitespace, returns an empty string.
    /// If <paramref name="model"/> is <see langword="null"/>, returns the original <paramref name="source"/>.
    /// </returns>
    /// <remarks>
    /// Uses Fluid to parse and render the template. If the model is <see langword="null"/>, the template is returned as-is.
    /// </remarks>
    public string ApplyTemplate<TModel>(string? source, TModel model)
    {
        if (string.IsNullOrEmpty(source))
            return string.Empty;

        if (model is null)
            return source ?? string.Empty;

        // cache parsed templates by hash of source
        var hash = source.ToXXHash3();
        var template = _templateCache.GetOrAdd(hash, _ => _fluidParser.Parse(source));

        var context = new TemplateContext(model, _templateOptions, allowModelMembers: true);
        return template.Render(context);
    }

    /// <summary>
    /// Retrieves a template by name and attempts to deserialize or cast it to the specified type using registered template resolvers.
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
    /// Attempts to resolve the template using all registered <see cref="ITemplateResolver"/> instances.
    /// Returns <see langword="true"/> on the first successful resolution; otherwise, <see langword="false"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if <paramref name="templateName"/> is <see langword="null"/> or empty.</exception>
    public bool TryGetTemplate<TTemplate>(
        string templateName,
        [NotNullWhen(true)] out TTemplate? template)
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);

        // try each registered resolver until one succeeds
        for (int i = 0; i < _templateResolvers.Count; i++)
        {
            if (_templateResolvers[i].TryResolveTemplate(templateName, out template))
                return true;
        }

        template = default;
        return false;
    }
}
