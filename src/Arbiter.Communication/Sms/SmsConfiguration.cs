using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Arbiter.Communication.Sms;

/// <summary>
/// Represents configuration options for SMS delivery, including sender information and template resources.
/// </summary>
public class SmsConfiguration
{
    /// <summary>
    /// Represents the configuration name used for SMS-related settings.
    /// </summary>
    public const string ConfigurationName = "Sms";

    /// <summary>
    /// Gets or sets the sender's phone number in E.164 format (e.g., +15554441234).
    /// </summary>
    [Required]
    public string SenderNumber { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user name for authenticating with the SMS delivery service.
    /// </summary>
    /// <remarks>
    /// For Twilio, this is the Account SID.
    /// </remarks>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the password for authenticating with the SMS delivery service.
    /// </summary>
    /// <remarks>
    /// For Twilio, this is the Auth Token.
    /// </remarks>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the service key used to authenticate or identify the service.
    /// </summary>
    public string? ServiceKey { get; set; }

    /// <summary>
    /// Gets or sets the assembly containing embedded SMS templates.
    /// </summary>
    /// <remarks>
    /// If specified, templates will be loaded from this assembly's resources.
    /// </remarks>
    public Assembly? TemplateAssembly { get; set; }

    /// <summary>
    /// Gets or sets the format string used to locate template resources within the assembly.
    /// </summary>
    /// <remarks>
    /// This format is typically used with <see cref="string.Format(string, object?)"/> to resolve the resource name for a template.
    /// </remarks>
    /// <example>
    /// Example format: "Arbiter.CommandQuery.Templates.{0}.yaml"
    /// </example>
    public string? TemplateResourceFormat { get; set; }

    /// <summary>
    /// Sets the <see cref="TemplateAssembly"/> to the assembly containing the specified type <typeparamref name="T"/>,
    /// and optionally sets the <see cref="TemplateResourceFormat"/>.
    /// </summary>
    /// <typeparam name="T">A type from the assembly to use for template resources.</typeparam>
    /// <param name="templateResourceFormat">
    /// An optional format string for locating template resources within the assembly.
    /// If provided, sets <see cref="TemplateResourceFormat"/>.
    /// </param>
    /// <returns>The current <see cref="SmsConfiguration"/> instance for chaining.</returns>
    public SmsConfiguration AddTemplateAssembly<T>(string? templateResourceFormat = null)
    {
        TemplateAssembly = typeof(T).Assembly;

        if (templateResourceFormat is not null)
            TemplateResourceFormat = templateResourceFormat;

        return this;
    }
}
