namespace Arbiter.Communication.Email;

/// <summary>
/// Represents an email template with subject, HTML body, and optional plain text body.
/// </summary>
/// <param name="Subject">The subject of the email.</param>
/// <param name="HtmlBody">The HTML body content of the email.</param>
/// <param name="TextBody">The optional plain text body content of the email.</param>
public readonly record struct EmailTemplate(
    string Subject,
    string HtmlBody,
    string? TextBody = null
);
