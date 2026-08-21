using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Handlers;
using Arbiter.CommandQuery.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arbiter.Health.Handlers;

/// <summary>
/// Handles <see cref="ConfigurationQuery"/> requests and returns flattened configuration key/value entries with provider information.
/// </summary>
public class ConfigurationQueryHandler : RequestHandlerBase<ConfigurationQuery, IReadOnlyList<ConfigurationValue>>
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationQueryHandler"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create the handler logger.</param>
    /// <param name="configuration">The application configuration source.</param>
    public ConfigurationQueryHandler(
        ILoggerFactory loggerFactory,
        IConfiguration configuration) : base(loggerFactory)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Processes a <see cref="ConfigurationQuery"/> and returns configuration entries ordered by key.
    /// </summary>
    /// <param name="request">The configuration query request.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A read-only list of configuration values with provider metadata.</returns>
    protected override ValueTask<IReadOnlyList<ConfigurationValue>?> Process(
        ConfigurationQuery request,
        CancellationToken cancellationToken = default)
    {
        var items = new List<ConfigurationValue>();
        if (_configuration is not IConfigurationRoot configurationRoot)
        {
            Logger.LogWarning("Configuration is not an IConfigurationRoot. Unable to retrieve provider information.");
            return ValueTask.FromResult<IReadOnlyList<ConfigurationValue>?>(items);
        }

        void RecurseChildren(IEnumerable<IConfigurationSection> children)
        {
            foreach (var child in children)
            {
                var (value, provider) = GetValueAndProvider(configurationRoot, child.Path);
                if (provider != null)
                {
                    ConfigurationValue providerValue = new()
                    {
                        Key = child.Path,
                        Value = value,
                        Provider = provider.ToString(),
                    };
                    items.Add(providerValue);
                }

                RecurseChildren(child.GetChildren());
            }
        }

        RecurseChildren(configurationRoot.GetChildren());

        IReadOnlyList<ConfigurationValue>? item = [.. items.OrderBy(p => p.Key, StringComparer.Ordinal)];
        return ValueTask.FromResult<IReadOnlyList<ConfigurationValue>?>(item);
    }

    /// <summary>
    /// Resolves the effective value and the configuration provider responsible for a given key.
    /// </summary>
    /// <param name="root">The configuration root containing all providers.</param>
    /// <param name="key">The configuration key to resolve.</param>
    /// <returns>A tuple containing the resolved value and the provider that supplied it.</returns>
    private static (string? Value, IConfigurationProvider? Provider) GetValueAndProvider(
        IConfigurationRoot root,
        string key)
    {
        // Iterate through the configuration providers in reverse order to find the last provider that has the key.
        foreach (var provider in root.Providers.Reverse())
        {
            if (provider.TryGet(key, out var value))
                return (value, provider);
        }

        return (null, null);
    }

}
