namespace Arbiter.Communication.Email;

/// <summary>
/// Represents the content of an email, including subject, HTML body, and optional plain text body.
/// </summary>
/// <param name="Subject">The subject of the email.</param>
/// <param name="HtmlBody">The HTML body content of the email.</param>
/// <param name="TextBody">The optional plain text body content of the email.</param>
public readonly record struct EmailContent(
    string Subject,
    string HtmlBody,
    string? TextBody = null
);
