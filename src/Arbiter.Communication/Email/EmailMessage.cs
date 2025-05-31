namespace Arbiter.Communication.Email;

/// <summary>
/// Represents a complete email message, including senders, recipients, content, optional headers, and attachments.
/// </summary>
/// <param name="Senders">The sender and optional reply-to addresses.</param>
/// <param name="Recipients">The recipients of the email.</param>
/// <param name="Content">The content of the email (subject, HTML, and text bodies).</param>
/// <param name="Headers">Optional custom headers for the email.</param>
/// <param name="Attachments">Optional collection of attachments.</param>
public readonly record struct EmailMessage(
    EmailSenders Senders,
    EmailRecipients Recipients,
    EmailContent Content,
    IReadOnlyDictionary<string, string>? Headers = null,
    IReadOnlyList<EmailAttachment>? Attachments = null
);
