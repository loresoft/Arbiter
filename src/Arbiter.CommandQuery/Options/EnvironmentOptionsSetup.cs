using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Options;

/// <summary>
/// Binds <see cref="EnvironmentOptions"/> from <see cref="IConfiguration"/>, using <see cref="IHostEnvironment"/> for defaults when available.
/// </summary>
internal sealed class EnvironmentOptionsSetup : IConfigureOptions<EnvironmentOptions>
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment? _hostEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentOptionsSetup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to bind <see cref="EnvironmentOptions"/> from.</param>
    /// <param name="hostEnvironment">The optional host environment used to provide default values.</param>
    public EnvironmentOptionsSetup(IConfiguration configuration, IHostEnvironment? hostEnvironment = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc />
    public void Configure(EnvironmentOptions options)
    {
        // host environment provides the default, explicit configuration values take precedence
        if (!string.IsNullOrEmpty(_hostEnvironment?.EnvironmentName))
            options.EnvironmentName = _hostEnvironment.EnvironmentName;

        _configuration.Bind(options);
    }
}
