using System.ComponentModel.DataAnnotations;

namespace Arbiter.Communication.Email;

/// <summary>
/// Represents configuration options for email delivery, including sender information and SMTP server settings.
/// </summary>
public class EmailConfiguration
{
    /// <summary>
    /// Represents the configuration name used for email-related settings.
    /// </summary>
    public const string ConfigurationName = "Email";

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
    /// The default value is <see langword="true"/>.
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
    /// Gets or sets the service key used to authenticate or identify the service.
    /// </summary>
    public string? ServiceKey { get; set; }
}
