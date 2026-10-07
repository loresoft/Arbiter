using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Options;

/// <summary>
/// Binds <see cref="EnvironmentOptions"/> from <see cref="IConfiguration"/>.
/// </summary>
internal sealed class EnvironmentOptionsSetup : IConfigureOptions<EnvironmentOptions>
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentOptionsSetup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to bind <see cref="EnvironmentOptions"/> from.</param>
    public EnvironmentOptionsSetup(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuration = configuration;
    }

    /// <inheritdoc />
    public void Configure(EnvironmentOptions options)
    {
        _configuration.Bind(options);
    }
}
