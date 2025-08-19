using System.ComponentModel.DataAnnotations;

namespace Arbiter.Communication.Sms;

/// <summary>
/// Represents configuration options for SMS delivery, including sender information.
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
}
