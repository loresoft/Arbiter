namespace Arbiter.Communication.Email;

/// <summary>
/// Represents the recipients of an email, including To, Cc, and Bcc addresses.
/// </summary>
/// <param name="To">The primary recipients of the email.</param>
/// <param name="Cc">The optional carbon copy recipients.</param>
/// <param name="Bcc">The optional blind carbon copy recipients.</param>
public readonly record struct EmailRecipients(
    IReadOnlyList<EmailAddress> To,
    IReadOnlyList<EmailAddress>? Cc = null,
    IReadOnlyList<EmailAddress>? Bcc = null
);
