using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Arbiter.Communication.Email;

/// <summary>
/// Represents configuration options for email delivery, including sender information, SMTP server settings, and template resources.
/// </summary>
public class EmailConfiguration
{
    /// <summary>
    /// Gets or sets the display name of the sender.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Gets or sets the sender's email address.
    /// </summary>
    /// <remarks>
    /// This value is required for sending emails.
    /// </remarks>
    [Required]
    public string FromAddress { get; set; } = null!;

    /// <summary>
    /// Gets or sets the SMTP server address used for sending emails.
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Gets or sets the port number for the SMTP server.
    /// </summary>
    /// <remarks>
    /// The default value is 465, which is commonly used for SMTP over SSL.
    /// </remarks>
    public int Port { get; set; } = 465;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL when connecting to the SMTP server.
    /// </summary>
    /// <remarks>
    /// The default value is <c>true</c>.
    /// </remarks>
    public bool UseSSL { get; set; } = true;

    /// <summary>
    /// Gets or sets the user name for authenticating with the SMTP server.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the password for authenticating with the SMTP server.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the assembly containing embedded email templates.
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
    /// <returns>The current <see cref="EmailConfiguration"/> instance for chaining.</returns>
    public EmailConfiguration AddTemplateAssembly<T>(string? templateResourceFormat = null)
    {
        TemplateAssembly = typeof(T).Assembly;

        if (templateResourceFormat is not null)
            TemplateResourceFormat = templateResourceFormat;

        return this;
    }
}
